using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour
{
    /// <summary>
    /// Returns direction assuming both points stand on the same plan.
    /// </summary>
    /// <param name="from">The origin from which you get a direction</param>
    /// <param name="to">The point the direction will point</param>
    /// <returns></returns>
    public static Vector3 FlatDirection(Vector3 from, Vector3 to)
    {
        Vector3 fFrom = from;
        Vector3 fTo = to;

        fFrom.y = fTo.y = 0f;

        return (fTo - fFrom).normalized;
    }
}
