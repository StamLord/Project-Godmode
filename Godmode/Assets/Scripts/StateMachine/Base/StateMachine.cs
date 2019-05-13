using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(VirtualInput))]
[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(AdvancedController))]
[RequireComponent(typeof(TargetingSystem))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(TechManager))]

public class StateMachine : MonoBehaviour
{
    [Header("Components")]
    public Camera cam;
    public ThirdPersonCam camScript;
    public VirtualInput vi;
    public CharacterStats stats;
    public AdvancedController cr;
    public TargetingSystem ts;
    public Animator anim;
    public TechManager techManager;
    public GroundCheck groundCheck;

    [Header("Combo")]
    public int hits;
    public int tempDamage;

    [Header("Permissions")]
    public bool canFly = true;

    [Header("Particle Systems")]
    public ParticleSystem chargeAura;

    [Header("Bounds")]
    public Transform topCheck;
    public Transform botCheck;

    [Header("UI")]
    public Image hBar;
    public Image[] eBars = new Image[5];
    public Image[] sBars = new Image[5];
    
    public TextMeshProUGUI hitCounter;
    public TextMeshProUGUI damageCounter;

    [Header("Holders")]
    public Vector3 lastVector;
    public Vector3 tossDirection;
    public Vector3 crashDirection;
    public RaycastHit wallToRun;

    [Header("Base")]
    public List<State> statesList = new List<State>();
    public State startingState;
    protected State currentState;

    // Start is called before the first frame update
    void Start()
    {
        SetState(startingState);
    }

    private void OnValidate()
    {
        vi = GetComponent<VirtualInput>();
        stats = GetComponent<CharacterStats>();
        cr = GetComponent<AdvancedController>();
        ts = GetComponent<TargetingSystem>();
        anim = GetComponent<Animator>();
        techManager = GetComponent<TechManager>();
    }

    protected virtual bool SwitchState(State state)
    {
        bool success = false;
        if (state && state != currentState)
        {
            if (currentState)
                currentState.OnStateExit();
            currentState = state;
            currentState.OnStateEnter();
            success = true;
        }
        return success;
    }

    public State GetCurrentState { get { return currentState; } }

    public virtual bool SetState (State state)
    {
        bool success = false;

        if(state && state != currentState)
        {
            State oldState = currentState;
            currentState = state;

            if (oldState)
            {
                oldState.StateExit();
            }

            currentState.StateEnter();
            success = true;
        }

        return success;
    }

    public void setState(State state)
    {
        SetState(state);
    }

    public virtual bool SetState<StateType> () where StateType : State
    {
        bool success = false;

        //Find it in list
        foreach (State state in statesList)
        {
            if(state is StateType)
            {
                success = SetState(state);
                return success;
            }
        }

        //If not in list, try to find on the GameObject
        State stateComponent = GetComponent<StateType>();
        if(stateComponent)
        {
            stateComponent.Initialize(this);
            statesList.Add(stateComponent);
            success = SetState(stateComponent);

            return success;
        }

        //If not on the GameObject, Add it
        State newState = gameObject.AddComponent<StateType>();
        newState.Initialize(this);
        statesList.Add(newState);
        success = SetState(newState);

        return success;
    }

    public bool Hit(int damage, StateMachine owner, Vector3 worldPosition)
    {
        return Hit(damage, owner, 0.25f, false, 0, worldPosition);
    }

    public bool Hit(int damage, StateMachine owner, float stunTime, bool juggle, float pushback, Vector3 worldPosition)
    {
        if(currentState.GetType() == typeof(GuardState))
        {
            GuardState gs = currentState as GuardState;

            //Perfect Guard
            if(gs.guardTimer <= gs.perfectGuardTime)
            {
                return false;
            }

            stats.UpdateHealth(Mathf.FloorToInt(-damage * 0.25f));
            if (camScript) camScript.StartShake(0.5f, false);
            return true;
        }
        
        else
        {
            if (currentState.GetType() == typeof(DashState) || currentState.GetType() == typeof(CrashState))
            {
                SetState<CrashState>();
            }
            else if (currentState.GetType() == typeof(JuggleState))
            {
                (currentState as JuggleState).ResetJuggle();
            }
            else if (juggle)
            {
                SetState<JuggleState>();
            }
            else
            {
                SetState<HitState>();
                HitState hitState = GetCurrentState as HitState;
                hitState.stunTime = stunTime;
                hitState.pushback = (transform.position - worldPosition).normalized * pushback;
            }
        }

        stats.UpdateHealth(-damage);
        if (camScript) camScript.StartShake(0.25f, true);
        return true;
    }

    public void EnterToss(Vector3 direction)
    {
        tossDirection = direction;
        SetState<TossState>();
    }

}
