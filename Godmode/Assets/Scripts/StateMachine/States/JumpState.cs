using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : State
{
    protected ThirdPersonCam camScript;
    protected CharacterController cr;
    protected Animator anim;
    protected VirtualInput vi;
    protected TechManager techManager;

    [Header("Settings")]
    public LayerMask wallMask;
    public float jumpControlSpeed = 15f;
    public int maxJumps = 1;
    public int jumpNumber = 0;
    public AnimationCurve jumpVelocity;
    public float jumpTimer;

    public bool isChargingTech;

    [Header("Input Controls")]
    public float doubleTapWindow = 0.5f;
    private float doubleTapTimer;
    private KeyCode lastKey;

    public float decelTime = 0.5f;
    protected float decelTimer;
    public Vector3 lastInputVector;
    public Vector3 currentVector;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        camScript = Machine.camScript;
        cr = Machine.cr;
        anim = Machine.anim;
        vi = Machine.vi;
        techManager = Machine.techManager;
        anim.SetBool("Jump", true);
        jumpNumber++;

        lastInputVector = Machine.lastVector;
    }

    void Update()
    {
        if (GroundCheck() && jumpTimer >= 0.1f)
            Machine.SetState<GroundedState>();

        //if (CeilingCheck() && jumpTimer >= 0.1f)
        //    Machine.SetState<FallingState>();

        InputCheck();

        AnimationUpdate();

        jumpTimer += Time.deltaTime;
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
            currentVector = Vector3.Lerp(lastInputVector, Vector3.zero, decelTimer / decelTime);
            Movement(currentVector);
            decelTimer += Time.deltaTime;
        }
        else
        {
            decelTimer = 0;
            Movement(inputVec);
            lastInputVector = ((inputVec * jumpControlSpeed) + -Vector3.up * 10f) * Time.deltaTime;
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
            RaycastHit runnableWall = RunnableWallCheck();

            if (runnableWall.collider)
            {
                Machine.wallToRun = runnableWall;
                Machine.SetState<WallRunState>();
            }
            else
            {
                //Jump again
                if (jumpNumber < maxJumps)
                {
                    jumpNumber++;
                    jumpTimer = 0f;
                    anim.SetBool("DoubleJump", (jumpNumber > 1));
                }
            }
        }

        #endregion

        #region Charge Key

        if (vi.e && Machine.canFly)
        {
            Machine.SetState<ChargeState>();
        }
        #endregion

    }

    private void Movement(Vector3 direction)
    {
        float verticalY = jumpVelocity.Evaluate(jumpTimer);
        Vector3 speedVector = direction * jumpControlSpeed;
        speedVector += Vector3.up * verticalY;

        cr.Move(speedVector * Time.deltaTime);
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

    private bool GroundCheck()
    {
        return Machine.groundCheck.grounded;
    }

    /*private bool CeilingCheck()
    {
        return Physics.Raycast(Machine.topCheck.position, Vector3.up, 0.08f);
    }*/

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
        anim.SetFloat("ySpeed", jumpVelocity.Evaluate(jumpTimer));
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


    RaycastHit RunnableWallCheck()
    {
        RaycastHit left;
        RaycastHit right;

        Physics.Raycast(transform.position, -transform.right, out left, 1f, wallMask);
        Physics.Raycast(transform.position, transform.right, out right, 1f, wallMask);

        Debug.Log(left.transform);
        Debug.Log(right.transform);

        RaycastHit closest = new RaycastHit();

        if(left.collider && Vector3.Dot(left.normal, Vector3.up) < 0.1f && Vector3.Dot(left.normal, Vector3.up) > -0.1f  && 
            right.collider && Vector3.Dot(right.normal, Vector3.up) < 0.1f && Vector3.Dot(right.normal, Vector3.up) > -0.1f)
        {
            if(Vector3.Distance(transform.position,left.point) < Vector3.Distance(transform.position, right.point))
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

    public override void OnStateExit()
    {
        base.OnStateExit();
        jumpNumber = 0;
        jumpTimer = 0f;
        anim.SetBool("Jump", false);
        anim.SetBool("DoubleJump", false);
        AnimationUpdate();

        decelTimer = 0;
        Machine.lastVector = currentVector;
    }
}
