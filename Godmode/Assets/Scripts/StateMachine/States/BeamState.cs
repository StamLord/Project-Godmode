using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamState : State
{
    protected AdvancedController cr;
    protected ThirdPersonCam camScript;
    protected Animator anim;
    protected TechManager techManager;
    protected VirtualInput vi;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        cr = Machine.cr;
        camScript = Machine.camScript;
        camScript.StartShake(false);
        anim = Machine.anim;
        anim.SetFloat("Speed", 0f);
        techManager = Machine.techManager;
        vi = Machine.vi;
    }

    void Update()
    {
        if(vi.lmbDown || vi.lmb)
        {
            if(GroundCheck())
            {
                Machine.SetState<GroundedState>();
            }
            else if(Machine.canFly)
            {
                Machine.SetState<FlyingState>();
            }
        }

        cr.ResetInput();
    }

    bool GroundCheck()
    {
        return Machine.groundCheck.grounded;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        camScript.EndShake();
        techManager.ExitBeamMode();
    }
}
