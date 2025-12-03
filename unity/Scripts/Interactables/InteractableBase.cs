using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Base class for interactive objects in the VR classroom.
/// Provides common functionality for hover, select, and interact behaviors.
/// Inherit from this class to create custom interactable objects.
/// </summary>
public class InteractableBase : MonoBehaviour
{
    [Header("Interaction Settings")]
    public bool isInteractable = true;
    public float interactionDistance = 3.0f;
    public string interactionPrompt = "Press E to interact";
    public KeyCode interactKey = KeyCode.E;

    [Header("Visual Feedback")]
    public Color highlightColor = new Color(1f, 1f, 0.5f, 1f);
    public bool useHighlight = true;

    [Header("Events")]
    public UnityEvent onHoverEnter;
    public UnityEvent onHoverExit;
    public UnityEvent onInteract;

    protected bool isHovered;
    protected bool isSelected;
    protected Renderer objectRenderer;
    protected Color originalColor;
    protected Material originalMaterial;

    protected virtual void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null && objectRenderer.material != null)
        {
            originalMaterial = objectRenderer.material;
            if (originalMaterial.HasProperty("_Color"))
            {
                originalColor = originalMaterial.color;
            }
        }
    }

    protected virtual void Update()
    {
        if (!isInteractable) return;

        CheckInteraction();
    }

    protected virtual void CheckInteraction()
    {
        // Check if player is looking at this object
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        bool wasHovered = isHovered;
        isHovered = false;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                isHovered = true;

                // Check for interaction input
                if (Input.GetKeyDown(interactKey))
                {
                    OnInteract();
                }
            }
        }

        // Handle hover state changes
        if (isHovered && !wasHovered)
        {
            OnHoverEnter();
        }
        else if (!isHovered && wasHovered)
        {
            OnHoverExit();
        }
    }

    protected virtual void OnHoverEnter()
    {
        if (useHighlight && objectRenderer != null && objectRenderer.material.HasProperty("_Color"))
        {
            objectRenderer.material.color = highlightColor;
        }
        onHoverEnter?.Invoke();
    }

    protected virtual void OnHoverExit()
    {
        if (useHighlight && objectRenderer != null && objectRenderer.material.HasProperty("_Color"))
        {
            objectRenderer.material.color = originalColor;
        }
        onHoverExit?.Invoke();
    }

    protected virtual void OnInteract()
    {
        onInteract?.Invoke();
    }

    /// <summary>
    /// Enable or disable interaction.
    /// </summary>
    public void SetInteractable(bool canInteract)
    {
        isInteractable = canInteract;
        if (!canInteract && isHovered)
        {
            OnHoverExit();
            isHovered = false;
        }
    }

    /// <summary>
    /// Get the interaction prompt text.
    /// </summary>
    public string GetPrompt()
    {
        return isHovered ? interactionPrompt : "";
    }
}
