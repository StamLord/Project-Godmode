using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunState : State
{
    protected ThirdPersonCam camScript;
    protected VirtualInput vi;
    protected AdvancedController cr;
    protected TargetingSystem ts;
    protected Animator anim;
    protected TechManager techManager;

    [Header("Settings")]
    public LayerMask runnableMask;
    public float moveSpeed = 15f;
    public float wallRunDuration = 3f;
    protected float wallrunTimer;
    public AnimationCurve ySpeed;


    public float decelTime = 0.5f;
    protected float decelTimer;
    public Vector3 lastInputVector;
    public Vector3 currentVector;

    public RaycastHit currentWall;

    [Header("Melee")]
    public bool isChargingTech;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        camScript = Machine.camScript;
        vi = Machine.vi;
        cr = Machine.cr;
        ts = Machine.ts;
        anim = Machine.anim;
        
        techManager = Machine.techManager;

        lastInputVector = Machine.lastVector;

        currentWall = Machine.wallToRun;
        Machine.wallToRun = new RaycastHit();

        //Vector along wall
        Vector3 along = Vector3.Cross(currentWall.normal, Vector3.up);

        //Find Angle
        float angle = Vector3.Angle(transform.forward, along);

        //Face along wall
        if (angle < 90)
            anim.SetBool("LeftWallrun", true);
        else
            anim.SetBool("RightWallrun", true);
        Debug.Log("entered");
    }

    public override void OnStateInitialize(StateMachine machine = null)
    {
        base.OnStateInitialize(machine);
    }

    private void Update()
    {
        currentWall = RunnableWallCheck();

        if (currentWall.collider == null || wallrunTimer >= wallRunDuration)
        {
            Machine.SetState<FallingState>();
        }

        InputCheck();

        wallrunTimer += Time.deltaTime;

    }

    private RaycastHit RunnableWallCheck()
    {
        RaycastHit left;
        RaycastHit right;

        Physics.Raycast(transform.position, -transform.right, out left, 1f, runnableMask);
        Physics.Raycast(transform.position, transform.right, out right, 1f, runnableMask);

        Debug.Log(left.transform);
        Debug.Log(right.transform);

        RaycastHit closest = new RaycastHit();

        if (left.collider && Vector3.Dot(left.normal, Vector3.up) < 0.1f && Vector3.Dot(left.normal, Vector3.up) > -0.1f &&
            right.collider && Vector3.Dot(right.normal, Vector3.up) < 0.1f && Vector3.Dot(right.normal, Vector3.up) > -0.1f)
        {
            if (Vector3.Distance(transform.position, left.point) < Vector3.Distance(transform.position, right.point))
            {
                closest = left;
            }
            else
            {
                closest = right;
            }
        }
        else if (right.collider && Vector3.Dot(right.normal, Vector3.up) < 0.1f && Vector3.Dot(right.normal, Vector3.up) > -0.1f)
        {
            closest = right;
        }
        else if (left.collider && Vector3.Dot(left.normal, Vector3.up) < 0.1f && Vector3.Dot(left.normal, Vector3.up) > -0.1f)
        {
            closest = left;
        }

        return closest;
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

            //Vector along wall
            Vector3 along = Vector3.Cross(currentWall.normal, Vector3.up);

            //Find Angle
            float angle = Vector3.Angle(transform.forward, along);

            //Face along wall
            if (angle < 90)
                transform.forward = along;
            else
            {
                transform.forward = -along;
            }

            Vector3 inputVec;

            //Run along wall
            if (angle < 90)
                inputVec = along;
            else
                inputVec = -along;

            inputVec.y += ySpeed.Evaluate(wallrunTimer);
            inputVec = inputVec.normalized;

            PlayerCharacterInputs inputs = new PlayerCharacterInputs();
            inputs.motion = inputVec * moveSpeed;

            cr.SetInputs(inputs);
            //Movement(inputVec);

            #endregion

            #region Fly Key

            if (Machine.canFly && vi.fDown)
            {
                Machine.SetState<FlyingState>();
            }

            #endregion

            #region Jump Key

            if (vi.spaceDown)
            {
                Machine.SetState<JumpState>();
            }

            #endregion
        }
    }

    private void Movement(Vector3 direction)
    {
        //cr.Move((direction * moveSpeed) * Time.deltaTime);
    }

    private void MousePressMain()
    {
        if(techManager.GetSelected is MartialArt == false)
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
        isChargingTech = true;
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
        }

        if (t.type == HitType.Beam && techManager.techChargeTimer > 0.1f)
            if (camScript && camScript.view != ThirdPersonCam.camView.RightZoomBeam)
                camScript.TransitionView(ThirdPersonCam.camView.RightZoomBeam);

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

        if (t.type == HitType.Beam)
            Machine.SetState<BeamState>();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        anim.SetBool("LeftWallrun", false);
        anim.SetBool("RightWallrun", false);
        wallrunTimer = 0f;

        decelTimer = 0;
        Machine.lastVector = currentVector;
    }
}
