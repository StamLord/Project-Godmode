using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitState : State
{
    protected AdvancedController cr;
    protected Animator anim;
    protected TechManager techManager;

    public float stunTime;
    public Vector3 pushback;
    public float timer;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        cr = Machine.cr;
        anim = Machine.anim;
        anim.SetBool("Hit", true);
        techManager = Machine.techManager;
        techManager.ResetCombo();
        timer = 0f;
    }

    void Update()
    {
        if(timer > stunTime)
        {
            if(GroundCheck())
                Machine.SetState<GroundedState>();
            else
                Machine.SetState<FallingState>();
        }

        Movement(Vector3.Lerp(pushback, Vector3.zero, timer / stunTime));

        timer += Time.deltaTime;
    }

    bool GroundCheck()
    {
        return Machine.groundCheck.grounded;
    }

    void Movement(Vector3 direction)
    {
        cr.Move(direction * Time.deltaTime);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        anim.SetBool("Hit", false);
        timer = 0f;
        stunTime = 0f;
    }
}
