using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [Header("Results")]
    public float groundSlopeAngle = 0f;
    public Vector3 groundSlopeDir = Vector3.zero;

    [Header("Settings")]
    public bool showDebug;
    public LayerMask castingMask;
    public float startDistanceFromBottom = 0.2f;
    public float sphereCastRadius = 0.25f;
    public float sphereCastDistance = 0.75f;

    public float raycastLength = 0.75f;
    public Vector3 rayOriginOffset1 = new Vector3(-0.2f, 0f, 0.16f);
    public Vector3 rayOriginOffset2 = new Vector3(0.2f, 0f, -0.16f);

    public CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (controller && controller.isGrounded)
            CheckGround(new Vector3(transform.position.x, transform.position.y - (controller.height / 2) + startDistanceFromBottom ,transform.position.z));
    }

    public void CheckGround(Vector3 origin)
    {
        RaycastHit hit;

        if (Physics.SphereCast(origin, sphereCastRadius, Vector3.down, out hit, sphereCastDistance, castingMask))
        {
            //Angle of the slop - Between the normal (90 degree up from surface) to world up
            groundSlopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            Vector3 temp = Vector3.Cross(hit.normal, Vector3.down);
            groundSlopeDir = Vector3.Cross(temp, hit.normal);
        }

        RaycastHit slopeHit1, slopeHit2;

        //Raycast 1
        if(Physics.Raycast(origin + rayOriginOffset1, Vector3.down, out slopeHit1, raycastLength))
        {
            if (showDebug) Debug.DrawLine(origin + rayOriginOffset1, slopeHit1.point, Color.red);

            float angleOne = Vector3.Angle(slopeHit1.normal, Vector3.up);

            //Raycast 2
            if(Physics.Raycast(origin + rayOriginOffset2, Vector3.down, out slopeHit2, raycastLength))
            {
                if (showDebug) Debug.DrawLine(origin + rayOriginOffset2, slopeHit1.point, Color.red);

                float angleTwo = Vector3.Angle(slopeHit2.normal, Vector3.up);

                //Find median of 3 angles
                float[] tempArray = new float[] { groundSlopeAngle, angleOne, angleTwo };
                Array.Sort(tempArray);
                groundSlopeAngle = tempArray[1];
            }

            else
            {
                //Find average between the 2 angles
                float average = (groundSlopeAngle + angleOne) / 2;
                groundSlopeAngle = average;
            }
        }

    }

    void OnDrawGizmosSelected()
    {
        if (showDebug)
        {
            // Visualize SphereCast with two spheres and a line
            Vector3 startPoint = new Vector3(transform.position.x, transform.position.y - (controller.height / 2) + startDistanceFromBottom, transform.position.z);
            Vector3 endPoint = new Vector3(transform.position.x, transform.position.y - (controller.height / 2) + startDistanceFromBottom - sphereCastDistance, transform.position.z);

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(startPoint, sphereCastRadius);

            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(endPoint, sphereCastRadius);

            Gizmos.DrawLine(startPoint, endPoint);
        }
    }

}
