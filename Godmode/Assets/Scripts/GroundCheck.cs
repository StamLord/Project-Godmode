using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public AdvancedController ac;
    public Transform groundColl;
    public float checkDistance = 0.25f;
    public float checkRadius = 0.2f;
    public LayerMask groundMask;
    public bool grounded;

    private void Start()
    {
        ac = GetComponent<AdvancedController>();
    }
    void Update()
    {
        grounded = isGrounded();
    }

    public bool isGrounded()
    {
        bool grounded = false;

        bool ray1 = Physics.Raycast(groundColl.position + new Vector3(checkRadius, 0, checkRadius), Vector3.down, checkDistance, groundMask);
        bool ray2 = Physics.Raycast(groundColl.position + new Vector3(-checkRadius, 0, -checkRadius), Vector3.down, checkDistance, groundMask);
        grounded = (ray1 || ray2);

        //return grounded;
        return grounded;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundColl.position, checkRadius);
        Gizmos.DrawLine(groundColl.position + new Vector3(checkRadius, 0, checkRadius), groundColl.position + new Vector3(checkRadius, -checkDistance, checkRadius));
        Gizmos.DrawLine(groundColl.position + new Vector3(-checkRadius, 0, -checkRadius), groundColl.position + new Vector3(-checkRadius, -checkDistance, -checkRadius));
    }
}
