using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TossState : State
{
    private AdvancedController cr;
    private ThirdPersonCam camScript;
    private VirtualInput vi;
    private Animator anim;

    [Header("Settings")]
    public Vector3 attackPoint;
    public LayerMask tossColMask;
    public Vector3 direction;
    public float duration;
    public float destructionRadius = 1f;
    public float destructionForce = 1f;
    private float speed;
    [SerializeField] private float tossTimer;

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
        inputs.lookAt = attackPoint;

        cr.SetInputs(inputs);

        if(vi.lmbDown)
        {
            tossTimer += 0.02f * duration;
        }

        if (tossTimer >= duration)
        {
            if(GroundCheck())
                Machine.SetState<GroundedState>();
            else if (Machine.canFly)
                Machine.SetState<FlyingState>();
            else
                Machine.SetState<FallingState>();
        }

        CollideCheck();

        tossTimer += Time.deltaTime;
    }

    void CollideCheck()
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
                ImpactManager.instance.Create(transform.position, Vector3.up);
                anim.SetTrigger("Impact");
                Machine.SetState<GroundedState>();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position + direction.normalized, .5f);
    }

    private bool GroundCheck()
    {
        return cr.grounded;
    }

    [System.Obsolete("This state only destroys objects it collided with at the moment.")]
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
