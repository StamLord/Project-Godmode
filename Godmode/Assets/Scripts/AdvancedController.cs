using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OrientationMethod
{
    TowardsCamera,
    TowardsMovement,
}

public struct PlayerCharacterInputs
{
    public Vector3 motion;
    public bool overrideY;
    public Vector3 cameraPlanarDirection;
    public float maxSpeed;
    public float decelRate;
    public Vector3 Gravity;
    public bool ignoreOrientation;
    public Vector3 lookAt;
}

public struct AICharacterInputs
{
    public Vector3 MoveVector;
    public Vector3 LookVector;
}

public class AdvancedController : MonoBehaviour, ICharacterController
{
    [Header("Settings")]
    public bool _overrideY = true;

    public TargetingSystem Targeting;
    public KinematicCharacterMotor Motor;
    public bool grounded;
    public Vector3 lastVector;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _decelRate;
    public float GetLastMaxSpeed { get { return this._maxSpeed; } }
    public float GetSpeed { get { return this.speed; } }
    private float speed;


    [Header("Stable Movement")]
    public float MaxStableMoveSpeed = 10f;
    public float StableMovementSharpness = 15f;
    public float OrientationSharpness = 10f;
    public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;

    [Header("Misc")]
    public List<Collider> IgnoredColliders = new List<Collider>();
    public bool OrientTowardsGravity = false;
    public Vector3 Gravity = new Vector3(0, -30f, 0);
    public Transform MeshRoot;
    public Transform CameraFollowPoint;

    private Collider[] _probedColliders = new Collider[8];
    private Vector3 _moveInputVector;
    private Vector3 _lookInputVector;
    private bool _jumpRequested = false;
    private bool _jumpConsumed = false;
    private bool _jumpedThisFrame = false;
    private float _timeSinceJumpRequested = Mathf.Infinity;
    private float _timeSinceLastAbleToJump = 0f;
    private Vector3 _internalVelocityAdd = Vector3.zero;
    private bool _shouldBeCrouching = false;
    private bool _isCrouching = false;

    private Vector3 lastInnerNormal = Vector3.zero;
    private Vector3 lastOuterNormal = Vector3.zero;


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
    /// This is called every frame by ExamplePlayer in order to tell the character what its inputs are
    /// </summary>
    public void SetInputs(PlayerCharacterInputs inputs)
    {
        _moveInputVector = inputs.motion;
        _overrideY = inputs.overrideY;
        _maxSpeed = inputs.maxSpeed;
        _decelRate = inputs.decelRate;

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
                    _lookInputVector = new Vector3(_moveInputVector.x, 0, _moveInputVector.z).normalized;
                    break;
            }
        }
    }

    /// <summary>
    /// This is called every frame by the AI script in order to tell the character what its inputs are
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

            Vector3 lockPoint = enemyPos + (playerPos - enemyPos).normalized * .8f;

            if (Vector3.Distance(playerPos, enemyPos) < Vector3.Distance(enemyPos, lockPoint))
            {
                Debug.Log("Distance smaller than lockpoint");
                
                lastVector = currentVelocity = Vector3.zero;

                Motor.SetPositionAndRotation(lockPoint, Quaternion.LookRotation(enemyPos - playerPos));
                return;
            }
        }

        #endregion

        currentVelocity = _moveInputVector;

        if (_overrideY)
        {
            currentVelocity.y = 0;

            //Add last vector for inertia
            currentVelocity += new Vector3(lastVector.x, 0, lastVector.z);

            currentVelocity = Vector3.ClampMagnitude(currentVelocity, _maxSpeed);

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

        //Save motion in all axis that are not 0 for inertia
        if(_moveInputVector.x != 0f)
        {
            lastVector.x += _moveInputVector.x;
        }

        if (_moveInputVector.y != 0f)
        {
            if (_overrideY)
                lastVector.y = _moveInputVector.y;
            else
                lastVector.y += _moveInputVector.y;
        }

        if (_moveInputVector.z != 0f)
        {
            lastVector.z += _moveInputVector.z;
        }

        //Set speedometer for outside reference
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

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void AddVelocity(Vector3 velocity)
    {
        _internalVelocityAdd += velocity;
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }

    protected void OnLanded()
    {
        grounded = true;
    }

    protected void OnLeaveStableGround()
    {
        grounded = false;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    public void ResetInput()
    {
        PlayerCharacterInputs inputs = new PlayerCharacterInputs();
        inputs.motion = Vector3.zero;
        SetInputs(inputs);
        
    }
}
