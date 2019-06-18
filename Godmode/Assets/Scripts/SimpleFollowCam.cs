using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFollowCam : MonoBehaviour
{
    public Transform target;
    public float distance;
    public bool bilboard;
    public Camera mainCam;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = target.position;
        Vector3 flatForward = Vector3.ProjectOnPlane(target.forward, Vector3.up);
        transform.position +=  Vector3.Cross(Vector3.up, Vector3.Cross(flatForward, Vector3.up)) * distance;
        transform.LookAt(target);

        if(bilboard)
        {
            transform.LookAt(mainCam.transform);
        }
    }
}
