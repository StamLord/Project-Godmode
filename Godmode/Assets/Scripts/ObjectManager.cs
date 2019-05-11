using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public float objectLifeTime;
    public Dictionary<GameObject, float> instantiated = new Dictionary<GameObject, float>();
    public List<GameObject> toRemove = new List<GameObject>();

    void Start()
    {
        StartCoroutine("RemoveFinished");
    }

    public virtual void Create(Vector3 worldPosition)
    {
        
    }

    public virtual void Create(Vector3 worldPosition, Vector3 normal)
    {
        
    }

    IEnumerator RemoveFinished()
    {
        while (true)
        {
            foreach (KeyValuePair<GameObject, float> p in instantiated)
            {
                if (Time.time - p.Value >= objectLifeTime)
                {
                    toRemove.Add(p.Key);
                    Destroy(p.Key.gameObject);
                }
            }

            foreach (GameObject go in toRemove)
            {
                instantiated.Remove(go);
            }

            toRemove.Clear();

            yield return new WaitForSeconds(1f);
        }
    }
}
