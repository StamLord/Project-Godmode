using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrashState : State
{
    private AdvancedController cr;
    private ThirdPersonCam camScript;
    private Animator anim;

    [Header("Settings")]
    public LayerMask crashColMask;
    public Vector3 direction;

    public float crashSpeed;
    public float gravity;

    public float destructionRadius = 1f;
    public float destructionForce = 1f;

    [Header("Animation")]
    public string animState = "Crashing";
    public float transitionSpeed = 0.1f;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        cr = Machine.cr;
        camScript = Machine.camScript;

        anim = Machine.anim;
        anim.CrossFade(animState, transitionSpeed);

        direction = cr.lastVector;
    }

    void Update()
    {
        Vector3 vectorGrav = new Vector3(0, gravity, 0);

        PlayerCharacterInputs inputs = new PlayerCharacterInputs();
        inputs.motion = (direction * crashSpeed) + vectorGrav;
        inputs.maxSpeed = crashSpeed;

        cr.SetInputs(inputs);
        
        if(GroundCheck())
        {
            Machine.layingFaceDown = true;
            Machine.SetState<LayingState>();
        }

        CheckCollision();

    }

    private void CheckCollision()
    {
        RaycastHit h;
        if (Physics.SphereCast(transform.position, .5f, direction.normalized, out h, 1f, crashColMask))
        {
            Destructable d = h.collider.GetComponent<Destructable>();
            if (d)
            {
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

    private bool GroundCheck()
    {
        return Machine.groundCheck.grounded;
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
        Machine.crashDirection = Vector3.zero;
    }
}
