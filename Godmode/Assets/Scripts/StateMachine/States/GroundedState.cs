using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedState : State
{
    protected CharacterStats stats;
    protected ThirdPersonCam camScript;
    protected VirtualInput vi;
    protected AdvancedController cr;
    protected TargetingSystem ts;
    protected Animator anim;
    protected TechManager techManager;
    protected SimpleAI ai;

    [Header("Settings")]
    public float moveSpeed = 15f;
    [Tooltip("Deceleration that comes into effect when stopping")]
    public float stopDecelRate = 10f;
    [Tooltip("Deceleration that comes into effect on turns")]
    public float moveDecelRate = 5f;
    protected float groundedTimer;

    [Header("Animation")]
    public string animState = "GroundBlend";
    public float transitionSpeed = 0.1f;

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
        stats = Machine.stats;
        camScript = Machine.camScript;
        vi = Machine.vi;
        cr = Machine.cr;
        ts = Machine.ts;

        anim = Machine.anim;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("JumpBlend"))
        {
            anim.CrossFade("JumpEnd", transitionSpeed);
        }
        else
        {
            anim.CrossFade(animState, transitionSpeed);
        }

        techManager = Machine.techManager;
        ai = Machine.ai;
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
        return cr.grounded;
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
        
        if (vi.localPlayer)
        {
            Vector3 moveVector = Vector3.zero;
            Vector3 cameraFlatDirection = Vector3.ProjectOnPlane(camScript.transform.forward, transform.up); //Camera forward projected on flat surface to avoid speed loss when not parallel to ground.
            Vector3 cameraRight = Vector3.Cross(cameraFlatDirection, transform.up) * -1;

            //If locked on target will move in relation to it
            if (ts.lockOn != null)
            {
                Vector3 enemyPos = ts.lockOn.position;
                Vector3 playerPos = transform.position;

                enemyPos.y = playerPos.y = 0f;

                Vector3 dir = (enemyPos - playerPos).normalized;

                if (Vector3.Distance(playerPos, enemyPos) > .8f || inputZ < 0f)
                {
                    moveVector += dir * inputZ;
                }

                moveVector += Vector3.Cross(transform.up, dir) * inputX;
                moveVector *= moveSpeed;
            }
            else
            { 
                moveVector = cameraFlatDirection * inputZ;
                moveVector += cameraRight * inputX;
                moveVector = moveVector.normalized; //Clean normalized vector of the input in relation to Camera

                Vector3 inputRight = Vector3.Cross(moveVector, transform.up);
                moveVector = Vector3.Cross(transform.up,inputRight); //Compensating for the angle between camera and surface
                moveVector *= moveSpeed;
            }

            PlayerCharacterInputs inputs = new PlayerCharacterInputs();
            inputs.motion = moveVector;
            inputs.cameraPlanarDirection = cameraFlatDirection;
            inputs.maxSpeed = moveSpeed;
            inputs.decelRate = (moveVector == Vector3.zero) ? stopDecelRate * 2f : moveDecelRate;

            cr.SetInputs(inputs);
        }
        else
        {
            Vector3 moveVector = Vector3.zero;

            if (ai != null)
            {
                moveVector = ai.currentDirection * inputZ;
                moveVector += Vector3.Cross(transform.up, ai.currentDirection) * inputX;
                moveVector *= moveSpeed;
            }

            PlayerCharacterInputs inputs = new PlayerCharacterInputs();
            inputs.motion = moveVector;
            inputs.maxSpeed = moveSpeed;
            inputs.decelRate = (moveVector == Vector3.zero) ? stopDecelRate * 2f : moveDecelRate;
            inputs.ignoreOrientation = true;

            cr.SetInputs(inputs);
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

        if (vi.eDown || vi.e && stats.GetEnergy < stats.maxEnergy)
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
        //To be added after TechManager is updated
    }

    private void MouseReleaseSecondary()
    {
        //To be added after TechManager is updated
    }

    private void TechCharge()
    {
        techManager.TechCharge();
    }

    private void AnimationUpdate()
    {
        anim.SetFloat("Speed", new Vector2(vi.horizontal, vi.vertical).normalized.magnitude);

        float animDelta = anim.GetFloat("DirectionDelta");
        float smoothDelta = Mathf.Lerp(animDelta, cr.GetDirectionDelta / 90 * 2, .1f);

        anim.SetFloat("DirectionDelta", smoothDelta);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        anim.SetFloat("Speed", 0f);
        groundedTimer = 0f;
    }
}
