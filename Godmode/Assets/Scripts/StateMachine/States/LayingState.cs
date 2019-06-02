using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayingState : State
{
    protected VirtualInput vi;
    protected Animator anim;
    protected AdvancedController cr;

    public float decelRate = 4f;
    protected float lastSpeed;

    public float duration = 3f;
    public float timer = 0f;

    [Header("Particles")]
    public ParticleSystem plowTrail;

    [Header("Animation")]
    public string animStateUp = "LayingFaceUp";
    public string animStateDown = "LayingFaceDown";
    public float transitionSpeed = 0.1f;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        vi = Machine.vi;

        anim = Machine.anim;

        if (Machine.layingFaceDown)
        {
            anim.CrossFade(animStateDown, transitionSpeed);
            Machine.layingFaceDown = false;
        }
        else
        {
            anim.CrossFade(animStateUp, transitionSpeed);
        }

        cr = Machine.cr;
        lastSpeed = cr.GetLastMaxSpeed;

        var emission = plowTrail.emission;
        emission.enabled = true;
    }

    void Update()
    {
        if(vi.lmbDown || vi.rmbDown)
        {
            timer += 0.02f * duration;
            Debug.Log("Removed " + 0.02f * duration);
        }

        PlayerCharacterInputs inputs = new PlayerCharacterInputs();
        inputs.motion = Vector3.zero;
        inputs.maxSpeed = lastSpeed;
        inputs.decelRate = decelRate;

        cr.SetInputs(inputs);

        timer += Time.deltaTime;

        if(timer >= duration)
        {
            Machine.SetState<GroundedState>();
        }
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        timer = 0f;
        anim.SetBool("Laying", false);

        var emission = plowTrail.emission;
        emission.enabled = false;
    }
}
