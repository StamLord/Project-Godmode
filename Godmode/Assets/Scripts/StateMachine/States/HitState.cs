using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitState : State
{
    protected AdvancedController cr;
    protected Animator anim;
    protected TechManager techManager;

    public Vector3 attackPoint;
    public float stunTime;
    public Vector3 pushback;
    public float timer;

    public float decelRate = 4f;

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

        timer += Time.deltaTime;

        Movement(pushback);
    }

    bool GroundCheck()
    {
        return Machine.groundCheck.grounded;
    }

    void Movement(Vector3 direction)
    {
        PlayerCharacterInputs inputs = new PlayerCharacterInputs();
        inputs.motion = direction;
        inputs.maxSpeed = direction.magnitude;
        inputs.decelRate = decelRate;
        inputs.ignoreOrientation = true;
        inputs.lookAt = attackPoint;

        cr.SetInputs(inputs);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        anim.SetBool("Hit", false);
        timer = 0f;
        stunTime = 0f;
    }
}
