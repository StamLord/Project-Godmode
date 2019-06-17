using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveAttribute { None, Juggle, TossForward, TossUp, TossDown, CancleJuggle};

[CreateAssetMenu(fileName = "NewMove", menuName = "Move")]

public class Move : ScriptableObject
{
    [Header("Menu Settings")]
    public string Name;

    [Header("Move Animation")]
    public AnimationClip animation;

    [Header("Costs")]
    public int healthCost;
    public int energyCost;
    public int staminaCost;

    [Header("Settings")]
    public bool dashAttack;
    public MoveAttribute attribute;
    public int damage;
    public float stunTime;
    public float pushBack;

    [Header("Chargable")]
    public bool chargable;
    public int fullChargeDamge;

    public float minChargeTime;
    public float fullChargeTime;

    public float fullChargePushBack;
}
