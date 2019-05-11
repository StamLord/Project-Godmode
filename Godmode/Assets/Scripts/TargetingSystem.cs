using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetingSystem : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public StateMachine[] allTargets;

    [Header("Scanning")]
    public bool isScanning;
    public float scanTimer;
    public LayerMask scanLayerMask;
    public List<Transform> scanned = new List<Transform>();

    [Header("Locking")]
    public float maxOffCenter = 0.1f;
    public Transform lockOn;
    public TargetCenter bodyCenter;
    public bool hardLock;
    public float outOfSightLoseTime = 1f;
    public float outOfSightTimer = 0f;
    public LayerMask targetBlockLayer;

    [Header("Canvas")]
    public Canvas canvas;
    public Image targetPrefab;
    public List<Image> targetsOnScreen;
    public Image lockedTarget;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        Scan();
        DrawTargetsOnScreen();
        CheckTarget();
        DebugRay();
    }

    void Init()
    {
        allTargets = FindObjectsOfType<StateMachine>();
        canvas = FindObjectOfType<Canvas>();
    }

    void Scan()
    {
        #region Input

        if (Input.GetKey(KeyCode.Tab))
        {
            isScanning = true;
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            EndScan();
            return;
        }
        else
        {
            return;
        }
        
        #endregion

        if (isScanning)
        {
            scanTimer += Time.deltaTime;

            scanned.Clear();

            foreach (StateMachine c in allTargets)
            {
                Vector3 dir = c.transform.position - transform.position;
                RaycastHit hit;
                
                Physics.Raycast(transform.position, dir, out hit, Mathf.Infinity, scanLayerMask);
                //Debug.Log(hit.transform);
                //Debug.DrawRay(transform.position, dir, Color.blue);
                if (hit.transform != null)
                {
                    StateMachine cr = hit.transform.GetComponent<StateMachine>();
                    if (cr)
                    {
                        scanned.Add(c.gameObject.transform);
                    }
                }

            }

            if (Input.GetKeyUp(KeyCode.Tab))
            {
                EndScan();
            }
        }

    }

    void EndScan()
    {
        LockOnTarget();

        isScanning = false;
        scanTimer = 0f;
        scanned.Clear();
    }

    void DebugRay()
    {
        foreach(Transform c in scanned)
        {
            Vector3 dir = c.transform.position - transform.position;
            Debug.DrawRay(transform.position, dir, Color.red);
        }
    }

    void DrawTargetsOnScreen()
    {
        List<Vector3> positions = new List<Vector3>();

        //Loop through targets in line of sight to see who's on screen
        for (int i = 0; i < scanned.Count; i++)
        {
            Vector3 viewPos = cam.WorldToViewportPoint(scanned[i].position);

            bool onScreen = viewPos.z > 0 && viewPos.x > 0 && viewPos.x < 1 && viewPos.y > 0 && viewPos.y < 1;

            if (onScreen)
            {
                Vector3 screenPos = cam.WorldToScreenPoint(scanned[i].position);
                positions.Add(screenPos);
            }
        }

        //Removes images
        while (targetsOnScreen.Count > positions.Count)
        {
            Destroy(targetsOnScreen[0].gameObject);
            targetsOnScreen.RemoveAt(0);
        }
        //Adds images
        while (targetsOnScreen.Count < positions.Count)
        {
            Image go = Instantiate(targetPrefab, canvas.transform);
            targetsOnScreen.Add(go);
        }

        //Creates image for each target on screen
        for (int i = 0; i < targetsOnScreen.Count; i++)
        {
            targetsOnScreen[i].transform.position = positions[i];
        }

        //
        if(lockOn != null)
        {
            if(lockedTarget == null)
                lockedTarget = Instantiate(targetPrefab, canvas.transform);
            
            Vector3 screenPos = cam.WorldToScreenPoint(lockOn.position);
            lockedTarget.transform.position = screenPos;
        }
        else if (lockedTarget != null)
        {
            Destroy(lockedTarget.gameObject);
        }
        
    }

    void LockOnTarget()
    {
        List<Transform> nearCenter = new List<Transform>();
        Transform closestTarget = null;

        foreach (Transform t in scanned)
        {
            Vector3 viewPos = cam.WorldToViewportPoint(t.position);
            viewPos -= new Vector3 (0.5f, 0.5f, 0);
            //Debug.Log(viewPos);

            if(Mathf.Abs(viewPos.x) < maxOffCenter && Mathf.Abs(viewPos.y) < maxOffCenter)
            {
                nearCenter.Add(t);
            }

        }

        foreach(Transform t in nearCenter)
        {
            if (closestTarget == null)
                closestTarget = t;
            else
            {
                Vector3 viewPos = cam.WorldToViewportPoint(t.position);
                viewPos -= new Vector3(0.5f, 0.5f, 0);
                Vector3 closestViewPos = cam.WorldToViewportPoint(closestTarget.position);
                closestViewPos -= new Vector3(0.5f, 0.5f, 0);

                if (Vector3.Distance(viewPos, Vector3.zero) < Vector3.Distance(closestViewPos, Vector3.zero))
                {
                    closestTarget = t;
                }
            }
        }

        lockOn = closestTarget;
        if(lockOn)
            bodyCenter = closestTarget.GetComponentInChildren<TargetCenter>();

        hardLock = lockOn != null;
    }

    public void LockOn(Transform target, bool hard)
    {
        lockOn = target;
        bodyCenter = lockOn.GetComponentInChildren<TargetCenter>();
        hardLock = hard;
    }

    void CheckTarget()
    {
        if (!lockOn)
            return;

        RaycastHit hit;
        Physics.Raycast(transform.position, lockOn.position -transform.position, out hit, Mathf.Infinity, targetBlockLayer);

        //Debug.Log(hit.transform);
        if (hit.transform == lockOn)
        {
            outOfSightTimer = 0f;
        }
        else
        {
            outOfSightTimer += Time.deltaTime;
        }

        if(outOfSightTimer > outOfSightLoseTime)
        {
            lockOn = null;
            outOfSightTimer = 0;
        }
    }

}
