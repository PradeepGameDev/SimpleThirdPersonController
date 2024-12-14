using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleTPSController : MonoBehaviour
{
    [SerializeField]
    private float speed;
    private InputActions inputActions;
    private Vector2 moveInput2D;
    private Vector3 moveInput;

    private void Awake()
    {
        inputActions = new();
    }

    private void Start()
    {
        inputActions.PlayerMap.Movement.performed += Movement_performed;
        inputActions.PlayerMap.Movement.canceled += Movement_performed;
    }

    private void Movement_performed(InputAction.CallbackContext ctx)
    {
        moveInput2D = ctx.ReadValue<Vector2>();
        moveInput.x = moveInput2D.x;
        moveInput.z = moveInput2D.y;
    }

    private void OnEnable()
    {
        inputActions.PlayerMap.Enable();
    }

    private void OnDisable()
    {
        inputActions.PlayerMap.Disable();
    }

    private void Update()
    {
        transform.position += speed * Time.deltaTime * moveInput;
    }
}
