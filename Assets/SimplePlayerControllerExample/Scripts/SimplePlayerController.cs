using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10;
    public float rotationSpeed = 15;

    [Header("Actions Settings")]
    public float jumpForce = 2;
    [Tooltip("Distance from bottom of player to the ground.")]
    [SerializeField] private float _groundDistance = 0.6f;

    [Header("Player Status")]
    [SerializeField] private bool _isGrounded;
    [SerializeField] private bool _isJumping;

    private PlayerInput _playerInput;
    private Rigidbody _body;
    private Transform _cameraTransform;
    private CapsuleCollider _collider;

    private Vector3 _moveDirection;
    private Vector3 _targetDirection;
    private Vector3 _playerBottomPosition;
    private Vector3 _jumpVelocity;
    private Quaternion _targetRotation;
    private Quaternion _playerRotation;

    private void Start()
    {
        //Getting component references
        _playerInput = GetComponent<PlayerInput>();
        _body = GetComponent<Rigidbody>();
        _cameraTransform = Camera.main.transform;
        _collider = GetComponent<CapsuleCollider>();
    }

    private void FixedUpdate()
    {
        if (_playerInput == null)
        {
            return;
        }

        CheckGrounded();
        HandleMovement();
        HandleRotation();
        HandleJump();
    }

    private void CheckGrounded()
    {
        // Getting player's bottom position with some offset so ground check can be detected accurately
        _playerBottomPosition = _collider.bounds.min + (Vector3.up * 0.2f);
        // Casting rays in down direction to check if it hits ground
        _isGrounded = Physics.Raycast(_playerBottomPosition, Vector3.down, _groundDistance);
    }

    private void HandleMovement()
    {
        if (_isJumping)
        {
            return;
        }

        // Calculating move direction based on camera forward and player input
        _moveDirection = _cameraTransform.forward * _playerInput.moveInput.z;
        _moveDirection += _cameraTransform.right * _playerInput.moveInput.x;
        _moveDirection.Normalize();
        _moveDirection.y = 0;

        // If not jumping, only then set y velocity to 0
        if (!_isGrounded)
        {
            _moveDirection.y = _body.linearVelocity.y;
        }
        else
        {
            _moveDirection.y = 0;
        }

        // Changing move direction based on the move speed
        _moveDirection *= moveSpeed;

        // Applying velocity 
        _body.linearVelocity = _moveDirection;
    }

    private void HandleRotation()
    {
        if (_isJumping)
        {
            return;
        }

        _targetDirection = Vector3.zero;

        // Calculating move direction based on camera forward and player input
        _targetDirection = _cameraTransform.forward * _playerInput.moveInput.z;
        _targetDirection += _cameraTransform.right * _playerInput.moveInput.x;
        _targetDirection.Normalize();

        // Checking if target direction is zero, then applying forward direction
        if (_targetDirection == Vector3.zero)
        {
            _targetDirection = transform.forward;
        }

        // Getting rotation from direction
        _targetRotation = Quaternion.LookRotation(_targetDirection);
        // Resetting x and z axis, So player only rotates only in y-axis
        _targetRotation.x = 0;
        _targetRotation.z = 0;

        // Smoothing the transition effect from transform's rotation to new player rotation
        _playerRotation = Quaternion.Slerp(transform.rotation, _targetRotation, rotationSpeed * Time.fixedDeltaTime);
        transform.rotation = _playerRotation;
    }

    private void HandleJump()
    {
        if (_isGrounded)
        {
            _isJumping = false;
        }

        if (_isJumping)
        {
            return;
        }

        _isJumping = false;

        if (_isGrounded && _playerInput.jump)
        {
            _isJumping = true;
            _jumpVelocity = _moveDirection;
            _jumpVelocity.y = Mathf.Sqrt(-2 * jumpForce * Physics.gravity.y);
            _body.linearVelocity = _jumpVelocity;
        }
    }
}
