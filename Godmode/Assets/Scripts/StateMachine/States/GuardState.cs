using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardState : State
{
    protected VirtualInput vi;
    protected Animator anim;

    public float perfectGuardTime = 0.25f;
    public float guardTimer = 0f;

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
            Machine.SetState<GroundedState>();
        }
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        guardTimer = 0f;
        anim.SetBool("Guard", false);
    }
}
