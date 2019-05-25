using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardState : State
{
    protected VirtualInput vi;
    protected Animator anim;
    protected AdvancedController cr;
    protected TechManager techManager;

    public float perfectGuardTime = 0.25f;
    public float guardTimer = 0f;
    public int staminaOnPerfect = 250;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        vi = Machine.vi;
        anim = Machine.anim;
        anim.SetBool("Guard", true);
        techManager = Machine.techManager;
        techManager.ResetCombo();
        cr = Machine.cr;

        PlayerCharacterInputs input = new PlayerCharacterInputs();
        input.motion = Vector3.zero;
        cr.SetInputs(input);
    }

    void Update()
    {
        if(vi.qUp || vi.q == false)
        {
            if(cr.grounded)
                Machine.SetState<GroundedState>();
            else
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
