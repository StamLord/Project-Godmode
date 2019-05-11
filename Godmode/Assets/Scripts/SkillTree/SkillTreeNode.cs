using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SkillTreeNode", menuName = "SkillTree/Node")]

public class SkillTreeNode: ScriptableObject
{
    public bool taken;

    [Header("Unlocking:")]
    public Technique tech;

    [Header("Stats Bonus:")]
    public int health;
    public int stamina;
    public int energy;

}

