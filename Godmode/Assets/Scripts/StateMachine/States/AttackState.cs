using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State
{
    private VirtualInput vi;
    private AdvancedController cr;
    private TargetingSystem ts;
    private ThirdPersonCam camScript;
    private TechManager techManager;

    private float timer;

    [Header("Settings")]
    public float moveDuration = 0.1f;
    public float moveAmount = 7.5f;
    public float decelRate = 0.4f;
    public float lockRadius = 5f;
    
    public override void OnStateEnter()
    {
        base.OnStateEnter();
        vi = Machine.vi;
        cr = Machine.cr;
        ts = Machine.ts;
        ts.LockOnNearest(lockRadius);
        techManager = Machine.techManager;
        camScript = Machine.camScript;

        ResetState();
    }

    private void Update()
    {
        Mouse();

        Movement();
    }

    public void Mouse()
    {
        if (vi.lmb)
        {
            techManager.MousePressMain();
        }
        else if (vi.lmbUp)
        {
            techManager.MouseReleaseMain();
        }

        if (vi.rmb)
        {

        }
        else if (vi.rmbUp)
        {

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

    public void ResetState()
    {
        timer = 0f;
    }

    public void ExitAttackState()
    {
        if (GroundCheck())
            Machine.SetState<GroundedState>();
        else
            Machine.SetState<FlyingState>();
    }

    private bool GroundCheck()
    {
        return cr.grounded;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
    }
}
