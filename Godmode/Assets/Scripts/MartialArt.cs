using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMartialArt", menuName = "Martial Art")]

public class MartialArt : Technique
{
    [Header("Combo")]
    public Move[] moveArray = new Move[5];

    [Header("Dash")]
    public Move dashAttack;

    [Header("Grab")]
    public Move grab;
}
