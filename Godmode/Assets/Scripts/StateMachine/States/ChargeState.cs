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
    //protected State originState;

    public ParticleSystem chargeAura;
    public ParticleSystem fullAura;

    public float lastSpeed;
    public float groundDecelRate = 4f;
    public float airDecelRate = 2f;

    public float chargeTimer = 0f;

    protected float lastShockwave;
    protected bool startedWithFullEnergy;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        vi = Machine.vi;
        stats = Machine.stats;
        camScript = Machine.camScript;
        cr = Machine.cr;
        lastSpeed = cr.GetLastMaxSpeed;

        anim = Machine.anim;
        anim.SetBool("Charge", true);
        if (camScript && !camScript.continousShake)
            camScript.StartShake(false);

        startedWithFullEnergy = stats.GetEnergy == stats.maxEnergy;
        if(startedWithFullEnergy)
            fullAura.Play();
        else
            chargeAura.Play();
    }

    void Update()
    {
        if (stats.GetEnergy == stats.maxEnergy && !startedWithFullEnergy)
        {
            if (Time.time - lastShockwave > 3f)
            {
                ShockwaveManager.instance.Create(transform.position);
                lastShockwave = Time.time;
            }

            StopCharge();
            return;
        }

        if (vi.eUp || vi.e == false)
        {
            StopCharge();
            return;
        }

        if(chargeTimer >= 1f / (float)stats.energyChargeRate)
        {
            stats.UpdateEnergy(1);
            chargeTimer = 0f;
        }

        chargeTimer += Time.deltaTime;

        PlayerCharacterInputs inputs = new PlayerCharacterInputs();
        inputs.motion = Vector3.zero;
        inputs.maxSpeed = lastSpeed;
        inputs.decelRate = (GroundCheck() ? groundDecelRate : airDecelRate);

        cr.SetInputs(inputs);
    }

    bool GroundCheck()
    {
        return Machine.groundCheck.grounded;
    }

    void StopCharge()
    {
        if (GroundCheck())
            Machine.SetState<GroundedState>();
        else if (Machine.canFly)
            Machine.SetState<FlyingState>();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        chargeTimer = 0f;
        anim.SetBool("Charge", false);
        chargeAura.Stop();
        fullAura.Stop();

        if (camScript && camScript.continousShake)
            camScript.EndShake();
    }
}
