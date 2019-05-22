using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingState : State
{
    protected ThirdPersonCam camScript;
    protected VirtualInput vi;
    protected AdvancedController cr;
    protected TargetingSystem ts;
    protected Animator anim;
    protected TechManager techManager;

    [Header("Settings")]
    public float moveSpeed = 15f;

    public float decelAmount = .01f;
    public float decelTime = 0.5f;
    protected float decelTimer;
    protected Vector3 lastInputVector;
    protected Vector3 currentVector;

    [Header("Input Controls")]
    public float doubleTapWindow = 0.5f;
    private float doubleTapTimer;
    private KeyCode lastKey;

    [Header("Melee")]
    public bool isChargingTech;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        vi = Machine.vi;
        cr = Machine.cr;
        ts = Machine.ts;
        anim = Machine.anim;
        anim.SetBool("Flying", true);
        camScript = Machine.camScript;
        techManager = Machine.techManager;

        lastInputVector = Machine.lastVector;
    }

    void Update()
    {
        if (GroundCheck() && vi.lShift)
            Machine.SetState<GroundedState>();

        InputCheck();

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

        DoubleTapCheck();

        if (anim.GetInteger("Combo") == 0)
        {
            #region Movement Input

            float inputX = vi.horizontal;
            float inputZ = vi.vertical;
            /*
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
                currentVector = Vector3.Lerp(lastInputVector, Vector3.zero, decelTimer / decelTime);
                //lastInputVector = lastInputVector - lastInputVector * decelAmount * Time.deltaTime;
                Movement(currentVector);
                decelTimer += Time.deltaTime;
            }
            else
            {
                decelTimer = 0;
                Movement(inputVec);
                lastInputVector = (inputVec * moveSpeed) * Time.deltaTime;
            }*/

            if (vi.localPlayer)
            {
                Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(camScript.transform.rotation * Vector3.forward, transform.up).normalized;
                Vector3 moveVector = new Vector3(inputX, 0, inputZ);
                moveVector = camScript.transform.rotation * moveVector;

                if (vi.space)
                    moveVector.y += 1;
                if (vi.lShift)
                    moveVector.y -= 1;

                moveVector = moveVector.normalized; //Clean normalized vector of the input in relation to Camera
                moveVector *= moveSpeed;

                PlayerCharacterInputs inputs = new PlayerCharacterInputs();
                inputs.motion = moveVector;
                inputs.cameraPlanarDirection = cameraPlanarDirection;

                if (moveVector != Vector3.zero)
                    cr.Motor.ForceUnground(0.1f);

                cr.SetInputs(inputs);
            }

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
                Machine.SetState<GuardState>();
            }
            #endregion
        }

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
        //cr.Move((direction * moveSpeed) * Time.deltaTime);
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
        anim.SetBool("Flying", false);

        decelTimer = 0;
        Machine.lastVector = currentVector;
    }
}
