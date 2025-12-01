using UnityEngine;

/// <summary>
/// Main player controller supporting both first-person and third-person camera modes.
/// Handles WASD movement, mouse look, and perspective switching.
/// Attach this to the player root GameObject with a CharacterController component.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public enum CameraMode
    {
        FirstPerson,
        ThirdPerson
    }

    [Header("Movement")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 5.5f;
    public float jumpHeight = 1.2f;
    public float gravity = -15.0f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2.0f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Camera Setup")]
    public Camera playerCamera;
    public Transform firstPersonCameraPoint;
    public Transform thirdPersonCameraPoint;
    public float thirdPersonDistance = 4.0f;
    public float thirdPersonHeight = 2.0f;
    public LayerMask cameraCollisionMask;

    [Header("Current State")]
    public CameraMode currentCameraMode = CameraMode.FirstPerson;

    private CharacterController controller;
    private Vector3 velocity;
    private float pitch = 0f;
    private float yaw = 0f;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize rotation
        yaw = transform.eulerAngles.y;

        SetupCameraMode(currentCameraMode);
    }

    void Update()
    {
        HandleCameraModeSwitch();
        HandleMouseLook();
        HandleMovement();
        UpdateCameraPosition();
    }

    void HandleCameraModeSwitch()
    {
        // Press V to toggle camera mode
        if (Input.GetKeyDown(KeyCode.V))
        {
            currentCameraMode = currentCameraMode == CameraMode.FirstPerson 
                ? CameraMode.ThirdPerson 
                : CameraMode.FirstPerson;
            SetupCameraMode(currentCameraMode);
        }

        // Press Escape to unlock cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Click to lock cursor again
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void SetupCameraMode(CameraMode mode)
    {
        currentCameraMode = mode;
        // Camera position will be updated in UpdateCameraPosition
    }

    void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch = Mathf.Clamp(pitch - mouseY, minPitch, maxPitch);

        // Rotate the player body (yaw only)
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        move = Vector3.ClampMagnitude(move, 1f);

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        controller.Move(move * speed * Time.deltaTime);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void UpdateCameraPosition()
    {
        if (playerCamera == null) return;

        if (currentCameraMode == CameraMode.FirstPerson)
        {
            UpdateFirstPersonCamera();
        }
        else
        {
            UpdateThirdPersonCamera();
        }
    }

    void UpdateFirstPersonCamera()
    {
        Vector3 targetPos;
        if (firstPersonCameraPoint != null)
        {
            targetPos = firstPersonCameraPoint.position;
        }
        else
        {
            // Default: at player position + height offset (eye level)
            targetPos = transform.position + Vector3.up * 1.6f;
        }

        playerCamera.transform.position = targetPos;
        playerCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void UpdateThirdPersonCamera()
    {
        Vector3 targetPos;
        if (thirdPersonCameraPoint != null)
        {
            targetPos = thirdPersonCameraPoint.position;
        }
        else
        {
            // Calculate position behind and above the player
            Vector3 offset = -transform.forward * thirdPersonDistance + Vector3.up * thirdPersonHeight;
            targetPos = transform.position + offset;
        }

        // Check for camera collision with walls
        Vector3 playerHead = transform.position + Vector3.up * 1.6f;
        Vector3 direction = targetPos - playerHead;
        float distance = direction.magnitude;

        if (Physics.Raycast(playerHead, direction.normalized, out RaycastHit hit, distance, cameraCollisionMask))
        {
            // Move camera closer to avoid clipping through walls
            targetPos = hit.point - direction.normalized * 0.2f;
        }

        playerCamera.transform.position = targetPos;
        playerCamera.transform.LookAt(transform.position + Vector3.up * 1.2f);
    }

    /// <summary>
    /// Teleport the player to a specific position and rotation.
    /// </summary>
    public void TeleportTo(Vector3 position, Quaternion rotation)
    {
        controller.enabled = false;
        transform.position = position;
        transform.rotation = rotation;
        yaw = rotation.eulerAngles.y;
        pitch = 0f;
        velocity = Vector3.zero;
        controller.enabled = true;
    }

    /// <summary>
    /// Set camera mode programmatically.
    /// </summary>
    public void SetCameraMode(CameraMode mode)
    {
        SetupCameraMode(mode);
    }
}
