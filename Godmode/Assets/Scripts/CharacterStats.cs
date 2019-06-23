using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(StateMachine))]
[RequireComponent(typeof(VirtualInput))]
public class CharacterStats : MonoBehaviour
{
    [Header("References")]
    private StateMachine machine;
    private VirtualInput vi;
    private TechManager techManager;

    [Header("Stats")]
    public int maxHealth = 1000;
    public int maxEnergy = 1000;
    public int maxStamina = 1000;

    [SerializeField] private int health = 1000;
    [SerializeField] private int energy = 1000;
    [SerializeField] private int stamina = 1000;

    public int GetHealth { get { return health; } }
    public int GetStamina { get { return stamina; } }
    public int GetEnergy { get { return energy; } }

    [Header("Regeneration")]
    public State[] skipHealthRegen;
    public int healthRegenRate = 0;
    public float healthRegenTimer;

    public State[] skipEnergyRegen;
    public int energyRegenRate = 10;
    public float energyRegenTimer;

    public State[] skipStaminaRegen;
    public int staminaRegenRate = 20;
    public float staminaRegenTimer;

    public int energyChargeRate = 250;

    

    private void OnValidate()
    {
        machine = GetComponent<StateMachine>();
        vi = GetComponent<VirtualInput>();
        techManager = machine.techManager;
    }

    void Update()
    {
        Regen();
    }

    void Regen()
    {
        RegenHealth();
        RegenEnergy();
        RegenStamina();
    }

    void RegenHealth()
    {
        if (ContainState(machine.GetCurrentState, skipHealthRegen))
        {
            healthRegenTimer = 0;
            return;
        }

        if (healthRegenTimer >= 1f / healthRegenRate)
        {
            UpdateHealth(1);
            healthRegenTimer -= 1f / healthRegenRate;
        }

        healthRegenTimer += Time.deltaTime;
    }

    void RegenStamina()
    {
        if (ContainState(machine.GetCurrentState, skipStaminaRegen))
        {
            staminaRegenTimer = 0;
            return;
        }

        if (staminaRegenTimer >= 1f / staminaRegenRate)
        {
            UpdateStamina(1);
            staminaRegenTimer -= 1f / staminaRegenRate;
        }

        staminaRegenTimer += Time.deltaTime;
    }

    void RegenEnergy()
    {
        if (ContainState(machine.GetCurrentState, skipEnergyRegen) || techManager.isChargingTech)
        {
            energyRegenTimer = 0;
            return;
        }

        if (energyRegenTimer >= 1f / energyRegenRate)
        {
            UpdateEnergy(1);
            energyRegenTimer -= 1f / energyRegenRate;
        }

        energyRegenTimer += Time.deltaTime;
    }

    bool ContainState(State state, State[] array)
    {
        foreach(State s in array)
        {
            if (state == s)
            {
                return true;
            }
        }

        return false;
    }

    public void UpdateHealth(int amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        if (health <= 0)
            Die();
    }

    public void UpdateStamina(int amount)
    {
        stamina += amount;
        stamina = Mathf.Clamp(stamina, 0, maxStamina);
        if(stamina == 0)
        {
            if((machine.GetCurrentState is ExhaustedState) == false)
            {
                machine.SetState<ExhaustedState>();
            }
        }
    }

    public void UpdateEnergy(int amount)
    {
        energy += amount;
        energy = Mathf.Clamp(energy, 0, maxEnergy);
    }

    void Die()
    {
        //Set state to dead
    }
}
