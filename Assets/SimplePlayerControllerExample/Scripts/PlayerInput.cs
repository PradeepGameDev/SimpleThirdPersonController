using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public Vector3 moveInput;
    public bool jump;

    private InputActions _inputActions;
    private Vector2 _moveInput2D;

    private void Awake()
    {
        _inputActions = new();
    }

    private void OnEnable()
    {
        _inputActions.PlayerMap.Enable();
    }

    private void Start()
    {
        _inputActions.PlayerMap.Movement.performed += OnMove;
        _inputActions.PlayerMap.Movement.canceled += OnMove;
        _inputActions.PlayerMap.Jump.performed += OnJump;
        _inputActions.PlayerMap.Jump.canceled += OnJump;
    }

    private void OnJump(InputAction.CallbackContext obj)
    {

    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput2D = ctx.ReadValue<Vector2>();
        moveInput.x = _moveInput2D.x;
        moveInput.z = _moveInput2D.y;
    }

    private void OnDisable()
    {
        _inputActions.PlayerMap.Disable();
    }
}
