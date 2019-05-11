using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(VirtualInput))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(TargetingSystem))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(TechManager))]

public class StateMachine : MonoBehaviour
{
    [Header("Components")]
    public Camera cam;
    public ThirdPersonCam camScript;
    public VirtualInput vi;
    public CharacterController cr;
    public TargetingSystem ts;
    public Animator anim;
    public TechManager techManager;
    public GroundCheck groundCheck;

    [Header("Stats")]
    public int maxHealth = 1000;
    public int maxEnergy = 1000;
    public int maxStamina = 1000;

    public int health = 1000;
    public int energy = 1000;
    public int stamina = 1000;

    public int healthRegenRate = 0;
    public float healthRegenTimer;
    public int energyRegenRate = 10;
    public float energyRegenTimer;
    public int staminaRegenRate = 10;
    public float staminaRegenTimer;

    public int energyChargeRate = 250;

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


    private void Update()
    {
        Regen();
        UpdateUI();
    }

    private void OnValidate()
    {
        vi = GetComponent<VirtualInput>();
        cr = GetComponent<CharacterController>();
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
        return Hit(damage, owner, 0.25f, false, worldPosition);
    }

    public bool Hit(int damage, StateMachine owner, float stunTime, bool juggle, Vector3 worldPosition)
    {
        if(currentState.GetType() == typeof(GuardState))
        {
            GuardState gs = currentState as GuardState;

            //Perfect Guard
            if(gs.guardTimer <= gs.perfectGuardTime)
            {
                return false;
            }

            UpdateHealth(Mathf.FloorToInt(-damage * 0.25f));
            if (camScript) camScript.StartShake(0.5f, false);
            return true;
        }

        if (juggle)
            SetState<JuggleState>();
        else
        {
            if (currentState.GetType() == typeof(DashState) || currentState.GetType() == typeof(CrashState))
            {
                //crashDirection = (worldPosition - transform.position).normalized;
                SetState<CrashState>();
            }
            else if (currentState.GetType() == typeof(JuggleState))
            {
                (currentState as JuggleState).timer = 0f;
            }
            else
            {
                SetState<HitState>();
                HitState hitState = GetCurrentState as HitState;
                hitState.stunTime = stunTime;
            }
        }

        UpdateHealth(-damage);
        if (camScript) camScript.StartShake(0.25f, true);
        return true;
    }

    void Regen()
    {
        if(healthRegenTimer >= 1f / healthRegenRate)
        {
            health = Mathf.Clamp(health + 1, 0, maxHealth);
            healthRegenTimer -= 1f / healthRegenRate;
        }

        if (energyRegenTimer >= 1f / energyRegenRate)
        {
            energy = Mathf.Clamp(energy + 1, 0, maxEnergy);
            energyRegenTimer -= 1f / energyRegenRate;
        }

        if (staminaRegenTimer >= 1f / staminaRegenRate)
        {
            stamina = Mathf.Clamp(stamina + 1, 0, maxStamina);
            staminaRegenTimer -= 1f / staminaRegenRate;
        }

        healthRegenTimer += Time.deltaTime;
        energyRegenTimer += Time.deltaTime;
        staminaRegenTimer += Time.deltaTime;
    }

    void UpdateHealth(int amount)
    {
        health += amount;
        if (health <= 0)
            Die();
    }

    void Die()
    {
        //Set state to dead
    }

    public void EnterToss(Vector3 direction)
    {
        tossDirection = direction;
        SetState<TossState>();
    }

    void UpdateUI()
    {
        if (!vi.localPlayer)
            return;

        #region Health

        hBar.fillAmount = (float)health / maxHealth;

        #endregion

        #region Energy

        float energyPerBar = maxEnergy / 5;

        int fullBars = Mathf.FloorToInt(energy / energyPerBar);
        float leftOver = energy % energyPerBar;
        //Debug.Log(leftOver);

        int i;
        for (i = 0; i < fullBars; i++)
        {
            eBars[i].fillAmount = 1;
        }

        if (leftOver != 0 && i < eBars.Length)
            eBars[i].fillAmount = leftOver / energyPerBar;

        for (i++; i < eBars.Length; i++)
        {
            eBars[i].fillAmount = 0;
        }

        #endregion

        #region Stamina

        float staminaPerBar = maxStamina / 5;

        int fullSTMBars = Mathf.FloorToInt(stamina / staminaPerBar);
        float leftOverSTM = stamina % staminaPerBar;

        int j;
        for (j = 0; j < fullSTMBars; j++)
        {
            sBars[j].fillAmount = 1;
        }

        if (leftOverSTM != 0 && j < sBars.Length)
            sBars[j].fillAmount = leftOverSTM / staminaPerBar;

        for (j++; j < sBars.Length; j++)
        {
            sBars[j].fillAmount = 0;
        }


        #endregion

    }
}
