using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveButton : MonoBehaviour, IPointerEnterHandler 
{
    public Move move;
    public MartialArtBuilder manager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        manager.AnimateMove(move);
    }

    public void AddMove()
    {
        manager.AddMove(move);
    }

}
