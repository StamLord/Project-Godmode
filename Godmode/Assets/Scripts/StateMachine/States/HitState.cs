using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitState : State
{
    private AdvancedController cr;
    private Animator anim;
    private TechManager techManager;

    [SerializeField] private Vector3 attackPoint;
    [SerializeField] private float _stunTime;
    [SerializeField] private float _pushback;
    [SerializeField] private float timer;
    private Vector3 _direction;

    public float decelRate = 4f;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        cr = Machine.cr;
        anim = Machine.anim;
        anim.SetBool("Hit", true);
        techManager = Machine.techManager;
        techManager.ResetCombo();
        timer = 0f;
    }

    public override void PassParameter(Vector3 origin)
    {
        _direction = (transform.position - attackPoint).normalized;
    }

    public override void PassParameter(float stunTime, float pushBack)
    {
        _stunTime = stunTime;
        _pushback = pushBack;
    }

    void Update()
    {
        if(timer > _stunTime)
        {
            if(GroundCheck())
                Machine.SetState<GroundedState>();
            else
                Machine.SetState<FallingState>();
        }

        timer += Time.deltaTime;

        Movement(_direction * _pushback);
    }

    bool GroundCheck()
    {
        return Machine.groundCheck.isGrounded();
    }

    void Movement(Vector3 direction)
    {
        PlayerCharacterInputs inputs = new PlayerCharacterInputs();
        inputs.motion = direction;
        inputs.maxSpeed = direction.magnitude;
        inputs.decelRate = decelRate;
        inputs.ignoreOrientation = true;
        inputs.lookAt = attackPoint;

        cr.SetInputs(inputs);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        anim.SetBool("Hit", false);
        timer = 0f;
        _stunTime = 0f;
    }
}
