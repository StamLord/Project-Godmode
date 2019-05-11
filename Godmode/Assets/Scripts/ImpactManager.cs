using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactManager : ObjectManager
{
    public GameObject impactPrefab;

    public static ImpactManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public override void Create(Vector3 worldPosition)
    {
        base.Create(worldPosition);
        GameObject go = Instantiate(impactPrefab, worldPosition, Quaternion.identity);
        instantiated.Add(go, Time.time);
    }

    public override void Create(Vector3 worldPosition, Vector3 normal)
    {
        base.Create(worldPosition, normal);
        GameObject go = Instantiate(impactPrefab, worldPosition, Quaternion.identity);
        go.transform.up = normal;
        instantiated.Add(go, Time.time);
    }
    
}
