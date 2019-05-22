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
    public Vector3 currentVector;

    public AnimationCurve tossSpeed;
    protected float curveLength;

    public float tossTimer;
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
        currentVector = direction;

        curveLength = tossSpeed.keys[tossSpeed.length - 1].time;

        transform.forward = -direction;
    }

    void Update()
    {
        float speed = tossSpeed.Evaluate(tossTimer);
        currentVector = direction * speed;

        PlayerCharacterInputs inputs = new PlayerCharacterInputs();
        inputs.motion = currentVector;

        cr.SetInputs(inputs);
        //Movement(currentVector);

        AnimationUpdate();

        if(vi.lmbDown)
        {
            tossTimer += 0.02f * curveLength;
        }

        //DestructionSphere();

        if (tossTimer >= curveLength * 0.75f)
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
                tossTimer += 0.2f * curveLength;
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
        anim.SetFloat("TossMag", currentVector.magnitude);
    }

    private bool GroundCheck()
    {
        return Machine.groundCheck.grounded;
    }

    private void Movement(Vector3 direction)
    {
        
        //cr.Move(direction * Time.deltaTime);
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
        tossTimer = 0f;
        anim.SetBool("Tossed", false);
        Machine.tossDirection = Vector3.zero;
        Machine.lastVector = currentVector;
        currentVector = Vector3.zero;
    }

}
