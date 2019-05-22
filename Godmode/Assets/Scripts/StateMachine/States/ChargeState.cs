using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeState : State
{
    protected VirtualInput vi;
    protected CharacterStats stats;
    protected ThirdPersonCam camScript;
    protected AdvancedController cr;
    protected Animator anim;
    protected ParticleSystem chargeAura;
    //protected State originState;

    public float chargeTimer = 0f;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        vi = Machine.vi;
        stats = Machine.stats;
        camScript = Machine.camScript;
        cr = Machine.cr;
        anim = Machine.anim;
        chargeAura = Machine.chargeAura;
        chargeAura.Play();
        anim.SetBool("Charge", true);
        if (camScript && !camScript.continousShake)
            camScript.StartShake(false);
    }

    void Update()
    {
        if (vi.eUp || vi.e == false)
        {
            if(GroundCheck())
                Machine.SetState<GroundedState>();
            else if (Machine.canFly)
                Machine.SetState<FlyingState>();
        }
        if(chargeTimer >= 1f / (float)stats.energyChargeRate)
        {
            stats.UpdateEnergy(1);
            chargeTimer = 0f;
        }

        chargeTimer += Time.deltaTime;
    }

    bool GroundCheck()
    {
        return Machine.groundCheck.grounded;
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        chargeTimer = 0f;
        anim.SetBool("Charge", false);
        chargeAura.Stop();

        if (camScript && camScript.continousShake)
            camScript.EndShake();
    }
}
