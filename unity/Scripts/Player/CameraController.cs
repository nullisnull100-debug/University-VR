using UnityEngine;

/// <summary>
/// Camera controller that manages first-person and third-person camera perspectives.
/// Can be used independently or together with PlayerController.
/// Supports smooth transitions between camera modes and camera collision detection.
/// </summary>
public class CameraController : MonoBehaviour
{
    public enum CameraPerspective
    {
        FirstPerson,
        ThirdPerson,
        TopDown,       // Bird's eye view for overview
        FreeLook       // Detached camera for inspection
    }

    [Header("Target")]
    public Transform target;
    public Vector3 targetOffset = Vector3.up * 1.6f; // Eye height offset

    [Header("First Person Settings")]
    public float firstPersonFOV = 60f;

    [Header("Third Person Settings")]
    public float thirdPersonDistance = 5f;
    public float thirdPersonHeight = 2f;
    public float thirdPersonFOV = 70f;
    public float orbitSpeed = 150f;
    public float minOrbitAngle = -20f;
    public float maxOrbitAngle = 60f;

    [Header("Top Down Settings")]
    public float topDownHeight = 15f;
    public float topDownFOV = 60f;

    [Header("Collision")]
    public LayerMask collisionLayers;
    public float collisionPadding = 0.3f;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.1f;
    public float rotationSmoothTime = 0.1f;
    public float transitionDuration = 0.5f;

    [Header("Input")]
    public KeyCode togglePerspectiveKey = KeyCode.V;
    public float mouseSensitivity = 2f;

    private CameraPerspective currentPerspective = CameraPerspective.FirstPerson;
    private Camera cam;
    private Vector3 currentVelocity;
    private float orbitAngleX; // Horizontal orbit angle
    private float orbitAngleY; // Vertical orbit angle
    private bool isTransitioning;
    private float transitionProgress;
    private Vector3 transitionStartPos;
    private Quaternion transitionStartRot;
    private float transitionStartFOV;

    public CameraPerspective CurrentPerspective => currentPerspective;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        if (target == null)
        {
            // Try to find player
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        // Initialize orbit angles based on target's current rotation
        if (target != null)
        {
            orbitAngleX = target.eulerAngles.y;
        }
    }

    void Update()
    {
        HandleInput();
        
        if (isTransitioning)
        {
            UpdateTransition();
        }
        else
        {
            UpdateCamera();
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(togglePerspectiveKey))
        {
            CyclePerspective();
        }
    }

    public void CyclePerspective()
    {
        switch (currentPerspective)
        {
            case CameraPerspective.FirstPerson:
                SetPerspective(CameraPerspective.ThirdPerson);
                break;
            case CameraPerspective.ThirdPerson:
                SetPerspective(CameraPerspective.FirstPerson);
                break;
            case CameraPerspective.TopDown:
                SetPerspective(CameraPerspective.FirstPerson);
                break;
            case CameraPerspective.FreeLook:
                SetPerspective(CameraPerspective.FirstPerson);
                break;
        }
    }

    public void SetPerspective(CameraPerspective perspective)
    {
        if (currentPerspective == perspective) return;

        // Start transition
        transitionStartPos = transform.position;
        transitionStartRot = transform.rotation;
        transitionStartFOV = cam != null ? cam.fieldOfView : 60f;
        transitionProgress = 0f;
        isTransitioning = true;

        currentPerspective = perspective;
    }

    void UpdateTransition()
    {
        transitionProgress += Time.deltaTime / transitionDuration;
        
        if (transitionProgress >= 1f)
        {
            transitionProgress = 1f;
            isTransitioning = false;
        }

        float t = Mathf.SmoothStep(0f, 1f, transitionProgress);

        // Calculate target position and rotation for new perspective
        CalculateCameraTransform(out Vector3 targetPos, out Quaternion targetRot, out float targetFOV);

        // Interpolate
        transform.position = Vector3.Lerp(transitionStartPos, targetPos, t);
        transform.rotation = Quaternion.Slerp(transitionStartRot, targetRot, t);
        
        if (cam != null)
        {
            cam.fieldOfView = Mathf.Lerp(transitionStartFOV, targetFOV, t);
        }
    }

    void UpdateCamera()
    {
        if (target == null) return;

        CalculateCameraTransform(out Vector3 targetPos, out Quaternion targetRot, out float targetFOV);

        // Smooth position
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, positionSmoothTime);
        
        // Smooth rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime / rotationSmoothTime);

        // Update FOV
        if (cam != null && Mathf.Abs(cam.fieldOfView - targetFOV) > 0.1f)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 5f);
        }
    }

    void CalculateCameraTransform(out Vector3 position, out Quaternion rotation, out float fov)
    {
        Vector3 targetPoint = target.position + targetOffset;

        switch (currentPerspective)
        {
            case CameraPerspective.FirstPerson:
                position = targetPoint;
                rotation = target.rotation;
                fov = firstPersonFOV;
                break;

            case CameraPerspective.ThirdPerson:
                UpdateOrbitInput();
                Vector3 offset = CalculateThirdPersonOffset();
                position = targetPoint + offset;
                position = CheckCameraCollision(targetPoint, position);
                rotation = Quaternion.LookRotation(targetPoint - position);
                fov = thirdPersonFOV;
                break;

            case CameraPerspective.TopDown:
                position = targetPoint + Vector3.up * topDownHeight;
                rotation = Quaternion.Euler(90f, 0f, 0f);
                fov = topDownFOV;
                break;

            case CameraPerspective.FreeLook:
            default:
                position = transform.position;
                rotation = transform.rotation;
                fov = cam != null ? cam.fieldOfView : 60f;
                break;
        }
    }

    void UpdateOrbitInput()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        orbitAngleX += mouseX * orbitSpeed * Time.deltaTime;
        orbitAngleY = Mathf.Clamp(orbitAngleY - mouseY * orbitSpeed * Time.deltaTime, minOrbitAngle, maxOrbitAngle);
    }

    Vector3 CalculateThirdPersonOffset()
    {
        // Calculate position based on orbit angles
        float radX = orbitAngleX * Mathf.Deg2Rad;
        float radY = orbitAngleY * Mathf.Deg2Rad;

        float x = -Mathf.Sin(radX) * Mathf.Cos(radY) * thirdPersonDistance;
        float y = Mathf.Sin(radY) * thirdPersonDistance + thirdPersonHeight;
        float z = -Mathf.Cos(radX) * Mathf.Cos(radY) * thirdPersonDistance;

        return new Vector3(x, y, z);
    }

    Vector3 CheckCameraCollision(Vector3 targetPoint, Vector3 desiredPosition)
    {
        Vector3 direction = desiredPosition - targetPoint;
        float distance = direction.magnitude;

        if (Physics.Raycast(targetPoint, direction.normalized, out RaycastHit hit, distance, collisionLayers))
        {
            // Move camera closer to avoid clipping
            return hit.point - direction.normalized * collisionPadding;
        }

        return desiredPosition;
    }

    /// <summary>
    /// Sync orbit angles with target rotation (useful when target rotates externally).
    /// </summary>
    public void SyncOrbitWithTarget()
    {
        if (target != null)
        {
            orbitAngleX = target.eulerAngles.y;
        }
    }

    /// <summary>
    /// Set a new target to follow.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        SyncOrbitWithTarget();
    }
}
