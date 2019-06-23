using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExhaustedState : State
{
    private AdvancedController cr;
    private Animator anim;
    
    [Header("Settings")]
    [SerializeField] private float groundDecelRate = 10f;
    [SerializeField] private float airDecelRate = 4f;
    [SerializeField] private float duration = 3;
    [SerializeField] private float timer;

    [Header("Animation")]
    [SerializeField] private string animationState = "Exhausted";
    [SerializeField] private float transitionSpeed = 0.1f;

    [Header("Effect")]
    [SerializeField] private ParticleSystem particles;



    public override void OnStateEnter()
    {
        base.OnStateEnter();
        cr = Machine.cr;
        anim = Machine.anim;
        anim.CrossFade(animationState, transitionSpeed);

        timer = 0;

        if (particles)
            particles.Play();
    }

    private void Update()
    {
        if (timer > duration)
        {
            if (GroundCheck())
                Machine.SetState<GroundedState>();
            else
                Machine.SetState<FallingState>();
        }

        PlayerCharacterInputs inputs = new PlayerCharacterInputs();
        inputs.keepMaxSpeed = true;
        inputs.decelRate = (GroundCheck() ? groundDecelRate : airDecelRate);
        cr.SetInputs(inputs);


        timer += Time.deltaTime;
    }

    private bool GroundCheck()
    {
        return Machine.groundCheck.isGrounded();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        if (particles)
            particles.Stop();
    }
}
