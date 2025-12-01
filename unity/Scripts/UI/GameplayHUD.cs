using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// In-game HUD controller that shows interaction prompts, camera mode, and other status info.
/// </summary>
public class GameplayHUD : MonoBehaviour
{
    [Header("UI References")]
    public Text interactionPromptText;
    public Text cameraModeText;
    public Text playerInfoText;
    public Text controlsHintText;
    public GameObject crosshair;
    public GameObject pausePanel;

    [Header("Settings")]
    public bool showControlsHint = true;
    public float controlsHintDuration = 10f;

    [Header("References")]
    public PlayerController playerController;
    public OnboardingMenu onboardingMenu;

    private InteractableBase currentInteractable;
    private bool isPaused;
    private float controlsHintTimer;

    void Start()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        if (onboardingMenu == null)
        {
            onboardingMenu = FindObjectOfType<OnboardingMenu>();
        }

        UpdatePlayerInfo();
        ShowControlsHint();

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    void Update()
    {
        UpdateInteractionPrompt();
        UpdateCameraModeDisplay();
        HandlePause();
        UpdateControlsHint();
    }

    void UpdateInteractionPrompt()
    {
        if (interactionPromptText == null) return;

        // Find what the player is looking at
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 5f))
        {
            var interactable = hit.collider.GetComponent<InteractableBase>();
            if (interactable != null && interactable.isInteractable)
            {
                currentInteractable = interactable;
                interactionPromptText.text = interactable.GetPrompt();
                interactionPromptText.enabled = true;
                return;
            }
        }

        currentInteractable = null;
        interactionPromptText.text = "";
        interactionPromptText.enabled = false;
    }

    void UpdateCameraModeDisplay()
    {
        if (cameraModeText == null || playerController == null) return;

        string modeText = playerController.currentCameraMode == PlayerController.CameraMode.FirstPerson 
            ? "First Person" 
            : "Third Person";
        
        cameraModeText.text = $"View: {modeText} (V to switch)";
    }

    void UpdatePlayerInfo()
    {
        if (playerInfoText == null) return;

        string playerName = "Guest";
        if (onboardingMenu != null)
        {
            playerName = onboardingMenu.GetDisplayName();
        }

        playerInfoText.text = playerName;
    }

    void ShowControlsHint()
    {
        if (!showControlsHint || controlsHintText == null) return;

        controlsHintText.text = "Controls:\n" +
            "WASD - Move\n" +
            "Mouse - Look\n" +
            "Shift - Run\n" +
            "Space - Jump\n" +
            "V - Toggle Camera\n" +
            "E - Interact\n" +
            "Escape - Menu";
        
        controlsHintText.enabled = true;
        controlsHintTimer = controlsHintDuration;
    }

    void UpdateControlsHint()
    {
        if (controlsHintText == null || controlsHintTimer <= 0) return;

        controlsHintTimer -= Time.deltaTime;
        if (controlsHintTimer <= 0)
        {
            controlsHintText.enabled = false;
        }
        else if (controlsHintTimer < 2f)
        {
            // Fade out
            var color = controlsHintText.color;
            color.a = controlsHintTimer / 2f;
            controlsHintText.color = color;
        }
    }

    void HandlePause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }

        if (isPaused)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void Resume()
    {
        if (isPaused)
        {
            TogglePause();
        }
    }

    public void ShowCrosshair(bool show)
    {
        if (crosshair != null)
        {
            crosshair.SetActive(show);
        }
    }

    /// <summary>
    /// Show a temporary message on screen.
    /// </summary>
    public void ShowMessage(string message, float duration = 3f)
    {
        if (interactionPromptText != null)
        {
            interactionPromptText.text = message;
            interactionPromptText.enabled = true;
            Invoke(nameof(ClearMessage), duration);
        }
    }

    void ClearMessage()
    {
        if (interactionPromptText != null)
        {
            interactionPromptText.text = "";
        }
    }
}
