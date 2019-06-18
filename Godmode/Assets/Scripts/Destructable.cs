using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour
{
    [Tooltip("Prefab to instantiate on destruction")]
    public GameObject destroyedPrefab;

    [Tooltip("The force that will affect the chunks on destruction")]
    public float forceMod = 1;

    [Tooltip("Is it suspended in the air")]
    public bool suspended = true;

    [Tooltip("Connected destructable object it depends on")]
    public List<Destructable> connected = new List<Destructable>();

    [Tooltip("How many seconds before a connection check will be made")]
    public float updateRate = .5f;

    [Tooltip("Below this amount of connections object will destroy itself")]
    public int connectionsNeeded = 1;

    private void Start()
    {
        if(!suspended)
        {
            StartCoroutine("ConnectionsCheck");
        }
    }

    public void Destruction()
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

    public void Destruction(Vector3 direction, float force)
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
