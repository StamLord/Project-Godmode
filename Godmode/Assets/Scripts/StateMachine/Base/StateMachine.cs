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
    public SimpleAI ai;
    public Camera headCam;

    [Header("Combo")]
    private int hits;
    private int tempDamage;
    public delegate void OnComboUpdateDelegate(int hits, int totalDamage);
    public event OnComboUpdateDelegate OnComboUpdate;

    [Header("Permissions")]
    public bool canFly = true;

    [Header("Particle Systems")]
    public ParticleSystem chargeAura;

    [Header("Bounds")]
    public Transform topCheck;
    public Transform botCheck;

    [Header("Holders")]
    public Vector3 crashDirection;
    public RaycastHit wallToRun;
    public bool layingFaceDown;

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
        ai = GetComponent<SimpleAI>();
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
        return Hit(damage, owner, 0.25f, MoveAttribute.None, 0, worldPosition);
    }

    public bool Hit(int damage, StateMachine owner, float stunTime, MoveAttribute attribute, float pushback, Vector3 worldPosition)
    {
        if(currentState.GetType() == typeof(GuardState))
        {
            GuardState gs = currentState as GuardState;

            //Perfect Guard
            if(gs.guardTimer <= gs.perfectGuardTime)
            {
                stats.UpdateStamina(gs.staminaOnPerfect);
                return false;
            }

            stats.UpdateHealth(Mathf.FloorToInt(-damage * 0.25f));
            if (camScript) camScript.StartShake(0.5f, false);
            return true;
        }
        
        else
        {
            switch(attribute)
            {
                case MoveAttribute.None:

                    if (currentState.GetType() == typeof(DashState) || currentState.GetType() == typeof(CrashState))
                    {
                        SetState<CrashState>();
                    }
                    else if (currentState.GetType() == typeof(JuggleState))
                    {
                        (currentState as JuggleState).ResetJuggle();

                        GetCurrentState.PassParameter(owner.transform.position);
                        GetCurrentState.PassParameter(pushback);
                    }
                    else if (currentState.GetType() != typeof(LayingState) && currentState.GetType() != typeof(CrashState))
                    {
                        if (SetState<HitState>())
                        {
                            GetCurrentState.PassParameter(stunTime, pushback);
                            GetCurrentState.PassParameter(owner.transform.position);
                        }
                    }

                    break;
                case MoveAttribute.Juggle:
                    if (currentState is JuggleState)
                        (currentState as JuggleState).ResetJuggle();
                    else
                        SetState<JuggleState>();

                    GetCurrentState.PassParameter(owner.transform.position);
                    GetCurrentState.PassParameter(pushback);
                    break;
                case MoveAttribute.TossUp:
                    EnterToss(Vector3.up * pushback, owner.transform.position);
                    ts.hardLock = true;
                    ShockwaveManager.instance.Create(transform.position);
                    break;
                case MoveAttribute.TossDown:
                    EnterToss(-Vector3.up * pushback, owner.transform.position);
                    ts.hardLock = true;
                    ShockwaveManager.instance.Create(transform.position);
                    break;
                case MoveAttribute.TossForward:
                    EnterToss(owner.transform.forward * pushback, owner.transform.position);
                    ts.hardLock = true;
                    ShockwaveManager.instance.Create(transform.position);
                    break;
                case MoveAttribute.CancleJuggle:
                    if (currentState.GetType() != typeof(LayingState) && currentState.GetType() != typeof(CrashState))
                    {
                        if (SetState<HitState>())
                        {
                            GetCurrentState.PassParameter(owner.transform.position);
                            GetCurrentState.PassParameter(stunTime, pushback);
                        }
                    }
                    break;
            }
            
        }

        stats.UpdateHealth(-damage);
        if (camScript) camScript.StartShake(0.25f, true);
        return true;
    }

    public void EnterToss(Vector3 direction, Vector3 origin)
    {
        if (SetState<TossState>())
        {
            //Direction comes multiplied by pushBack
            GetCurrentState.PassParameter(origin, direction);
        }
    }

    public void AddToCombo(int hitChange, int damageChange)
    {
        hits += hitChange;
        tempDamage += damageChange;

        OnComboUpdate(hits, tempDamage);
    }

    public void SetHeadCamTexture(RenderTexture texture)
    {
        headCam.targetTexture = texture;
    }
}
