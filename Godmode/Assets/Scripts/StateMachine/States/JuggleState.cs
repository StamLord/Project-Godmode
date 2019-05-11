﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuggleState : State
{
    protected CharacterController cr;
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

        if (GroundCheck())
            Machine.SetState<GroundedState>();
    }

    void Movement()
    {
        float y = yCurve.Evaluate(timer);
        Vector3 direction = new Vector3(0, y, 0);
        cr.Move(direction * Time.deltaTime);
    }

    bool GroundCheck()
    {
        return cr.isGrounded;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        anim.SetBool("JuggleEnd", true);
        timer = 0f;
    }
}
