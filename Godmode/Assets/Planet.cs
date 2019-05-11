using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public float radius = 2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, transform.localScale.x * radius);

        foreach(Collider c in cols)
        {
            c.transform.up = c.transform.position - transform.position;
            c.transform.LookAt(c.transform.forward) ;
        }
    }
}
