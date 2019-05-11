using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMartialArt", menuName = "Martial Art")]

public class MartialArt : Technique
{
    [Header("Martial Art")]
    public Move[] moveArray = new Move[5];

   // new protected int healthCost;
    
}
