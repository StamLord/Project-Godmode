using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AdvancedController : MonoBehaviour
{
    CharacterController cr;

    private void OnValidate()
    {
        cr = GetComponent<CharacterController>();
    }

    public void Move(Vector3 motion)
    {
        cr.Move(motion);
    }
}
