using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitState : State
{
    protected CharacterController cr;
    protected Animator anim;
    protected TechManager techManager;

    public float stunTime;
    public float timer;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        cr = Machine.cr;
        anim = Machine.anim;
        anim.SetBool("Hit", true);
        techManager = Machine.techManager;
        techManager.ResetCombo();
    }

    void Update()
    {
        if(timer> stunTime)
        {
            if(GroundCheck())
                Machine.SetState<GroundedState>();
            else
                Machine.SetState<FallingState>();
        }

        timer += Time.deltaTime;
    }

    bool GroundCheck()
    {
        return cr.isGrounded;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        anim.SetBool("Hit", false);
        timer = 0f;
        stunTime = 0f;
    }
}
