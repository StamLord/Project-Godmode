﻿//using KinematicCharacterController;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public enum OrientationMethod
//{
//    TowardsCamera,
//    TowardsMovement,
//}

//public struct PlayerCharacterInputs
//{
//    public Vector3 motion;
//    public float MoveAxisForward;
//    public float MoveAxisUp;
//    public float MoveAxisRight;
//    public float Speed;
//    public Vector3 Gravity;
//}

//public struct AICharacterInputs
//{
//    public Vector3 MoveVector;
//    public Vector3 LookVector;
    
//}

//public class AdvancedController1 : MonoBehaviour, ICharacterController
//{
//    public KinematicCharacterMotor Motor;
//    public ThirdPersonCam Camera1;
//    public bool grounded;

//    [Header("Stable Movement")]
//    public float MaxStableMoveSpeed = 10f;
//    public float StableMovementSharpness = 15f;
//    public float OrientationSharpness = 10f;
//    public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;

//    [Header("Misc")]
//    public List<Collider> IgnoredColliders = new List<Collider>();
//    public bool OrientTowardsGravity = false;
//    public Vector3 Gravity = new Vector3(0, -30f, 0);
//    public Transform MeshRoot;
//    public Transform CameraFollowPoint;

//    private Collider[] _probedColliders = new Collider[8];
//    private Vector3 _moveInputVector;
//    private Vector3 _lookInputVector;
//    private bool _jumpRequested = false;
//    private bool _jumpConsumed = false;
//    private bool _jumpedThisFrame = false;
//    private float _timeSinceJumpRequested = Mathf.Infinity;
//    private float _timeSinceLastAbleToJump = 0f;
//    private Vector3 _internalVelocityAdd = Vector3.zero;
//    private bool _shouldBeCrouching = false;
//    private bool _isCrouching = false;

//    private Vector3 lastInnerNormal = Vector3.zero;
//    private Vector3 lastOuterNormal = Vector3.zero;

//    private void Start()
//    {
//        // Assign the characterController to the motor
//        Motor.CharacterController = this;
//    }

//    /// <summary>
//    /// This is called every frame by ExamplePlayer in order to tell the character what its inputs are
//    /// </summary>
//    public void SetInputs(ref PlayerCharacterInputs inputs)
//    {
//        Vector3 cameraFlatDirection = Vector3.ProjectOnPlane(Camera1.transform.forward, Motor.CharacterUp);
//        Vector3 cameraRight = Vector3.Cross(cameraFlatDirection, Motor.CharacterUp) *-1;

//        _moveInputVector = cameraFlatDirection * inputs.MoveAxisForward;
//        _moveInputVector += cameraRight * inputs.MoveAxisRight;
        
//        //Set Gravity
//        Gravity = inputs.Gravity;

//        switch (OrientationMethod)
//        {
//            case OrientationMethod.TowardsCamera:
//                _lookInputVector = cameraFlatDirection;
//                break;
//            case OrientationMethod.TowardsMovement:
//                _lookInputVector = _moveInputVector.normalized;
//                break;
//        }

//        //Set Max Speed
//        MaxStableMoveSpeed = inputs.Speed;

//        _moveInputVector += Vector3.up * inputs.MoveAxisUp;
//        _moveInputVector = _moveInputVector.normalized;
//    }

//    /// <summary>
//    /// This is called every frame by the AI script in order to tell the character what its inputs are
//    /// </summary>
//    public void SetInputs(ref AICharacterInputs inputs)
//    {
//        _moveInputVector = inputs.MoveVector;
//        _lookInputVector = inputs.LookVector;
//    }

//    /// <summary>
//    /// (Called by KinematicCharacterMotor during its update cycle)
//    /// This is called before the character begins its movement update
//    /// </summary>
//    public void BeforeCharacterUpdate(float deltaTime)
//    {

//    }

//    /// <summary>
//    /// (Called by KinematicCharacterMotor during its update cycle)
//    /// This is where you tell your character what its rotation should be right now. 
//    /// This is the ONLY place where you should set the character's rotation
//    /// </summary>
//    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
//    {
//        if (_lookInputVector.sqrMagnitude > 0f && OrientationSharpness > 0f)
//        {
//            // Smoothly interpolate from current to target look direction
//            Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

//            // Set the current rotation (which will be used by the KinematicCharacterMotor)
//            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
//        }
//        if (OrientTowardsGravity)
//        {
//            // Rotate from current up to invert gravity
//            currentRotation = Quaternion.FromToRotation((currentRotation * Vector3.up), -Gravity) * currentRotation;
//        }
//    }

//    /// <summary>
//    /// (Called by KinematicCharacterMotor during its update cycle)
//    /// This is where you tell your character what its velocity should be right now. 
//    /// This is the ONLY place where you can set the character's velocity
//    /// </summary>
//    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
//    {
//        if (Motor.GroundingStatus.IsStableOnGround)
//        {
//            // Ground movement
//            float currentVelocityMagnitude = currentVelocity.magnitude;

//            Vector3 effectiveGroundNormal = Motor.GroundingStatus.GroundNormal;
//            if (currentVelocityMagnitude > 0f && Motor.GroundingStatus.SnappingPrevented)
//            {
//                // Take the normal from where we're coming from
//                Vector3 groundPointToCharacter = Motor.TransientPosition - Motor.GroundingStatus.GroundPoint;
//                if (Vector3.Dot(currentVelocity, groundPointToCharacter) >= 0f)
//                {
//                    effectiveGroundNormal = Motor.GroundingStatus.OuterGroundNormal;
//                }
//                else
//                {
//                    effectiveGroundNormal = Motor.GroundingStatus.InnerGroundNormal;
//                }
//            }

//            // Reorient velocity on slope
//            currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

//            // Calculate target velocity
//            Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
//            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * _moveInputVector.magnitude;
//            Vector3 targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

//            // Smooth movement Velocity
//            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-StableMovementSharpness * deltaTime));
//        }
//        else
//            currentVelocity = _moveInputVector;

//        //Gravity
//        currentVelocity += Gravity;

//        // Drag
//        //currentVelocity *= (1f / (1f + (Drag * deltaTime)));

//        // Take into account additive velocity
//        if (_internalVelocityAdd.sqrMagnitude > 0f)
//        {
//            currentVelocity += _internalVelocityAdd;
//            _internalVelocityAdd = Vector3.zero;
//        }

        
//    }

//    /// <summary>
//    /// (Called by KinematicCharacterMotor during its update cycle)
//    /// This is called after the character has finished its movement update
//    /// </summary>
//    public void AfterCharacterUpdate(float deltaTime)
//    {
        
//    }

//    public void PostGroundingUpdate(float deltaTime)
//    {
//        // Handle landing and leaving ground
//        if (Motor.GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
//        {
//            OnLanded();
//        }
//        else if (!Motor.GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
//        {
//            OnLeaveStableGround();
//        }
//    }

//    public bool IsColliderValidForCollisions(Collider coll)
//    {
//        if (IgnoredColliders.Count == 0)
//        {
//            return true;
//        }

//        if (IgnoredColliders.Contains(coll))
//        {
//            return false;
//        }
//        return true;
//    }

//    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
//    {
//    }

//    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
//    {
//    }

//    public void AddVelocity(Vector3 velocity)
//    {
//        _internalVelocityAdd += velocity;
//    }

//    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
//    {
//    }

//    protected void OnLanded()
//    {
//        grounded = true;
//    }

//    protected void OnLeaveStableGround()
//    {
//        grounded = false;
//    }

//    public void OnDiscreteCollisionDetected(Collider hitCollider)
//    {
//    }
//}