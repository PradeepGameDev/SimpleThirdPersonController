using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class SimplePlayerController : MonoBehaviour
{
    public float moveSpeed = 10;
    public float rotationSpeed = 15;
    public float jumpHeight = 2;
    public float jumpTimeout = 0.5f;

    private PlayerInput _playerInput;
    private Rigidbody _body;
    private Transform _cameraTransform;

    private Vector3 _moveDirection;
    private Vector3 _targetDirection;
    private Quaternion _targetRotation;
    private Quaternion _playerRotation;

    private void Start()
    {
        //Getting component references
        _playerInput = GetComponent<PlayerInput>();
        _body = GetComponent<Rigidbody>();
        _cameraTransform = Camera.main.transform;
    }

    private void FixedUpdate()
    {
        if (_playerInput != null)
        {
            Movement();
        }
    }

    private void Movement()
    {
        _targetDirection = Vector3.zero;

        //Calculating move direction based on camera forward and player input
        _moveDirection = _cameraTransform.forward * _playerInput.moveInput.z;
        _moveDirection += _cameraTransform.right * _playerInput.moveInput.x;
        _moveDirection.Normalize();
        _moveDirection.y = 0;
        _targetDirection = _moveDirection;

        //Changing move direction based on the move speed
        _moveDirection *= moveSpeed;

        //Applying velocity 
        _body.linearVelocity = _moveDirection;

        //Checking if target direction is zero, then applying forward direction
        if (_targetDirection == Vector3.zero)
        {
            _targetDirection = transform.forward;
        }

        _targetRotation = Quaternion.LookRotation(_targetDirection);

        //Smoothing the transition effect from transform's rotation to new player rotation
        _playerRotation = Quaternion.Slerp(transform.rotation, _targetRotation, rotationSpeed * Time.fixedDeltaTime);
        transform.rotation = _playerRotation;
    }
}
