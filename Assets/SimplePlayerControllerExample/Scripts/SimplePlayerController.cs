using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputs))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Speed when player is walking.")]
    public float walkingSpeed = 3;

    [Tooltip("Speed when player is running.")]
    public float runningSpeed = 8;

    [Tooltip("How fast the player turns to face move direction.")]
    public float rotationSmoothRate = 15;

    [Tooltip("How fast the player changes speed.")]
    public float speedChangeRate = 10;

    [Header("Jump")]
    [Tooltip("If the player is jumping right now.")]
    public bool isJumping;

    [Tooltip("The height that player can jump.")]
    public float jumpHeight = 1.5f;

    [Tooltip("Custom gravity used by player for jump calculation. Actual gravity is -9.81f.")]
    public float customGravity = -15f;

    [Header("Grounded")]
    [Tooltip("If the player is grounded or not.")]
    public bool isGrounded;

    [Tooltip("Radius of sphere which is used in ground detection. Make it slightly lower than capsule collider's radius.")]
    public float groundCheckRadius = 0.28f;

    [Tooltip("Offset of ground check, useful when ground is rough.")]
    public float groundOffset = -0.2f;

    [Tooltip("Time to wait before checking fall. Useful when going downward usally through stairs.")]
    public float fallTimeout = 0.15f;

    [Tooltip("Layer to check ground.")]
    public LayerMask groundLayer;

    [Tooltip("Maximum angle at which player can walk.")]
    public float maxSlopeAngle = 50;

    private PlayerInputs _input;
    private Rigidbody _body;
    private Transform _cameraTransform;
    private Animator _animator;
    private CinemachineFreeLook _cinemachineCamera;

    private int _verticalId;
    private int _jumpId;
    private int _fallId;
    private int _groundedId;

    private bool _isOnSlope;

    private float _moveSpeed;
    private float _currentMoveSpeed;
    private float _speedAnimationBlend;
    private float _fallTimeoutDelta;
    private float _vertVel;
    private float _slopeAngle;

    private Vector3 _currentMoveVelocity;
    private Vector3 _moveDirection;
    private Vector3 _targetDirection;
    private Vector3 _verticalVelocity;
    private Vector3 _spherePosition;
    private Vector3 _slopeCheckRayPosition;
    private Vector3 _slopeNormal;
    private Quaternion _targetRotation;
    private Quaternion _playerRotation;
    private RaycastHit _hit;

    private void Awake()
    {
        // Getting component references.
        _input = GetComponent<PlayerInputs>();
        _body = GetComponent<Rigidbody>();
        _cameraTransform = Camera.main.transform;
        _animator = GetComponent<Animator>();

#if UNITY_6000_0_OR_NEWER
        _cinemachineCamera = FindFirstObjectByType<CinemachineFreeLook>();
#else
        _cinemachineCamera = FindObjectOfType<CinemachineFreeLook>();
#endif

        AssignAnimatorIDs();
    }

    private void Start()
    {
        // If cinemachine camera is available then set this as follow target.
        if (_cinemachineCamera != null)
        {
            _cinemachineCamera.Follow = transform;
            _cinemachineCamera.LookAt = transform;
        }
    }

    private void AssignAnimatorIDs()
    {
        // Getting animator parameter ids
        _verticalId = Animator.StringToHash("Vertical");
        _jumpId = Animator.StringToHash("IsJumping");
        _fallId = Animator.StringToHash("Freefall");
        _groundedId = Animator.StringToHash("IsGrounded");
    }

    private void FixedUpdate()
    {
        if (_input == null)
        {
            return;
        }

        CheckGrounded();
        CheckSlope();
        HandleMovement();
        HandleRotation();
        HandleJump();
    }

    private void CheckGrounded()
    {
        // Getting sphere position with some offset so ground check can be detected accurately.
        _spherePosition = transform.position;
        _spherePosition.y -= groundOffset;
        // Casting rays in down direction to check if it hits ground.
        isGrounded = Physics.CheckSphere(_spherePosition, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);

        // Set animator IsGrounded property.
        _animator.SetBool(_groundedId, isGrounded);
    }

    private void CheckSlope()
    {
        if (!isGrounded)
        {
            _isOnSlope = false;
            return;
        }

        _slopeCheckRayPosition = _spherePosition;
        _slopeCheckRayPosition.y += groundOffset;

        // Check if on ground and gets hit info
        if (Physics.Raycast(_slopeCheckRayPosition, Vector3.down, out _hit, 1, groundLayer))
        {
            _slopeNormal = _hit.normal;

            // Calculating angle between slope's normal and up direction
            _slopeAngle = Vector3.Angle(_slopeNormal, Vector3.up);

            _isOnSlope = _slopeAngle > 0 && _slopeAngle <= maxSlopeAngle;
        }
    }

    private void HandleMovement()
    {
        if (!isGrounded)
        {
            return;
        }

        // Deciding the move speed of player.
        _moveSpeed = _input.isRunning ? runningSpeed : walkingSpeed;

        if (_input.move == Vector3.zero)
        {
            _moveSpeed = 0;
        }

        // Calculating move direction based on camera forward and player input.
        _moveDirection = _cameraTransform.forward * _input.move.z;
        _moveDirection += _cameraTransform.right * _input.move.x;
        _moveDirection.y = 0;
        _moveDirection.Normalize();

        // Used to determine the animation based on speed.
        _speedAnimationBlend = Mathf.Lerp(_speedAnimationBlend, _moveSpeed, Time.deltaTime * speedChangeRate);
        if (_speedAnimationBlend < 0.01f)
        {
            _speedAnimationBlend = 0;
        }

        // Calculating the current speed of the player.
        _currentMoveVelocity = _body.linearVelocity;
        _currentMoveVelocity.y = 0;
        _currentMoveSpeed = _currentMoveVelocity.magnitude;

        if (_currentMoveSpeed < _moveSpeed - 0.1f || _currentMoveSpeed > _moveSpeed + 0.1f)
        {
            _moveSpeed = Mathf.Lerp(_currentMoveSpeed, _moveSpeed, speedChangeRate * Time.deltaTime);
        }

        // If on slope then modify move direction.
        if (_isOnSlope)
        {
            // Apply same move direction on plane with slope normal.
            _moveDirection = Vector3.ProjectOnPlane(_moveDirection, _slopeNormal).normalized;
        }

        _moveDirection *= _moveSpeed;

        // Applying direction velocity.
        _body.linearVelocity = _moveDirection;

        // Setting animation based on speed.
        _animator.SetFloat(_verticalId, _speedAnimationBlend);
    }

    private void HandleRotation()
    {
        if (!isGrounded)
        {
            return;
        }

        // Calculating move direction based on camera forward and player input
        _targetDirection = _cameraTransform.forward * _input.move.z;
        _targetDirection += _cameraTransform.right * _input.move.x;
        _targetDirection.Normalize();

        // Checking if target direction is zero, then applying forward direction
        if (_targetDirection == Vector3.zero)
        {
            _targetDirection = transform.forward;
        }

        // Getting rotation from direction
        _targetRotation = Quaternion.LookRotation(_targetDirection);
        // Resetting x and z axis, So player rotates only in y-axis
        _targetRotation.x = 0;
        _targetRotation.z = 0;

        // Smoothing the transition effect from transform's rotation to target rotation
        _playerRotation = Quaternion.Slerp(transform.rotation, _targetRotation, rotationSmoothRate * Time.fixedDeltaTime);
        transform.rotation = _playerRotation;
    }

    private void HandleJump()
    {
        if (isGrounded)
        {
            isJumping = false;

            // Resetting fall timeout
            _fallTimeoutDelta = fallTimeout;
            
            // Disable jump and freefall
            _animator.SetBool(_jumpId, false);
            _animator.SetBool(_fallId, false);

            // Reset vertical velocity
            if (_vertVel < 0)
            {
                _vertVel = -3;
            }

            // If we are on a slope with too steep slope then return and do not jump
            if (_slopeAngle > maxSlopeAngle)
            {
                return;
            }

            // Checking jump input
            if (_input.jump)
            {
                isJumping = true;

                // Applying move direction so new velocity does
                // not modify the direction player is going
                _verticalVelocity = _moveDirection;

                // Calculating square root of (-2 * jump height * gravity)
                // to get velocity needed to jump to the specified height
                _vertVel = Mathf.Sqrt(-2 * jumpHeight * customGravity);
                _verticalVelocity.y = _vertVel;

                // Applying velocity
                _body.linearVelocity = _verticalVelocity;

                // Enable jump animation
                _animator.SetBool(_jumpId, true);
            }
        }
        else
        {
            // Handling Freefall
            if (_fallTimeoutDelta > 0)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // Enable Freefall animation
                _animator.SetBool(_fallId, true);
            }

            // Set vertical velocity to original velocity
            _verticalVelocity = _body.linearVelocity;

            // Keep increase gravity if it does not reach its limit.
            // Limit is useful to stop it increasing infinitely.
            if (_vertVel > -50)
            {
                // Gradually increasing gravity
                _vertVel += customGravity * Time.deltaTime;
            }

            // Set vertical velocity on y axis
            _verticalVelocity.y = _vertVel;
            // Assign modified velocity
            _body.linearVelocity = _verticalVelocity;
        }
    }
}
