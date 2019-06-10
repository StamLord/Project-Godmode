using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TossState : State
{
    protected AdvancedController cr;
    protected ThirdPersonCam camScript;
    protected VirtualInput vi;
    protected Animator anim;

    public LayerMask tossColMask;
    public Vector3 direction;
    private float speed;

    public AnimationCurve tossSpeed;

    public float duration;
    [SerializeField] private float tossTimer;
    public float destructionRadius = 1f;
    public float destructionForce = 1f;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        cr = Machine.cr;
        camScript = Machine.camScript;
        vi = Machine.vi;
        anim = Machine.anim;
        anim.SetBool("Tossed", true);

        direction = Machine.tossDirection;
        speed = direction.magnitude;

        tossTimer = 0f;
    }

    void Update()
    {
        PlayerCharacterInputs inputs = new PlayerCharacterInputs();
        inputs.motion = direction;
        inputs.maxSpeed = speed;
        inputs.ignoreOrientation = true;

        cr.SetInputs(inputs);

        AnimationUpdate();

        if(vi.lmbDown)
        {
            tossTimer += 0.02f * duration;
        }

        //DestructionSphere();

        if (tossTimer >= duration)
        {
            if(GroundCheck())
                Machine.SetState<GroundedState>();
            else if (Machine.canFly)
                Machine.SetState<FlyingState>();
            else
                Machine.SetState<FallingState>();
        }

        tossTimer += Time.deltaTime;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        RaycastHit h;
        if (Physics.SphereCast(transform.position, .5f, direction.normalized, out h, 1f, tossColMask))
        {
            Destructable d = h.collider.GetComponent<Destructable>();
            if (d)
            {
                tossTimer += 0.2f * duration;
                d.Destruction(direction, destructionForce);
            }
            else
            {
                ImpactManager.instance.Create(transform.position, -direction);
                anim.SetTrigger("Impact");
                Machine.SetState<GroundedState>();
            }
        }
    }

    void AnimationUpdate()
    {
        anim.SetFloat("TossMag", speed);
    }

    private bool GroundCheck()
    {
        return cr.grounded;
    }

    void DestructionSphere()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, destructionRadius);

        foreach (Collider c in colliders)
        {
            if (c.CompareTag("Projectile"))
                continue;

            Destructable d = c.GetComponent<Destructable>();
            Rigidbody r = (c.transform != transform) ? c.GetComponent<Rigidbody>() : null;
            if (d)
            {
                d.Destruction(transform.forward, destructionForce);
                camScript.StartShake(.75f, true);
            }

            if (r)
            {
                Vector3 force = (c.transform.position - transform.position).normalized * 10f;
                r.AddForce(force, ForceMode.Impulse);
                if (!c.CompareTag("Player"))
                    if (camScript) camScript.StartShake(.75f, true);
            }
        }
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        
        anim.SetBool("Tossed", false);
        Machine.tossDirection = Vector3.zero;
    }

}
