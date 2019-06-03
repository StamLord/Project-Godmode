using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuggleState : State
{
    protected AdvancedController cr;
    protected Animator anim;
    protected TechManager techManager;

    public AnimationCurve yCurve;
    public float timer;

    public Vector3 attackPoint;
    public Vector3 pushback;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        cr = Machine.cr;
        anim = Machine.anim;
        anim.SetBool("Juggle", true);
        techManager = Machine.techManager;
        techManager.ResetCombo();
    }

    void Update()
    {
        Movement();

        timer += Time.deltaTime;

        if (timer > 0.5f && GroundCheck())
            Machine.SetState<LayingState>();
    }

    void Movement()
    {
        float y = yCurve.Evaluate(timer);
        Vector3 direction = new Vector3(0, y, 0);
        PlayerCharacterInputs inputs = new PlayerCharacterInputs();
        inputs.motion = direction;
        inputs.motion += pushback;
        inputs.maxSpeed = yCurve.keys[0].value;
        inputs.ignoreOrientation = true;
        inputs.lookAt = attackPoint;

        cr.Motor.ForceUnground(0.1f);
        cr.SetInputs(inputs);
    }

    bool GroundCheck()
    {
        return Machine.groundCheck.grounded;
    }

    public void ResetJuggle()
    {
        timer = 0;
        anim.SetBool("Juggle", true);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        anim.SetBool("JuggleEnd", true);
        timer = 0f;
    }
}
