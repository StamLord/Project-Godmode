using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public CharacterStats player_stats;
    public StateMachine machine;
    private TargetingSystem targetingSystem;

    [Header("Player UI")]
    public Image hBar;
    public Image[] eBars = new Image[5];
    public Image[] sBars = new Image[5];

    public GameObject comboText;
    public TextMeshProUGUI hitCounter;
    public TextMeshProUGUI damageCounter;

    [Header("Enemy UI")]
    public GameObject enemy_hudParent;
    public RenderTexture enemyHead;
    public Image enemy_hBar;
    public Image[] enemy_eBars = new Image[5];
    public Image[] enemy_sBars = new Image[5];

    private StateMachine enemy;

    private void Start()
    {
        machine.OnComboUpdate += UpdateCombo;
        targetingSystem = machine.ts;

        targetingSystem.OnChangeTargetEvent += OnChangeTarget;
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (hBar == null)
            return;

        //Update Player HUD
        UpdateHUD(player_stats, hBar, eBars, sBars);

        //Update Enemy Hud if locked on
        enemy_hudParent.SetActive((enemy));
        if (enemy_hudParent.activeSelf)
        {
            if (enemy.stats)
                UpdateHUD(enemy.stats, enemy_hBar, enemy_eBars, enemy_sBars);
        }
        
    }

    void UpdateCombo(int hits, int totalDamage)
    {
        comboText.SetActive(hits > 0);
        hitCounter.text = hits.ToString();
        damageCounter.text = totalDamage.ToString();
    }

    void UpdateHUD(CharacterStats stats, Image healthBar, Image[] energyBars, Image[] staminaBars)
    {
        #region Health

        healthBar.fillAmount = (float)stats.GetHealth / stats.maxHealth;

        #endregion

        #region Energy

        float energyPerBar = stats.maxEnergy / energyBars.Length;

        int fullBars = Mathf.FloorToInt(stats.GetEnergy / energyPerBar);
        float leftOver = stats.GetEnergy % energyPerBar;

        int i;
        for (i = 0; i < fullBars; i++)
        {
            energyBars[i].fillAmount = 1;
        }

        if (leftOver != 0 && i < energyBars.Length)
            energyBars[i].fillAmount = leftOver / energyPerBar;

        for (i++; i < energyBars.Length; i++)
        {
            energyBars[i].fillAmount = 0;
        }

        #endregion

        #region Stamina

        float staminaPerBar = stats.maxStamina / staminaBars.Length;

        int fullSTMBars = Mathf.FloorToInt(stats.GetStamina / staminaPerBar);
        float leftOverSTM = stats.GetStamina % staminaPerBar;

        int j;
        for (j = 0; j < fullSTMBars; j++)
        {
            staminaBars[j].fillAmount = 1;
        }

        if (leftOverSTM != 0 && j < staminaBars.Length)
            staminaBars[j].fillAmount = leftOverSTM / staminaPerBar;

        for (j++; j < staminaBars.Length; j++)
        {
            staminaBars[j].fillAmount = 0;
        }


        #endregion
    }
    void OnChangeTarget(StateMachine newTarget)
    {
        if (enemy != null)
            enemy.headCam.enabled = false;

        enemy = newTarget;

        if (enemy != null)
        {
            enemy.SetHeadCamTexture(enemyHead);
            enemy.headCam.enabled = true;
        }
    }
}

