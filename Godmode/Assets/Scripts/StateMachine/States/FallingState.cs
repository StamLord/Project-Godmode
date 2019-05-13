using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingState : State
{
    protected AdvancedController cr;
    protected ThirdPersonCam camScript;
    protected Animator anim;
    protected VirtualInput vi;
    protected TechManager techManager;

    [Header("Settings")]
    public float fallControlSpeed = 15f;
    public AnimationCurve fallVelocity;
    public float fallTimer;

    [Header("Input Controls")]
    public float doubleTapWindow = 0.5f;
    private float doubleTapTimer;
    private KeyCode lastKey;

    public float decelTime = 0.5f;
    protected float decelTimer;
    protected Vector3 lastInputVector;
    protected Vector3 currentVector;

    bool isChargingTech;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        cr = Machine.cr;
        camScript = Machine.camScript;
        anim = Machine.anim;
        vi = Machine.vi;
        anim.SetBool("Jump", true);
        techManager = Machine.techManager;

        lastInputVector = Machine.lastVector;
    }

    void Update()
    {
        if (GroundCheck())
            Machine.SetState<GroundedState>();

        InputCheck();

        fallTimer += Time.deltaTime;
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

        DoubleTapCheck();

        #region Movement Input

        float inputX = vi.horizontal;
        float inputZ = vi.vertical;


        Vector3 inputVec;

        inputVec = (transform.forward * inputZ) + (transform.right * inputX);
        inputVec = inputVec.normalized;

        if (inputVec == Vector3.zero)
        {
            currentVector = Vector3.Slerp(lastInputVector, Vector3.zero, decelTimer / decelTime);
            Movement(currentVector);
            decelTimer += Time.deltaTime;
        }
        else
        {
            decelTimer = 0;
            Movement(inputVec);
            lastInputVector = (inputVec * fallControlSpeed) * Time.deltaTime;
        }

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
            
        }

        #endregion

        #region Charge Key

        if (vi.e && Machine.canFly)
        {
            Machine.SetState<ChargeState>();
        }
        #endregion

    }

    void DoubleTapCheck()
    {
        if (vi.aDown)
        {
            if (lastKey == KeyCode.A)
            {
                Machine.SetState<DashState>();
                lastKey = KeyCode.None;
                doubleTapTimer = 0;
            }
            else
            {
                lastKey = KeyCode.A;
                doubleTapTimer = 0;
            }
        }
        else if (vi.sDown)
        {
            if (lastKey == KeyCode.S)
            {
                Machine.SetState<DashState>();
                lastKey = KeyCode.None;
                doubleTapTimer = 0;
            }
            else
                lastKey = KeyCode.S;
        }
        else if (vi.dDown)
        {
            if (lastKey == KeyCode.D)
            {
                Machine.SetState<DashState>();
                lastKey = KeyCode.None;
                doubleTapTimer = 0;
            }
            else
                lastKey = KeyCode.D;
        }
        else if (vi.wDown)
        {
            if (lastKey == KeyCode.W)
            {
                Machine.SetState<DashState>();
                lastKey = KeyCode.None;
                doubleTapTimer = 0;
            }
            else
                lastKey = KeyCode.W;
        }
        else if (vi.spaceDown)
        {
            if (lastKey == KeyCode.Space)
            {
                Machine.SetState<DashState>();
                lastKey = KeyCode.None;
                doubleTapTimer = 0;
            }
            else
                lastKey = KeyCode.Space;
        }
        else if (vi.lShiftDown)
        {
            if (lastKey == KeyCode.LeftShift)
            {
                Machine.SetState<DashState>();
                lastKey = KeyCode.None;
                doubleTapTimer = 0;
            }
            else
                lastKey = KeyCode.LeftShift;
        }

        if (lastKey != KeyCode.None) doubleTapTimer += Time.deltaTime;
        if (doubleTapTimer >= doubleTapWindow)
        {
            lastKey = KeyCode.None;
            doubleTapTimer = 0;
        }
    }

    private void Movement(Vector3 direction)
    {
        float verticalY = fallVelocity.Evaluate(fallTimer);
        Vector3 speedVector = direction * fallControlSpeed;
        speedVector += Vector3.up * verticalY;

        cr.Move(speedVector * Time.deltaTime);
    }

    private bool GroundCheck()
    {
        return Machine.groundCheck.grounded;
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

    private void AnimationUpdate()
    {
        anim.SetFloat("Speed", vi.vertical);
        anim.SetFloat("ySpeed", fallVelocity.Evaluate(fallTimer));
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
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        fallTimer = 0f;
        anim.SetBool("Jump", false);

        decelTimer = 0;
        Machine.lastVector = currentVector;
    }
}
