using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpecialArt", menuName = "Special Art")]
public class SpecialArt : Technique
{
    [Header("Costs")]
    public int healthCost;
    public int energyCost;
    public int staminaCost;

    public HitType type;

    [Header("Cast Type")]
    public bool chargable;
    //public bool releaseBeforeChargeEnd;
    public float minChargeTime;
    public float fullChargeTime;
    public bool autoFire;
    public float timeBetweenFire;
    public float castRadius;

    [Header("Scondary")]
    public bool secondary;

    [Header("Stats")]
    public int damage;
    public int fullChargeDamge;
    public int speed;
    public float blowBackForce;
    public float fullChargeBlowBackForce;
    public Vector3 fullChargeScale;

    [Header("Prefabs")]
    public GameObject projectile;
    public GameObject chargePrefab;

    [Header("Animation")]
    [Range(1, 6)] public int chargeAnimation = 1;
    [Range(1, 5)] public int attackAnimation = 1;
}

public enum HitType {Projectile, Beam, Hitscan, Cast, Other };
