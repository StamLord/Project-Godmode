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
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> parent of dc1c306... test
        
=======

        cr.Motor.ForceUnground(0.1f);
<<<<<<< HEAD
>>>>>>> parent of e540f79... 25.05
=======

        cr.Motor.ForceUnground(0.1f);
>>>>>>> parent of e540f79... 25.05
=======

        cr.Motor.ForceUnground(0.1f);
>>>>>>> parent of e540f79... 25.05
=======

        cr.Motor.ForceUnground(0.1f);
>>>>>>> parent of e540f79... 25.05
=======

        cr.Motor.ForceUnground(0.1f);
>>>>>>> parent of e540f79... 25.05
=======

        cr.Motor.ForceUnground(0.1f);
>>>>>>> parent of e540f79... 25.05
        cr.SetInputs(inputs);
<<<<<<< HEAD
=======

        cr.Motor.ForceUnground(0.1f);
        cr.SetInputs(ref inputs);
>>>>>>> parent of c4c115a... 21.05
=======
        cr.SetInputs(ref inputs);
>>>>>>> parent of c4c115a... 21.05
=======
>>>>>>> parent of dc1c306... test
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
