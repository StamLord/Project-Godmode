using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _origin;
    [SerializeField] private LineRenderer _lr;
    [SerializeField] private LayerMask _beamCollision;
    [SerializeField] private float _maximumLength;
    [SerializeField] private Vector3 _endPoint;
    [SerializeField] private GameObject _head;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Initialize (Camera camera, Transform origin, float maximumLength)
    {
        _camera = camera;
        _origin = origin;
        _maximumLength = maximumLength;

        _lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        _lr.SetPosition(0, _origin.position);
        Vector2 mousePos = Input.mousePosition;
        Ray ray = new Ray(_origin.position, _camera.transform.forward * _maximumLength);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, _maximumLength, _beamCollision))
        {
            _endPoint = hit.point;
        }
        else
        {
            _endPoint = _origin.position + ray.direction * _maximumLength;
        }
        _lr.SetPosition(1, _endPoint);
        _head.transform.position = _endPoint;
    }
}
