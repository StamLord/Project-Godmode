using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamState : State
{
    private AdvancedController cr;
    private CharacterStats stats;
    private ThirdPersonCam camScript;
    private Animator anim;
    private TechManager techManager;
    private VirtualInput vi;

    [SerializeField] private SpecialArt beam;

    private float tempHealthDeplete;
    private float tempEnergyDeplete;
    private float tempStaminaDeplete;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        cr = Machine.cr;
        stats = Machine.stats;
        camScript = Machine.camScript;
        camScript.StartShake(false);
        camScript.TransitionView(ThirdPersonCam.CamView.FiringBeam);
        anim = Machine.anim;
        
        techManager = Machine.techManager;
        vi = Machine.vi;
    }

    public void InitializeBeam(SpecialArt special)
    {
        beam = special;
    }

    void Update()
    {
        if(vi.lmbDown || vi.lmb)
        {
            if(GroundCheck())
            {
                Machine.SetState<GroundedState>();
            }
            else if(Machine.canFly)
            {
                Machine.SetState<FlyingState>();
            }
        }

        anim.SetFloat("Speed", 0f);

        PlayerCharacterInputs inputs = new PlayerCharacterInputs()
        {
            cameraPlanarDirection = Vector3.ProjectOnPlane(camScript.transform.forward, transform.up),
            orientationMethod = OrientationMethod.TowardsCamera
        };

        cr.SetInputs(inputs);

        DepleteStats();
    }

    void DepleteStats()
    {
        tempHealthDeplete += beam.healthCost * 0.2f * Time.deltaTime;
        if (tempHealthDeplete >= 1)
        {
            stats.UpdateHealth(-1);
            tempHealthDeplete -= 1f;
        }

        tempEnergyDeplete += beam.energyCost * 0.2f * Time.deltaTime;
        if (tempEnergyDeplete >= 1)
        {
            stats.UpdateEnergy(-1);
            tempEnergyDeplete -= 1f;
        }

        tempStaminaDeplete += beam.staminaCost * 0.2f * Time.deltaTime;
        if (tempStaminaDeplete >= 1)
        {
            stats.UpdateStamina(-1);
            tempStaminaDeplete -= 1f;
        }

        if(stats.GetEnergy <= 0)
        {
            if (GroundCheck())
            {
                Machine.SetState<GroundedState>();
            }
            else if (Machine.canFly)
            {
                Machine.SetState<FlyingState>();
            }
        }
    }

    bool GroundCheck()
    {
        return Machine.groundCheck.isGrounded();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        camScript.EndShake();
        camScript.SetDefault();
        techManager.ExitBeamMode();
    }
}
