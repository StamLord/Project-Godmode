using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyToSleep : MonoBehaviour
{
    Rigidbody rb;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
       // if (rb.velocity.magnitude < 0.1f)
       //     rb.Sleep();

        Debug.Log(rb.IsSleeping());
    }
}
