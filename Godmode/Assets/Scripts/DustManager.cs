using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustManager : ObjectManager
{
    public GameObject dustPrefab;

    public static DustManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public override void Create(Vector3 worldPosition)
    {
        base.Create(worldPosition);
        GameObject go = Instantiate(dustPrefab, worldPosition, Quaternion.identity);
        instantiated.Add(go, Time.time);
    }

    public override void Create(Vector3 worldPosition, Vector3 direction)
    {
        base.Create(worldPosition, direction);
        GameObject go = Instantiate(dustPrefab, worldPosition, Quaternion.identity);
        go.transform.forward = direction;
        instantiated.Add(go, Time.time);
    }
}
