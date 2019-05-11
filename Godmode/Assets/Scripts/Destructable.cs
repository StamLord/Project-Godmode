using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour
{
    public GameObject destroyedPrefab;
    public float forceMod = 1;

    public float updateRate = .5f;
    public bool suspended = true;
    public List<Destructable> connected = new List<Destructable>();
    public int connectionsNeeded = 1;

    private void Start()
    {
        if(!suspended)
        {
            StartCoroutine("ConnectionsCheck");
        }
    }

    public virtual void Destruction()
    {
        if (destroyedPrefab == null)
        {
            MeshRenderer[] children = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer c in children)
            {
                c.gameObject.AddComponent<Rigidbody>();
            }

            return;
        }

        Destroy(this.gameObject);
        Instantiate(destroyedPrefab, transform.position, transform.rotation);
    }

    public virtual void Destruction(Vector3 direction, float force)
    {
        if(destroyedPrefab == null)
        {
            MeshRenderer[] children = GetComponentsInChildren<MeshRenderer>();
            Rigidbody rb;
            foreach (MeshRenderer c in children)
            {
                rb = c.gameObject.AddComponent<Rigidbody>();
                rb.AddForce(direction * force * forceMod, ForceMode.Impulse);
            }

            return;
        }

        Destroy(this.gameObject);
        Instantiate(destroyedPrefab, transform.position, transform.rotation);

        Collider[] cols = Physics.OverlapSphere(transform.position, 1f);

        foreach( Collider c in cols)
        {
            if(c.attachedRigidbody)
                c.attachedRigidbody.AddForce(direction * force * forceMod, ForceMode.Impulse);
        }
    }

    IEnumerator ConnectionsCheck()
    {
        while (true)
        {
            int i = 0;
            foreach (Destructable d in connected)
            {
                if (d != null)
                {
                    i++;
                }
            }

            if (i < connectionsNeeded)
            {
                Destruction();
            }

            yield return new WaitForSeconds(updateRate);
        }
    }
}
