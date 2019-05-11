using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeButton : MonoBehaviour
{
    [Header("Conditions")]
    public SkillTreeButton[] prerequisites;

    [Header("References")]
    public SkillTreeNode skill;
    public SkillTreeManager manager;
    public Image image;
    public Button button;

    [Header("Colors")]
    public Color activeColor;
    public Color regularColor;

    public bool isActive = false;
    public bool unlocked = false;

    public void Start()
    {
        SetLockState(CheckConditions());
    }

    public void Press()
    {
        if (!unlocked)
            return;

        SetActiveState(!isActive);
        skill.taken = isActive;
        manager.UpdateButton(this, isActive);
    }

    public void SetActiveState(bool active)
    {
        isActive = active;
        image.color = (isActive) ? activeColor : regularColor;
    }

    public void SetLockState(bool unlock)
    {
        unlocked = unlock;
        button.enabled = unlock;

        if (unlock == false && isActive)
        {
            skill.taken = false;
            SetActiveState(false);
        }

        manager.UpdateButton(this, isActive);
    }

    public bool CheckConditions()
    {
        if (prerequisites.Length == 0)
            return true;

        foreach (var s in prerequisites)
        {
            if (s.isActive == true)
                return true;
        }

        return false;
    }
}
