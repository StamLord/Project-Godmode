using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashState : State
{
    private ThirdPersonCam camScript;
    private VirtualInput vi;
    private CharacterStats stats;
    private AdvancedController cr;
    private TargetingSystem ts;
    private Animator anim;
    private TechManager techManager;

    [Header("Settings")]
    public float decelRate = 4f;
    public float staminaDepleteRate = 100f;
    public float moveSpeed = 35f;
    public float destructionRadius = 1f;
    public float destructionForce = 1f;
    public int obstacleCost = 100;

    [Header("Animation")]
    public string animState = "DashBlend";
    public float transitionSpeed = 0.1f;

    //Private
    private float staminaTimer;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        vi = Machine.vi;
        stats = Machine.stats;
        cr = Machine.cr;
        ts = Machine.ts;

        anim = Machine.anim;
        anim.CrossFade(animState, transitionSpeed);

        camScript = Machine.camScript;
        camScript.SetMaxFov(true);
        techManager = Machine.techManager;
    }

    void Update()
    {
        if (GroundCheck() && vi.lShift)
            Machine.SetState<GroundedState>();

        InputCheck();
        StaminaDeplete();
        AnimationUpdate();
    }

    private bool GroundCheck()
    {
        return cr.grounded;
    }

    private void InputCheck()
    {
        #region Mouse Input

        if (vi.lmb)
        {
            MousePressMain();
        }
        else if (vi.lmbUp)
        {
            MouseReleaseMain();
        }

        if (vi.rmb)
        {
            MousePressSecondary();
        }
        else if (vi.rmbUp)
        {
            MouseReleaseSecondary();
        }

        #endregion

        #region Movement Input

        float inputX = vi.horizontal;
        float inputZ = vi.vertical;
                        
        if (inputZ == 0f && inputX==0f &&!vi.lShift && !vi.space)
        {
            Machine.SetState<FlyingState>();
        }

        if (vi.localPlayer)
        {
            Vector3 moveVector = new Vector3();
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(camScript.transform.rotation * Vector3.forward, transform.up).normalized;

            //If locked on target will move in relation to it
            if (ts.lockOn != null)
            {
                Vector3 enemyPos = ts.lockOn.position;
                Vector3 playerPos = transform.position;

                Vector3 dir = (enemyPos - playerPos).normalized;

                if (Vector3.Distance(playerPos, enemyPos) > .8f || inputZ < 0f)
                {
                    moveVector += dir * inputZ;
                }

                moveVector += Vector3.Cross(transform.up, dir) * inputX;

                if (vi.space)
                    moveVector.y += 1;
                if (vi.lShift)
                    moveVector.y -= 1;

                moveVector *= moveSpeed;
            }
            else
            {
                moveVector = new Vector3(inputX, 0, inputZ);
                moveVector = camScript.transform.rotation * moveVector;

                if (vi.space)
                    moveVector.y += 1;
                if (vi.lShift)
                    moveVector.y -= 1;

                moveVector = moveVector.normalized; //Clean normalized vector of the input in relation to Camera
                moveVector *= moveSpeed;
            }

            PlayerCharacterInputs inputs = new PlayerCharacterInputs();
            inputs.motion = moveVector;
            inputs.decelRate = decelRate;
            inputs.cameraPlanarDirection = cameraPlanarDirection;
            inputs.maxSpeed = moveSpeed;

            if (moveVector != Vector3.zero)
                cr.Motor.ForceUnground(0.1f);

            cr.SetInputs(inputs);

            #endregion

        #region Fly Key

        if (vi.fDown)
        {
            Machine.SetState<FallingState>();
        }

        #endregion

        #region Charge Key

        if (vi.e)
        {
            Machine.SetState<ChargeState>();
        }
        #endregion

        #region Guard Key

        if (vi.q)
        {
            Machine.SetState<FlyingState>();
        }
        #endregion

        }

        DestructionSphere();
    }

    void DestructionSphere()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, destructionRadius);

        foreach (Collider c in colliders)
        {
            if (c.CompareTag("Projectile"))
                continue;

            if (c.transform.root.transform == this.transform)
                continue;

            Destructable d = c.GetComponent<Destructable>();
            Rigidbody r = (c.transform != transform) ? c.GetComponent<Rigidbody>() : null;
            if (d)
            {
                if (stats.GetStamina >= obstacleCost)
                {
                    d.Destruction(transform.forward, destructionForce);
                    camScript.StartShake(.75f, true);
                    stats.UpdateStamina(-obstacleCost);
                }
            }

            if (r)
            {
                Vector3 force = (c.transform.position - transform.position).normalized * 10f;
                r.AddForce(force, ForceMode.Impulse);
                if (!c.CompareTag("Player"))
                    if (camScript) camScript.StartShake(.75f, true);
            }
        }
    }

    private void StaminaDeplete()
    {
        if(staminaTimer >= 1f / staminaDepleteRate)
        {
            stats.UpdateStamina(-1);
            staminaTimer = 0f;

            if (stats.GetStamina <= 0)
            {
                if(Machine.groundCheck.grounded)
                    Machine.SetState<GroundedState>();
                else
                    Machine.SetState<FlyingState>();
            }
        }
        staminaTimer += Time.deltaTime;

    }

    private void MousePressMain()
    {
        techManager.MousePressMain();
    }

    private void MouseReleaseMain()
    {
        techManager.MouseReleaseMain();
    }

    private void MousePressSecondary()
    {

    }

    private void MouseReleaseSecondary()
    {

    }

    private void AnimationUpdate()
    {
        anim.SetFloat("Speed", vi.vertical);

        float animDelta = anim.GetFloat("DirectionDelta");
        float smoothDelta = Mathf.Lerp(animDelta, cr.GetDirectionDelta / 90 * 2, .1f);

        anim.SetFloat("DirectionDelta", smoothDelta);
    }


    public override void OnStateExit()
    {
        base.OnStateExit();
        anim.SetBool("Dashing", false);
        camScript.SetMaxFov(false);
    }
}
