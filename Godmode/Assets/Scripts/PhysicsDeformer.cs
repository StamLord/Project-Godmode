using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsDeformer : MonoBehaviour
{
    public float collisionRadius = 0.1f;
    public DeformableMesh dMesh;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionStay(Collision collision)
    {
        dMesh = collision.transform.GetComponent<DeformableMesh>();
        if (!dMesh)
            return;

        foreach(var contact in collision.contacts)
        {
            //Debug.Log(contact.point);
            dMesh.AddDepression(contact.point, -transform.up, collisionRadius);
        }
    }
}
