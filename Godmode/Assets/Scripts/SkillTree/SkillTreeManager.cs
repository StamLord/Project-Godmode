using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SkillTreeManager : MonoBehaviour
{
    public int startPoints;
    protected int _points = 0;

    public SkillTreeButton[] buttons;
    public TextMeshProUGUI pointText;


    void Start()
    {
        ChangePoints(startPoints);
        buttons = GameObject.FindObjectsOfType<SkillTreeButton>();
    }

    public void UpdateButton(SkillTreeButton button, bool active)
    {
        foreach (SkillTreeButton b in buttons)
        {
            foreach (SkillTreeButton s in b.prerequisites)
            {
                if (s == button)
                {
                    b.SetLockState(b.CheckConditions());
                }
            }
        }
    }

    public void ChangePoints(int amount)
    {
        _points += amount;
        UpdatePointText();
    }

    public void UpdatePointText()
    {
        pointText.text = _points.ToString();
    }
}
