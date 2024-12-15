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

    private Vector3 _targetDirection;
    private Quaternion _targetRotation;
    private Quaternion _playerRotation;

    private void Start()
    {
        //Getting component references
        _playerInput = GetComponent<PlayerInput>();
        _body = GetComponent<Rigidbody>();
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
        _body.linearVelocity = _playerInput.moveInput * moveSpeed;

        _targetDirection = _playerInput.moveInput;

        if (_targetDirection == Vector3.zero)
        {
            _targetDirection = transform.forward;
        }

        _targetRotation = Quaternion.LookRotation(_targetDirection);

        _playerRotation = Quaternion.Slerp(transform.rotation, _targetRotation, rotationSpeed * Time.fixedDeltaTime);
        transform.rotation = _playerRotation;
    }
}
