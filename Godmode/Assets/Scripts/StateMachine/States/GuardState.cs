using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardState : State
{
    protected VirtualInput vi;
    protected Animator anim;

    public float perfectGuardTime = 0.20f;
    public float guardTimer = 0f;
    public int staminaOnPerfect = 250;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        vi = Machine.vi;
        anim = Machine.anim;
        anim.SetBool("Guard", true);
    }

    void Update()
    {
        if(vi.qUp || vi.q == false)
        {
            if(Machine.groundCheck.grounded)
                Machine.SetState<GroundedState>();
            else if(Machine.canFly)
                Machine.SetState<FlyingState>();

        }

        guardTimer += Time.deltaTime;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        guardTimer = 0f;
        anim.SetBool("Guard", false);
    }
}
