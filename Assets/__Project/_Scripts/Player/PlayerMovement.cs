using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController playerCC;
    [SerializeField] private Transform cameraTransform;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 1.0f; // Doğrudan kontrol
    [SerializeField] private float verticalClamp = 90f;

    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isSprinting;

    private float xRotation;
    private float verticalVelocity;

    private void Awake()
    {
        if (playerCC == null) playerCC = GetComponent<CharacterController>();
        inputActions = new InputSystem_Actions();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Update()
    {
        GetInput();
        HandleLook();     // Önce bakış
        HandleMovement(); // Sonra hareket
    }

    private void GetInput()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();
        isSprinting = inputActions.Player.Sprint.ReadValue<float>() > 0.5f;
    }

    private void HandleMovement()
    {
        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        if (move.magnitude > 0.1f)
            playerCC.Move(move * targetSpeed * Time.deltaTime);

        // Gravity
        if (playerCC.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        playerCC.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        // 🔹 Gerçekçi FPS mouse davranışı (frame bağımsız)
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        // Dikey rotasyonu sınırla
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalClamp, verticalClamp);

        // Kamera sadece X ekseninde döner (yukarı-aşağı)
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Oyuncu gövdesi yatay döner (sağa-sola)
        transform.Rotate(Vector3.up * mouseX);
    }
}
