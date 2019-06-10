using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeAttackState : State
{
    private VirtualInput vi;
    private AdvancedController cr;
    private TargetingSystem ts;
    private ThirdPersonCam camScript;
    private TechManager techManager;

    private float timer;

    [Header("Settings")]
    public string animationState = "FlyKick";
    public string exitState = "New State";

    public float moveDuration = 0.5f;
    public float moveAmount = 35f;
    public float decelRate = 0.4f;

    private Animator anim;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        vi = Machine.vi;
        cr = Machine.cr;
        ts = Machine.ts;
        techManager = Machine.techManager;
        camScript = Machine.camScript;

        ResetState();
        anim = Machine.anim;
        anim.CrossFade(animationState, 0.1f, 1);
    }

    public void ResetState()
    {
        timer = 0f;
    }

    public void Update()
    {
        Mouse();
        Movement();
    }

    public void Mouse()
    {
        if (vi.lmbUp)
        {
            techManager.MouseReleaseMain();
        }
    }

    public void Movement()
    {
        Vector3 dir = new Vector3();

        if (ts.lockOn != null)
        {
            dir = (ts.lockOn.position - transform.position).normalized;
        }
        else
        {
            dir = Vector3.ProjectOnPlane(camScript.transform.forward, transform.up);
        }

        PlayerCharacterInputs inputs = new PlayerCharacterInputs();
        inputs.motion = dir * moveAmount;
        inputs.decelRate = decelRate;
        inputs.maxSpeed = moveAmount;

        cr.SetInputs(inputs);

        if (timer >= moveDuration)
            ExitAttackState();

        timer += Time.deltaTime;
    }

    private bool GroundCheck()
    {
        return cr.grounded;
    }

    public void ExitAttackState()
    {
        if (GroundCheck())
            Machine.SetState<GroundedState>();
        else
            Machine.SetState<FlyingState>();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        anim.CrossFade(exitState, 0.1f, 1);
    }
}
