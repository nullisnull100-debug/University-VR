using UnityEngine;

/// <summary>
/// Simple non-XR avatar controller for demo mode.
/// WASD movement + mouse look. Attach to a GameObject that contains the Camera (head).
/// Use ToggleToInstructor() to snap the avatar to a presentation position.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class AvatarSimple : MonoBehaviour
{
    public Transform headTransform;
    public float moveSpeed = 2.5f;
    public float runMultiplier = 1.8f;
    public float mouseSensitivity = 2.0f;
    public Transform instructorViewPoint; // optional: an empty transform with desired instructor camera pose

    CharacterController controller;
    float pitch = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch - mouseY, -70f, 70f);
        if (headTransform != null)
            headTransform.localEulerAngles = Vector3.right * pitch;
        transform.Rotate(Vector3.up * mouseX);

        // Movement
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 forward = transform.forward * v;
        Vector3 right = transform.right * h;
        Vector3 motion = (forward + right).normalized;
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? runMultiplier : 1f);
        controller.SimpleMove(motion * speed);
    }

    public void ToggleToInstructor()
    {
        if (instructorViewPoint == null) return;
        // Snap the whole avatar (position and rotation) to instructor view
        transform.position = instructorViewPoint.position;
        transform.rotation = instructorViewPoint.rotation;
        if (headTransform != null)
        {
            headTransform.localRotation = Quaternion.identity;
        }
    }
}
