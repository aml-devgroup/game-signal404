using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private InputActionAsset PlayerControls;

    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private float upDownRange = 80f;

    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = 19.6f;

    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;

    private CharacterController characterController;
    private Camera mainCamera;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private Vector2 currentMovementSpeed = Vector2.zero;
    private float verticalVelocity;
    private float verticalRotation;

    private void Awake()
    {
        moveAction = PlayerControls.FindActionMap("Player").FindAction("Move");
        lookAction = PlayerControls.FindActionMap("Player").FindAction("Look");
        jumpAction = PlayerControls.FindActionMap("Player").FindAction("Jump");
        sprintAction = PlayerControls.FindActionMap("Player").FindAction("Sprint");
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();

        HandleMovement();
        HandleRotation();
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
    }

    private void HandleMovement()
    {
        float speedMultiplier = sprintAction.ReadValue<float>() > 0 ? sprintMultiplier : 1f;

        float targetSpeedX = moveInput.x * walkSpeed * speedMultiplier;
        float targetSpeedY = moveInput.y * walkSpeed * speedMultiplier;

        float currentLerpSpeed = moveInput.sqrMagnitude > 0 ? acceleration : deceleration;

        currentMovementSpeed.x = Mathf.Lerp(currentMovementSpeed.x, targetSpeedX, currentLerpSpeed * Time.deltaTime);
        currentMovementSpeed.y = Mathf.Lerp(currentMovementSpeed.y, targetSpeedY, currentLerpSpeed * Time.deltaTime);

        Vector3 horizontalMovement = new Vector3(currentMovementSpeed.x, 0, currentMovementSpeed.y);
        horizontalMovement = transform.rotation * horizontalMovement;

        if (characterController.isGrounded)
        {
            verticalVelocity = -2f;

            if (jumpAction.triggered)
            {
                verticalVelocity = jumpForce;
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }

        Vector3 finalMovement = horizontalMovement;
        finalMovement.y = verticalVelocity;

        characterController.Move(finalMovement * Time.deltaTime);
    }

    private void HandleRotation()
    {
        float mouseXRotation = lookInput.x * mouseSensitivity;
        transform.Rotate(0, mouseXRotation, 0);

        verticalRotation -= lookInput.y * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);

        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
}