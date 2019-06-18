using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public StateMachine character;
    public Transform target;
    public Camera cam;
    public Projector castProj;
    public TargetingSystem targetSys;

    [Header("FOV")]
    public float restFov;
    public float maxFov = 90f;
    public bool transitionToMaxFov;
    public float fovTransitionTimer;
    public float fovTransitionDuration = 0.25f;

    [Header("Distance")]
    public float restDistance = 5f;
    public float currentDistance = 5f;
    public float minDistance = 1f;
    public float disTransitionTimer;
    public float disTransitionDuration = 0.5f;
    public float minOffsetY = 1.8f;
    public float offsetY = 1.8f;
    public float maxOffsetY = 1.4f;

    [Header("Other")]
    public float currentX;
    public float currentY;
    public float sensX = 4f;
    public float sensY = 1f;

    [Header("Views")]
    public bool useCamera = true;
    public CamView view;
    public float startTime;
    public enum CamView {InstantFront, InstantBack, LeftZoomFlight, RightZoomFlight, LeftZoomBeam, RightZoomBeam, TransitionFront, TransitionBack, LockedOn};

    [Header("Zoom Beam Corners")]
    public float zbDistance;
    public float zbOffsetY;
    public float zbOffsetX;

    [Header("Zoom Flight Corners")]
    public float zfDistance;
    public float zfOffsetY;
    public float zfOffsetX;

    [Header("Shake")]
    public bool continousShake;
    public float strength1;
    public float strength2;
    public bool strong;
    public float freq = 1f;

    private float shakeStartTime;
    private float completionPercent = 0;
    private float duration = 0;

    [Header("Rotations")]
    public float maxRollAngle = 25f;
    public float rollSpeed = .1f;
    public float speedDependency = 0.1f;
    private float rollAngle = 0f;

    Vector3 previousWaypoint = Vector3.zero;
    Vector3 currentWaypoint = Vector3.zero;
    Vector3 shakeOffset;

    Vector3 lastPos;
    Vector3 currPos;

    public Transform Transform;
    public Quaternion CurrentRotation;

    // Start is called before the first frame update
    void Start()
    {
        Transform = this.transform;
        cam = GetComponent<Camera>();
        restFov = cam.fieldOfView;
        currentDistance = restDistance;
        Cursor.lockState = CursorLockMode.Locked;
        targetSys = target.GetComponent<TargetingSystem>();
    }

    private void Update()
    {
        currentX += Input.GetAxis("Mouse X") * sensX;
        currentY += Input.GetAxis("Mouse Y") * sensY;

        //Keep values in a cirlce
        currentX %= 360f;
        currentY %= 360f;

        //Clamp Camera
        if (currentY > 80f)
        {
            currentY = 80f;
        }

        if (currentY < -80f)
        {
            currentY = -80f;
        }

        #region Camera Views Control for testing
        if (Application.isEditor)
        {
            if (Input.GetMouseButtonDown(2))
                TransitionView(CamView.InstantBack);

            else if (Input.GetMouseButtonUp(2))
                TransitionView(CamView.InstantFront);

            if (Input.GetKeyDown(KeyCode.Keypad4))
                TransitionView(CamView.LeftZoomBeam);

            if (Input.GetKeyDown(KeyCode.Keypad6))
                TransitionView(CamView.RightZoomBeam);

            if (Input.GetKeyDown(KeyCode.Keypad1))
                TransitionView(CamView.LeftZoomFlight);

            if (Input.GetKeyDown(KeyCode.Keypad3))
                TransitionView(CamView.RightZoomFlight);

            if (Input.GetKeyDown(KeyCode.Keypad8))
                TransitionView(CamView.TransitionFront);

            if (Input.GetKeyDown(KeyCode.Keypad2))
                TransitionView(CamView.TransitionBack);
        }
        #endregion
    }

    void LateUpdate()
    {
        AdjustFov();

        Vector3 dir = new Vector3(0, 0, -currentDistance);
        Quaternion rot = Quaternion.Euler(currentY, currentX, 0);
        

        Vector3 nextPos;
        if (useCamera)
        {
            switch (view)
            {
                case CamView.InstantBack:
                    dir = new Vector3(0, 0, currentDistance);
                    nextPos = target.transform.position + rot * dir;
                    transform.position = nextPos;
                    transform.LookAt(target.transform.position);
                    break;
                case CamView.InstantFront:
                    dir = new Vector3(0, offsetY, -currentDistance);
                    nextPos = target.transform.position + rot * dir;
                    transform.position = nextPos;
                    transform.rotation = Quaternion.Euler(currentY, currentX, 0);
                    break;
                case CamView.LeftZoomFlight:
                    dir = new Vector3(-zfOffsetX, zfOffsetY, -zfDistance);
                    nextPos = target.transform.position + rot * dir;
                    transform.position = Vector3.Slerp(transform.position, nextPos, (Time.time - startTime) / .5f);
                    transform.rotation = Quaternion.Euler(currentY, currentX, 0);
                    break;
                case CamView.RightZoomFlight:
                    dir = new Vector3(zfOffsetX, zfOffsetY, -zfDistance);
                    nextPos = target.transform.position + rot * dir;
                    transform.position = Vector3.Slerp(transform.position, nextPos, (Time.time - startTime) / .5f);
                    transform.rotation = Quaternion.Euler(currentY, currentX, 0);
                    break;
                case CamView.LeftZoomBeam:
                    dir = new Vector3(-zbOffsetX, zbOffsetY, -zbDistance);
                    nextPos = target.transform.position + rot * dir;
                    transform.position = Vector3.Slerp(transform.position, nextPos, (Time.time - startTime) / .5f);
                    transform.rotation = Quaternion.Euler(currentY, currentX, 0);
                    break;
                case CamView.RightZoomBeam:
                    dir = new Vector3(zbOffsetX, zbOffsetY, -zbDistance);
                    nextPos = target.transform.position + rot * dir;
                    transform.position = Vector3.Slerp(transform.position, nextPos, (Time.time - startTime) / .5f);
                    transform.rotation = Quaternion.Euler(currentY, currentX, 0);
                    break;
                case CamView.TransitionBack:
                    dir = new Vector3(0, offsetY, currentDistance);
                    nextPos = target.transform.position + rot * dir;
                    transform.position = Vector3.Slerp(transform.position, nextPos, (Time.time - startTime) / 1);
                    if (Time.time - startTime >= 1)
                        view = CamView.InstantBack;
                    transform.LookAt(target.transform.position);
                    break;
                case CamView.TransitionFront:
                    dir = new Vector3(0, offsetY, -currentDistance);
                    nextPos = target.transform.position + rot * dir;
                    transform.position = Vector3.Slerp(transform.position, nextPos, (Time.time - startTime) / 1);
                    if (Time.time - startTime >= 1)
                        view = CamView.InstantFront;
                    transform.rotation = Quaternion.Euler(currentY, currentX, 0);
                    break;
                case CamView.LockedOn:
                    dir = new Vector3(0, offsetY, currentDistance);
                    Vector3 dif = targetSys.lockOn.position - transform.position;

                    nextPos = target.transform.position + rot * dir;
                    transform.position = Vector3.Slerp(transform.position, nextPos, (Time.time - startTime) / 1);
                    transform.LookAt(target.transform.position);
                    break;
            }
        }
        
        Shake();

        RotateRoll(character.cr.GetDirectionDelta * character.cr.GetSpeed * speedDependency);
    }

    public void SetMaxFov(bool state)
    {
        transitionToMaxFov = state;
        fovTransitionTimer = 0f;
        disTransitionTimer = 0f;
    }

    void AdjustFov()
    {
        //if (cam.fieldOfView == maxFov && transitionToMaxFov)
        //    return;
        //else if (cam.fieldOfView == restFov && !transitionToMaxFov)
        //    return;

        //FOV Timer
        if (fovTransitionTimer + Time.deltaTime < fovTransitionDuration)
        {
            fovTransitionTimer += Time.deltaTime;
        }
        else
            fovTransitionTimer = fovTransitionDuration;

        float t = fovTransitionTimer / fovTransitionDuration;

        //Distance Timer
        if (disTransitionTimer + Time.deltaTime < disTransitionDuration)
        {
            disTransitionTimer += Time.deltaTime;
        }
        else
            disTransitionTimer = disTransitionDuration;

        float d = disTransitionTimer / disTransitionDuration;

        if (transitionToMaxFov)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, maxFov, t);
            currentDistance = Mathf.Lerp(currentDistance, minDistance, d);
            offsetY = Mathf.Lerp(minOffsetY, maxOffsetY, t);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, restFov, t);
            currentDistance = Mathf.Lerp(currentDistance, restDistance, d);
            offsetY = Mathf.Lerp(maxOffsetY, minOffsetY, t);
        }
    }

    public void TransitionView(CamView v)
    {
        startTime = Time.time;
        view = v;
    }

    public void StartShake(float time, bool hard)
    {
        completionPercent = 0;
        duration = time;
        strong = hard;
    }

    public void StartShake(bool hard)
    {
        completionPercent = 0;
        continousShake = true;
        strong = hard;
    }

    public void EndShake()
    {
        continousShake = false;
        strong = false;
    }

    void Shake()
    {
        completionPercent += Time.deltaTime / duration;

        if (Time.time - shakeStartTime > freq || completionPercent == 0)
        {
            if (continousShake)
            {
                currentWaypoint = Random.insideUnitCircle;
                currentWaypoint *= (strong) ? strength2 : strength1;
            }
            else
            {
                float tempStrength = (strong) ? strength2 - strength2 * completionPercent : strength1 - strength1 * completionPercent;
                tempStrength = (tempStrength < 0f) ? 0 : tempStrength;
                currentWaypoint = Random.insideUnitCircle * tempStrength;
            }

            previousWaypoint = shakeOffset;

            shakeStartTime = Time.time;
        }

        shakeOffset = Vector3.Lerp(shakeOffset, currentWaypoint, .1f);
        transform.position += shakeOffset;
    }

    void RotateRoll(float amount)
    {
        float targetAngle = amount / 180 * maxRollAngle;
        float angle = Mathf.Lerp(rollAngle, targetAngle, rollSpeed);

        Quaternion targetRot = Quaternion.Euler(0, 0, angle);

        transform.rotation *= targetRot;

        rollAngle = angle;
    }
}
