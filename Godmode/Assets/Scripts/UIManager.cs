using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public CharacterStats stats;
    public StateMachine machine;

    [Header("UI")]
    public Image hBar;
    public Image[] eBars = new Image[5];
    public Image[] sBars = new Image[5];

    public GameObject comboText;
    public TextMeshProUGUI hitCounter;
    public TextMeshProUGUI damageCounter;

    private void Start()
    {
        machine.OnComboUpdate += UpdateCombo;
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (hBar == null)
            return;

        #region Health

        hBar.fillAmount = (float)stats.GetHealth / stats.maxHealth;

        #endregion

        #region Energy

        float energyPerBar = stats.maxEnergy / 5;

        int fullBars = Mathf.FloorToInt(stats.GetEnergy / energyPerBar);
        float leftOver = stats.GetEnergy % energyPerBar;
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

        float staminaPerBar = stats.maxStamina / 5;

        int fullSTMBars = Mathf.FloorToInt(stats.GetStamina / staminaPerBar);
        float leftOverSTM = stats.GetStamina % staminaPerBar;

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

    void UpdateCombo(int hits, int totalDamage)
    {
        comboText.SetActive(hits > 0);
        hitCounter.text = hits.ToString();
        damageCounter.text = totalDamage.ToString();
    }
}
