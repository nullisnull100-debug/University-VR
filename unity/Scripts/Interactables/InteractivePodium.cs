using UnityEngine;

/// <summary>
/// Interactive podium for the instructor.
/// Provides controls for presentation mode and whiteboard access.
/// </summary>
public class InteractivePodium : InteractableBase
{
    [Header("Podium Settings")]
    public Transform presentationPosition;
    public WhiteboardInteractable whiteboard;
    public bool isPresentationMode;

    [Header("Controls")]
    public KeyCode nextSlideKey = KeyCode.RightArrow;
    public KeyCode prevSlideKey = KeyCode.LeftArrow;
    public KeyCode pointerKey = KeyCode.P;

    private int currentSlide;
    private bool isUsingPodium;
    private PlayerController activePresenter;

    protected override void Awake()
    {
        base.Awake();
        interactionPrompt = "Press E to present";
    }

    protected override void Update()
    {
        base.Update();

        if (isUsingPodium && isPresentationMode)
        {
            HandlePresentationInput();
        }
    }

    protected override void OnInteract()
    {
        base.OnInteract();

        var player = FindObjectOfType<PlayerController>();
        if (player == null) return;

        if (!isUsingPodium)
        {
            StartPresentation(player);
        }
        else
        {
            EndPresentation();
        }
    }

    void StartPresentation(PlayerController player)
    {
        activePresenter = player;
        isUsingPodium = true;
        isPresentationMode = true;

        // Move player to presentation position
        if (presentationPosition != null)
        {
            player.TeleportTo(presentationPosition.position, presentationPosition.rotation);
        }

        interactionPrompt = "Press E to stop presenting";
        Debug.Log("Presentation mode started");
    }

    void EndPresentation()
    {
        if (activePresenter != null)
        {
            // Move player away from podium
            Vector3 exitPos = transform.position + transform.forward * 1.5f;
            activePresenter.TeleportTo(exitPos, Quaternion.identity);
        }

        activePresenter = null;
        isUsingPodium = false;
        isPresentationMode = false;
        interactionPrompt = "Press E to present";

        Debug.Log("Presentation mode ended");
    }

    void HandlePresentationInput()
    {
        if (Input.GetKeyDown(nextSlideKey))
        {
            NextSlide();
        }
        else if (Input.GetKeyDown(prevSlideKey))
        {
            PreviousSlide();
        }
        else if (Input.GetKeyDown(pointerKey))
        {
            TogglePointer();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndPresentation();
        }
    }

    void NextSlide()
    {
        currentSlide++;
        Debug.Log($"Slide {currentSlide}");
        // In a real implementation, this would update the whiteboard/display
    }

    void PreviousSlide()
    {
        currentSlide = Mathf.Max(0, currentSlide - 1);
        Debug.Log($"Slide {currentSlide}");
    }

    void TogglePointer()
    {
        Debug.Log("Pointer toggled");
        // In a real implementation, this would show/hide a laser pointer
    }

    /// <summary>
    /// Clear the whiteboard if connected.
    /// </summary>
    public void ClearWhiteboard()
    {
        if (whiteboard != null)
        {
            whiteboard.ClearBoard();
        }
    }

    /// <summary>
    /// Check if someone is currently presenting.
    /// </summary>
    public bool IsPresenting()
    {
        return isPresentationMode && isUsingPodium;
    }
}
