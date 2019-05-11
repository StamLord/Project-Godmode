using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMove", menuName = "Move")]

public class Move : Technique
{
    new protected HitType type = HitType.Melee;
    [Header("Move Animation")]
    public AnimationClip animation;
    [Header("Settings")]
    public float stunTime;
    public bool juggle;


}
