using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OrientationMethod
{
    TowardsMovement,
    TowardsCamera,
}

/// <summary>
/// Contains player's input relevant for the controller to move in a single frame.
/// </summary>
public struct PlayerCharacterInputs
{
    public Vector3 motion;                      // The vector by which the controller will move each second. Should come multiplied by speed from the any State.
    public bool faceY;                          // When True, (OrientationMethod.TowardsMovement) will also face the Y value of (motion)
    public bool overrideY;                      // When True will use the (motion.y) as an absolute and will not add inertia to it.
    public Vector3 cameraPlanarDirection;       // The Camera's forward, which the controller will move in relation to.
    public OrientationMethod orientationMethod; //
    public float maxSpeed;                      // This will be used to clamp the speed after adding inertia from last frame's movement (lastVector).
    public float decelRate;                     // Rate by which the (lastVector) will be slowed down each frame.
    public bool ignoreOrientation;              // Will decide controller will be rotated to match (cameraPlanarDirection) / (motion) direction; 
    public Vector3 lookAt;                      // While (ignoreOrientation) is True, controller will look at this position. Note: This is a point in World Space and NOT a direction**
}

/// <summary>
/// Contains an AI's input relevant for the controller to move in a single frame.
/// </summary>
public struct AICharacterInputs
{
    public Vector3 MoveVector;
    public Vector3 LookVector;
}

public class AdvancedController : MonoBehaviour, ICharacterController
{
    [Header("References")]
    public KinematicCharacterMotor Motor;
    [SerializeField] private TargetingSystem Targeting;
    public bool grounded { get; private set; }

    [Header("Current Settings")]
    [SerializeField] private bool _overrideY = true;  
    [SerializeField] private float _maxSpeed;         
    [SerializeField] private float _decelRate;
    [SerializeField] private float speed;
    [SerializeField] private float directionDelta;
    public Vector3 lastVector;

    [Header("Fixed Settings")]

    [Tooltip("How fast controll will rotate to a direction")]
    public float OrientationSharpness = 10f;

    [Tooltip("Controller will rotate to face this by default")]
    public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;

    [Tooltip("Will ignore a collision with these")]
    public List<Collider> IgnoredColliders = new List<Collider>();

    [Tooltip("If controller should orient according to Gravity")]
    public bool OrientTowardsGravity = false;
    public Vector3 Gravity = new Vector3(0, -30f, 0);

    private Vector3 _moveInputVector;
    private Vector3 _lookInputVector;

    #region Get Functions

    public float GetLastMaxSpeed { get { return this._maxSpeed; } }
    public float GetSpeed { get { return this.speed; } }
    public float GetDirectionDelta { get { return this.directionDelta; } }

    #endregion

    private void OnValidate()
    {
        Motor = GetComponent<KinematicCharacterMotor>();
    }

    private void Start()
    {
        // Assign the characterController to the motor
        Motor.CharacterController = this;
    }

    /// <summary>
    /// This is called every frame by other States in order to tell the controller how to behave.
    /// </summary>
    public void SetInputs(PlayerCharacterInputs inputs)
    {
        _moveInputVector = inputs.motion;
        _overrideY = inputs.overrideY;
        _maxSpeed = inputs.maxSpeed;
        _decelRate = inputs.decelRate;
        OrientationMethod = inputs.orientationMethod;

        if (inputs.ignoreOrientation)
        {
            _lookInputVector = Utility.FlatDirection(transform.position,inputs.lookAt);
        }
        else
        {
            switch (OrientationMethod)
            {
                case OrientationMethod.TowardsCamera:
                    _lookInputVector = inputs.cameraPlanarDirection;
                    break;
                case OrientationMethod.TowardsMovement:
                    _lookInputVector = new Vector3(_moveInputVector.x, (inputs.faceY) ? _moveInputVector.y : 0, _moveInputVector.z).normalized;
                    break;
            }
        }
    }

    /// <summary>
    /// This is called every frame by an AI script in order to tell the controller how to behave.
    /// </summary>
    public void SetInputs(ref AICharacterInputs inputs)
    {
        _moveInputVector = inputs.MoveVector;
        _lookInputVector = inputs.LookVector;
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called before the character begins its movement update
    /// </summary>
    public void BeforeCharacterUpdate(float deltaTime)
    {

    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its rotation should be right now. 
    /// This is the ONLY place where you should set the character's rotation
    /// </summary>
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (_lookInputVector.sqrMagnitude > 0f && OrientationSharpness > 0f)
        {
            // Smoothly interpolate from current to target look direction
            Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;
            
            // Set the current rotation (which will be used by the KinematicCharacterMotor)
            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
        }
        if (OrientTowardsGravity)
        {
            // Rotate from current up to invert gravity
            currentRotation = Quaternion.FromToRotation((currentRotation * Vector3.up), -Gravity) * currentRotation;
        }
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is where you tell your character what its velocity should be right now. 
    /// This is the ONLY place where you can set the character's velocity
    /// </summary>
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        #region Locking Near Target

        if (Targeting != null && Targeting.lockOn != null)
        {
            Vector3 playerPos = transform.position;
            Vector3 enemyPos = Targeting.lockOn.position;
            enemyPos.y = playerPos.y;

            Vector3 lockPoint = enemyPos + (playerPos - enemyPos).normalized * .8f; // Closest point a controller can reach to it's target

            if (Vector3.Distance(playerPos, enemyPos) < Vector3.Distance(enemyPos, lockPoint))
            {
                lastVector = currentVelocity = Vector3.zero;

                Motor.SetPositionAndRotation(lockPoint, Quaternion.LookRotation(enemyPos - playerPos));
                return;
            }
        }

        #endregion

        #region Direction Delta

        Vector3 flatLast = Vector3.ProjectOnPlane(currentVelocity, Vector3.up);
        Vector3 flatCurrent = Vector3.ProjectOnPlane(_moveInputVector, Vector3.up);

        directionDelta = Vector3.SignedAngle(flatLast, flatCurrent, Vector3.up);
        #endregion

        currentVelocity = _moveInputVector;

        if (_overrideY)
        {
            currentVelocity.y = 0;

            currentVelocity += new Vector3(lastVector.x, 0, lastVector.z);

            //Clamping only x and z
            currentVelocity = Vector3.ClampMagnitude(currentVelocity, _maxSpeed);

            //The y is added after clamping
            currentVelocity.y = _moveInputVector.y;
        }
        else
        {
            //Add last vector for inertia
            currentVelocity += lastVector;
            currentVelocity = Vector3.ClampMagnitude(currentVelocity, _maxSpeed);
        }

        //Decelerate
        if (lastVector.magnitude > 0.25f)
            lastVector -= lastVector * _decelRate * Time.deltaTime;
        else
            lastVector = Vector3.zero;

        //Save motion in all axis
        lastVector.x += _moveInputVector.x;

        if (_overrideY)
            lastVector.y = _moveInputVector.y;
        else
            lastVector.y += _moveInputVector.y;

        lastVector.z += _moveInputVector.z;

        //Set speedometer for outside Editor
        speed = currentVelocity.magnitude;
    }

    /// <summary>
    /// (Called by KinematicCharacterMotor during its update cycle)
    /// This is called after the character has finished its movement update
    /// </summary>
    public void AfterCharacterUpdate(float deltaTime)
    {
        ResetInput();
    }

    public void PostGroundingUpdate(float deltaTime)
    {
        // Handle landing and leaving ground
        if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
        {
            OnLanded();
        }
        else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
        {
            OnLeaveStableGround();
        }
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        if (IgnoredColliders.Count == 0)
        {
            return true;
        }

        if (IgnoredColliders.Contains(coll))
        {
            return false;
        }
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport){
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport){
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport){
    }

    protected void OnLanded()
    {
        grounded = true;
    }

    protected void OnLeaveStableGround()
    {
        grounded = false;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider){
    }

    [System.Obsolete("No need to reset input from states the can't move. Simply not calling SetInput() is enough.")]
    public void ResetInput()
    {
        PlayerCharacterInputs inputs = new PlayerCharacterInputs();
        inputs.motion = Vector3.zero;
        SetInputs(inputs);
    }
}
