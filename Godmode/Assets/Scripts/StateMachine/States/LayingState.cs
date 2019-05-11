using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayingState : State
{
    protected VirtualInput vi;
    protected Animator anim;

    public float duration = 3f;
    public float timer = 0f;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        vi = Machine.vi;
        anim = Machine.anim;
        anim.SetBool("Laying", true);
    }

    void Update()
    {
        if(vi.lmbDown || vi.rmbDown)
        {
            timer += 0.02f * duration;
            Debug.Log("Removed " + 0.02f * duration);
        }

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
    }
}
