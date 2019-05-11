using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomController : MonoBehaviour
{
    public LayerMask colMask;
    public float height;
    public float radius;

    protected bool _grounded;
    public bool isGrounded { get { return _grounded; } }
    public float slopeAngle;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.DrawWireCube(transform.position, new Vector3(.1f, height, .1f));
    }

    private void Update()
    {
        GroundCheck();
    }

    public void MoveBody(Vector3 vector)
    {
        RaycastHit collision = CollisionDetection(vector, radius);
        if (collision.collider)
            transform.position = collision.point;
        else
            transform.position += vector;
    }

    private RaycastHit CollisionDetection(Vector3 dir, float distance)
    {
        Ray ray = new Ray(transform.position, dir);
        RaycastHit hit;
        Debug.DrawRay(transform.position, dir, Color.blue, 1);
        if(Physics.Raycast(ray, out hit, distance, colMask))
        {
            Debug.Log(this.gameObject + ":: Collision Detected with: " + hit.collider.gameObject);
        }

        return hit;
    }

    private void GroundCheck()
    {
        RaycastHit collision = CollisionDetection(Vector3.down, height / 2);
        _grounded = (collision.collider != null);
        if(_grounded)
        {
            slopeAngle = Vector3.Angle(transform.position, collision.normal);
            //Vector3 forward = transform.forward;
            //transform.up = collision.normal;
            //transform.forward = forward;
        }

    }
}
