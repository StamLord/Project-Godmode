using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedState : State
{
    protected ThirdPersonCam camScript;
    protected VirtualInput vi;
    protected CharacterController cr;
    protected TargetingSystem ts;
    protected Animator anim;
    protected TechManager techManager;

    [Header("Settings")]
    public float moveSpeed = 15f;
    protected float groundedTimer;

    public float decelTime = 0.5f;
    protected float decelTimer;
    public Vector3 lastInputVector;
    public Vector3 currentVector;

    [Header("Input Controls")]
    public float doubleTapWindow = 0.5f;
    private float doubleTapTimer;
    private KeyCode lastKey;
    private Vector3 inputVec;

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
    }

    public override void OnStateInitialize(StateMachine machine = null)
    {
        base.OnStateInitialize(machine);
    }

    private void Update()
    {
        if (GroundCheck() == false && groundedTimer >= 0.1f)
        {
            Machine.SetState<FallingState>();
        }

        
        InputCheck();

        AnimationUpdate();

        groundedTimer += Time.deltaTime;
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

            //Vector3 inputVec;

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
                lastInputVector = ((inputVec * moveSpeed) + -Vector3.up * 10f ) * Time.deltaTime;
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
                Machine.SetState<JumpState>();
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
        cr.Move(((direction * moveSpeed) + -Vector3.up * 10f) * Time.deltaTime);
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
        techManager.TechCharge();
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

        if(anim.GetBool("ChargingAttack"))
            anim.SetBool("ChargingAttack", false);

        anim.SetBool("FiringAttack", true);

        if (t.type == HitType.Beam)
            Machine.SetState<BeamState>();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        anim.SetFloat("Speed", 0f);
        groundedTimer = 0f;

        decelTimer = 0;
        Machine.lastVector = currentVector;
    }
}
