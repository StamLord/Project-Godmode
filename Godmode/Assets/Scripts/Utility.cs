using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour
{
    public static Vector3 FlatDirection(Vector3 from, Vector3 to)
    {
        Vector3 fFrom = from;
        Vector3 fTo = to;

        fFrom.y = fTo.y = 0f;

        return fTo - fFrom;
    }
}
