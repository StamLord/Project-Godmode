using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashState : State
{
    protected ThirdPersonCam camScript;
    protected VirtualInput vi;
    protected CharacterController cr;
    protected TargetingSystem ts;
    protected Animator anim;
    protected TechManager techManager;

    [Header("Settings")]
    public float staminaDepleteRate = 100f;
    protected float staminaTimer;
    public float moveSpeed = 15f;
    public float destructionRadius = 1f;
    public float destructionForce = 1f;

    protected Vector3 lastInputVector;

    [Header("Melee")]
    public bool isChargingTech;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        vi = Machine.vi;
        cr = Machine.cr;
        ts = Machine.ts;
        anim = Machine.anim;
        anim.SetBool("Dashing", true);
        camScript = Machine.camScript;
        camScript.SetMaxFov(true);
        techManager = Machine.techManager;
    }

    void Update()
    {
        if (GroundCheck() && vi.lShift)
            Machine.SetState<GroundedState>();

        InputCheck();
        StaminaDeplete();
        AnimationUpdate();
    }

    private bool GroundCheck()
    {
        return Machine.groundCheck.grounded;
    }

    private void InputCheck()
    {
        #region Mouse Input

        if (vi.lmb)
        {
            MousePressMain();
        }
        else if (vi.lmbUp)
        {
            MouseReleaseMain();
        }

        if (vi.rmb)
        {
            MousePressSecondary();
        }
        else if (vi.rmbUp)
        {
            MouseReleaseSecondary();
        }

        #endregion

        if (anim.GetInteger("Combo") == 0)
        {
            #region Movement Input

            float inputX = vi.horizontal;
            float inputZ = vi.vertical;

            Vector3 inputVec;

            //If no target, create from own forward and right
            if (ts.lockOn == false)
            {
                inputVec = (transform.forward * inputZ) + (transform.right * inputX);
                inputVec = inputVec.normalized;
            }
            else //Find the direction to the target (clamped to magnitude of 1)
            {
                Vector3 dirToTarget = ts.bodyCenter.transform.position - transform.position;
                dirToTarget = dirToTarget / dirToTarget.magnitude;

                float angleZX = Mathf.Atan2(dirToTarget.z, dirToTarget.x);
                float zLength = Mathf.Sin(angleZX);
                float xLength = Mathf.Cos(angleZX);

                float angleZY = Mathf.Atan2(dirToTarget.z, dirToTarget.y);
                float yLength = Mathf.Cos(angleZY);

                Vector3 newDir = new Vector3(xLength, yLength, zLength);

                inputVec = (newDir * inputZ) + (transform.right * inputX);
            }

            if (vi.space)
            {
                inputVec += Vector3.up;
            }

            if (vi.lShift)
            {
                inputVec -= Vector3.up;
            }

            if (inputVec == Vector3.zero)
            {
                Machine.SetState<FlyingState>();
            }
            else
            {
                lastInputVector = (inputVec * moveSpeed) * Time.deltaTime;
            }

            Movement(inputVec);

            #endregion

            #region Fly Key

            if (vi.fDown)
            {
                Machine.SetState<FallingState>();
            }

            #endregion

            #region Charge Key

            if (vi.e)
            {
                Machine.SetState<ChargeState>();
            }
            #endregion

            #region Guard Key

            if (vi.q)
            {
                Machine.SetState<FlyingState>();
            }
            #endregion
        }

        DestructionSphere();
    }

    private void Movement(Vector3 direction)
    {
        cr.Move(((direction * moveSpeed)) * Time.deltaTime);
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

    private void StaminaDeplete()
    {
        if(staminaTimer >= 1f / staminaDepleteRate)
        {
            Machine.stamina--;
            staminaTimer = 0f;

            if (Machine.stamina <= 0)
            {
                if(Machine.groundCheck.grounded)
                    Machine.SetState<GroundedState>();
                else
                    Machine.SetState<FlyingState>();
            }
        }
        staminaTimer += Time.deltaTime;

    }

    private void MousePressMain()
    {
        techManager.MousePressMain();
    }

    private void MouseReleaseMain()
    {
        techManager.MouseReleaseMain();
    }

    private void MousePressSecondary()
    {

    }

    private void MouseReleaseSecondary()
    {

    }

    private void TechCharge()
    {
        Technique t = techManager.GetSelected;

        //Animations
        if (t.type == HitType.Melee)
        {
            if (techManager.techChargeTimer > 0.25f)
            {
                if (vi.space)
                {
                    anim.SetBool("ChargeKick", true);
                    anim.SetBool("ChargePunch", false);
                }
                else
                {
                    anim.SetBool("ChargePunch", true);
                    anim.SetBool("ChargeKick", false);
                }
            }
        }
        else if (anim.GetBool("ChargingAttack") == false)
        {
            AnimateCharge(t);

            if (t.type == HitType.Beam && techManager.techChargeTimer > 0.1f)
                if (camScript && camScript.view != ThirdPersonCam.camView.RightZoomBeam)
                    camScript.TransitionView(ThirdPersonCam.camView.RightZoomBeam);
        }

        techManager.TechCharge();
    }

    private void ExitTechCharge()
    {
        isChargingTech = false;

        anim.SetBool("ChargingAttack", false);
        anim.SetBool("ChargePunch", false);
        anim.SetBool("ChargeKick", false);

        if (camScript && camScript.view != ThirdPersonCam.camView.InstantFront)
            camScript.TransitionView(ThirdPersonCam.camView.TransitionFront);

        techManager.ExitTechCharge();

    }

    private void AnimationUpdate()
    {
        anim.SetFloat("Speed", vi.vertical);
    }

    void AnimateCharge(Technique t)
    {
        anim.SetInteger("ChargeAnim", t.chargeAnimation);
        anim.SetBool("ChargingAttack", true);
    }

    private void AnimateAttack(Technique t)
    {
        techManager.attackAnimating = t;

        anim.SetInteger("AttackAnim", t.attackAnimation);

        if (anim.GetBool("ChargingAttack"))
            anim.SetBool("ChargingAttack", false);

        anim.SetBool("FiringAttack", true);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        anim.SetBool("Dashing", false);
        Machine.lastVector = lastInputVector;
        camScript.SetMaxFov(false);
    }
}
