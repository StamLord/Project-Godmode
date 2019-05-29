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
    public StateMachine machine;
    public VirtualInput vi;

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

    [Header("UI")]
    public Image hBar;
    public Image[] eBars = new Image[5];
    public Image[] sBars = new Image[5];

    public TextMeshProUGUI hitCounter;
    public TextMeshProUGUI damageCounter;

    private void OnValidate()
    {
        machine = GetComponent<StateMachine>();
        vi = GetComponent<VirtualInput>();
    }

    void Update()
    {
        Regen();
        UpdateUI();
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
            health = Mathf.Clamp(health + 1, 0, maxHealth);
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

        if (energyRegenTimer >= 1f / energyRegenRate)
        {
            energy = Mathf.Clamp(energy + 1, 0, maxEnergy);
            energyRegenTimer -= 1f / energyRegenRate;
        }

        staminaRegenTimer += Time.deltaTime;
    }

    void RegenEnergy()
    {
        if (ContainState(machine.GetCurrentState, skipEnergyRegen))
        {
            energyRegenTimer = 0;
            return;
        }

        if (staminaRegenTimer >= 1f / staminaRegenRate)
        {
            stamina = Mathf.Clamp(stamina + 1, 0, maxStamina);
            staminaRegenTimer -= 1f / staminaRegenRate;
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

    void UpdateUI()
    {
        if (vi && !vi.localPlayer)
            return;

        if (hBar == null)
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
