using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JuggleState : State
{
    private AdvancedController cr;
    private Animator anim;
    private TechManager techManager;

    public AnimationCurve yCurve;
    [SerializeField] private float decelRate = 2;
    [SerializeField] private float timer;

    private Vector3 _attackPoint;
    private float _pushBack;

    private Vector3 _direction;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        cr = Machine.cr;
        anim = Machine.anim;
        anim.SetBool("Juggle", true);
        techManager = Machine.techManager;
        techManager.ResetCombo();
    }

    public override void PassParameter(float pushBack)
    {
        _pushBack = pushBack;
    }

    public override void PassParameter(Vector3 attackPoint)
    {
        _attackPoint = attackPoint;
        _direction = (transform.position - _attackPoint).normalized;
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
        Vector3 upVector = new Vector3(0, y, 0);
        PlayerCharacterInputs inputs = new PlayerCharacterInputs();
        inputs.motion = upVector;
        inputs.motion += _direction * _pushBack;
        inputs.overrideY = true;
        inputs.decelRate = decelRate;
        inputs.maxSpeed = yCurve.keys[0].value;
        inputs.ignoreOrientation = true;
        inputs.lookAt = _attackPoint;

        cr.Motor.ForceUnground(0.1f);
        cr.SetInputs(inputs);
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
