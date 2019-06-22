using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingState : State
{
    protected CharacterStats stats;
    protected AdvancedController cr;
    protected ThirdPersonCam camScript;
    protected Animator anim;
    protected VirtualInput vi;
    protected TechManager techManager;

    [Header("Settings")]
    public float fallControlSpeed = 15f;
    public AnimationCurve fallVelocity;
    public float fallTimer;
    public float decelRate = 2f;

    [Header("Animation")]
    public string animState = "JumpBlend";
    public float transitionSpeed = 0.1f;

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
        stats = Machine.stats;
        cr = Machine.cr;
        camScript = Machine.camScript;
        vi = Machine.vi;

        anim = Machine.anim;
        anim.CrossFade(animState, transitionSpeed);

        techManager = Machine.techManager;
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

        if (vi.localPlayer)
        {
            Vector3 cameraFlatDirection = Vector3.ProjectOnPlane(camScript.transform.forward, transform.up);
            Vector3 cameraRight = Vector3.Cross(cameraFlatDirection, transform.up) * -1;

            Vector3 moveVector = cameraFlatDirection * inputZ;
            moveVector += cameraRight * inputX;
            moveVector = moveVector.normalized; //Clean normalized vector of the input in relation to Camera

            Vector3 inputRight = Vector3.Cross(moveVector, transform.up);
            moveVector = Vector3.Cross(transform.up, inputRight); //Compensating for the angle between camera and surface

            moveVector *= fallControlSpeed;

            moveVector.y = fallVelocity.Evaluate(fallTimer); //Downward velocity is seperate to all inputs

            PlayerCharacterInputs inputs = new PlayerCharacterInputs();
            inputs.motion = moveVector;
            inputs.overrideY = true;
            inputs.cameraPlanarDirection = cameraFlatDirection;
            inputs.maxSpeed = 30f;
            inputs.decelRate = decelRate;

            cr.SetInputs(inputs);
        }
        else
        {
            Vector3 moveVector = Vector3.zero;

            moveVector.y = fallVelocity.Evaluate(fallTimer); //Downward velocity is seperate to all inputs

            PlayerCharacterInputs inputs = new PlayerCharacterInputs();
            inputs.motion = moveVector;
            inputs.overrideY = true;
            inputs.maxSpeed = 30f;
            inputs.decelRate = decelRate;

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
            
        }

        #endregion

        #region Charge Key

        if (vi.eDown && Machine.canFly || vi.e && stats.GetEnergy < stats.maxEnergy && Machine.canFly)
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


    private bool GroundCheck()
    {
        return Machine.groundCheck.isGrounded();
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

    public override void OnStateExit()
    {
        base.OnStateExit();
        fallTimer = 0f;

        decelTimer = 0;
    }
}
