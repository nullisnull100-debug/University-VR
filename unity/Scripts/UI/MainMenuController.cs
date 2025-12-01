using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Main menu controller for the VR Classroom demo.
/// Provides options to enter classroom, change settings, and manage profile.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainMenuPanel;
    public Button enterClassroomButton;
    public Button settingsButton;
    public Button editProfileButton;
    public Button quitButton;
    public Text welcomeText;

    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public Slider mouseSensitivitySlider;
    public Slider volumeSlider;
    public Toggle fullscreenToggle;
    public Dropdown qualityDropdown;
    public Button closeSettingsButton;

    [Header("Events")]
    public UnityEvent onEnterClassroom;

    [Header("References")]
    public OnboardingMenu onboardingMenu;
    public ClassroomSceneManager sceneManager;

    void Start()
    {
        SetupUI();
        UpdateWelcomeText();
    }

    void SetupUI()
    {
        // Main menu buttons
        if (enterClassroomButton != null)
        {
            enterClassroomButton.onClick.AddListener(OnEnterClassroom);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(ShowSettings);
        }

        if (editProfileButton != null)
        {
            editProfileButton.onClick.AddListener(OnEditProfile);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuit);
        }

        // Settings panel
        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.AddListener(HideSettings);
        }

        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 2.0f);
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1.0f);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = Screen.fullScreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
            qualityDropdown.value = QualitySettings.GetQualityLevel();
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        }

        // Initially hide settings panel
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    void UpdateWelcomeText()
    {
        if (welcomeText == null) return;

        string displayName = "Guest";
        if (onboardingMenu != null)
        {
            displayName = onboardingMenu.GetDisplayName();
        }

        welcomeText.text = $"Welcome, {displayName}!";
    }

    public void OnEnterClassroom()
    {
        Debug.Log("Entering classroom...");
        
        // Hide main menu
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Trigger event
        onEnterClassroom?.Invoke();
    }

    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
    }

    public void HideSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    public void OnEditProfile()
    {
        if (onboardingMenu != null)
        {
            onboardingMenu.ResetProfile();
        }
    }

    public void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnMouseSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();

        // Apply to player controller if available
        if (sceneManager != null && sceneManager.playerController != null)
        {
            sceneManager.playerController.mouseSensitivity = value;
        }
    }

    void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("Volume", value);
        PlayerPrefs.Save();
        AudioListener.volume = value;
    }

    void OnFullscreenChanged(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    void OnQualityChanged(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex, true);
    }

    /// <summary>
    /// Show the main menu and unlock cursor.
    /// </summary>
    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdateWelcomeText();
    }
}
