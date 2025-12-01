using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Onboarding menu controller that collects user information before entering the classroom.
/// Collects: Name, Age, School, Major, and optional preferences.
/// Wire UI elements in the Inspector and call OnSubmit() when the user completes the form.
/// </summary>
public class OnboardingMenu : MonoBehaviour
{
    [System.Serializable]
    public class UserProfile
    {
        public string displayName;
        public int age;
        public string schoolName;
        public string major;
        public string role; // "Student" or "Instructor"
        public bool hasCompletedOnboarding;
    }

    [Header("UI References")]
    public InputField nameInputField;
    public InputField ageInputField;
    public InputField schoolInputField;
    public InputField majorInputField;
    public Dropdown roleDropdown;
    public Button submitButton;
    public Button skipButton;
    public Text errorText;

    [Header("Panels")]
    public GameObject onboardingPanel;
    public GameObject mainMenuPanel;

    [Header("Events")]
    public UnityEvent onOnboardingComplete;

    [Header("Settings")]
    public int minimumAge = 13;
    public int maximumAge = 100;
    public bool allowSkip = true;

    private UserProfile currentProfile;

    public static OnboardingMenu Instance { get; private set; }
    public UserProfile CurrentProfile => currentProfile;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentProfile = new UserProfile();
        LoadSavedProfile();
    }

    void Start()
    {
        SetupUI();
        
        // Check if user has already completed onboarding
        if (currentProfile.hasCompletedOnboarding)
        {
            // Skip onboarding and go directly to main menu
            ShowMainMenu();
        }
        else
        {
            ShowOnboarding();
        }
    }

    void SetupUI()
    {
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmit);
        }

        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(allowSkip);
            skipButton.onClick.AddListener(OnSkip);
        }

        if (roleDropdown != null)
        {
            roleDropdown.ClearOptions();
            roleDropdown.AddOptions(new System.Collections.Generic.List<string> { "Student", "Instructor", "Observer" });
        }

        if (errorText != null)
        {
            errorText.text = "";
        }

        // Pre-fill saved data if available
        if (!string.IsNullOrEmpty(currentProfile.displayName) && nameInputField != null)
        {
            nameInputField.text = currentProfile.displayName;
        }
        if (currentProfile.age > 0 && ageInputField != null)
        {
            ageInputField.text = currentProfile.age.ToString();
        }
        if (!string.IsNullOrEmpty(currentProfile.schoolName) && schoolInputField != null)
        {
            schoolInputField.text = currentProfile.schoolName;
        }
        if (!string.IsNullOrEmpty(currentProfile.major) && majorInputField != null)
        {
            majorInputField.text = currentProfile.major;
        }
    }

    public void ShowOnboarding()
    {
        if (onboardingPanel != null) onboardingPanel.SetActive(true);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        
        // Unlock cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowMainMenu()
    {
        if (onboardingPanel != null) onboardingPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void OnSubmit()
    {
        if (!ValidateInputs())
        {
            return;
        }

        // Collect data
        currentProfile.displayName = nameInputField != null ? nameInputField.text.Trim() : "Guest";
        
        if (ageInputField != null && int.TryParse(ageInputField.text, out int parsedAge))
        {
            currentProfile.age = parsedAge;
        }
        
        currentProfile.schoolName = schoolInputField != null ? schoolInputField.text.Trim() : "";
        currentProfile.major = majorInputField != null ? majorInputField.text.Trim() : "";
        
        if (roleDropdown != null)
        {
            currentProfile.role = roleDropdown.options[roleDropdown.value].text;
        }
        else
        {
            currentProfile.role = "Student";
        }

        currentProfile.hasCompletedOnboarding = true;

        // Save profile
        SaveProfile();

        // Trigger completion event
        onOnboardingComplete?.Invoke();

        // Transition to main menu or classroom
        ShowMainMenu();

        Debug.Log($"Onboarding complete: {currentProfile.displayName}, Age: {currentProfile.age}, " +
                  $"School: {currentProfile.schoolName}, Major: {currentProfile.major}, Role: {currentProfile.role}");
    }

    public void OnSkip()
    {
        // Create a guest profile
        currentProfile.displayName = "Guest";
        currentProfile.age = 0;
        currentProfile.schoolName = "";
        currentProfile.major = "";
        currentProfile.role = "Student";
        currentProfile.hasCompletedOnboarding = false; // Will show again next time

        onOnboardingComplete?.Invoke();
        ShowMainMenu();

        Debug.Log("Onboarding skipped, using guest profile");
    }

    bool ValidateInputs()
    {
        // Validate name
        if (nameInputField != null && string.IsNullOrWhiteSpace(nameInputField.text))
        {
            ShowError("Please enter your name.");
            return false;
        }

        // Validate age
        if (ageInputField != null)
        {
            if (!int.TryParse(ageInputField.text, out int age))
            {
                ShowError("Please enter a valid age.");
                return false;
            }
            if (age < minimumAge || age > maximumAge)
            {
                ShowError($"Age must be between {minimumAge} and {maximumAge}.");
                return false;
            }
        }

        // Validate school (optional but if provided, should be reasonable)
        if (schoolInputField != null && !string.IsNullOrEmpty(schoolInputField.text))
        {
            if (schoolInputField.text.Trim().Length < 2)
            {
                ShowError("Please enter a valid school name.");
                return false;
            }
        }

        ClearError();
        return true;
    }

    void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.color = Color.red;
        }
        Debug.LogWarning($"Onboarding validation error: {message}");
    }

    void ClearError()
    {
        if (errorText != null)
        {
            errorText.text = "";
        }
    }

    void SaveProfile()
    {
        PlayerPrefs.SetString("UserProfile_Name", currentProfile.displayName);
        PlayerPrefs.SetInt("UserProfile_Age", currentProfile.age);
        PlayerPrefs.SetString("UserProfile_School", currentProfile.schoolName);
        PlayerPrefs.SetString("UserProfile_Major", currentProfile.major);
        PlayerPrefs.SetString("UserProfile_Role", currentProfile.role);
        PlayerPrefs.SetInt("UserProfile_Completed", currentProfile.hasCompletedOnboarding ? 1 : 0);
        PlayerPrefs.Save();
    }

    void LoadSavedProfile()
    {
        currentProfile.displayName = PlayerPrefs.GetString("UserProfile_Name", "");
        currentProfile.age = PlayerPrefs.GetInt("UserProfile_Age", 0);
        currentProfile.schoolName = PlayerPrefs.GetString("UserProfile_School", "");
        currentProfile.major = PlayerPrefs.GetString("UserProfile_Major", "");
        currentProfile.role = PlayerPrefs.GetString("UserProfile_Role", "Student");
        currentProfile.hasCompletedOnboarding = PlayerPrefs.GetInt("UserProfile_Completed", 0) == 1;
    }

    /// <summary>
    /// Reset the user profile and show onboarding again.
    /// </summary>
    public void ResetProfile()
    {
        PlayerPrefs.DeleteKey("UserProfile_Name");
        PlayerPrefs.DeleteKey("UserProfile_Age");
        PlayerPrefs.DeleteKey("UserProfile_School");
        PlayerPrefs.DeleteKey("UserProfile_Major");
        PlayerPrefs.DeleteKey("UserProfile_Role");
        PlayerPrefs.DeleteKey("UserProfile_Completed");
        PlayerPrefs.Save();

        currentProfile = new UserProfile();
        ShowOnboarding();
    }

    /// <summary>
    /// Get the current user's display name.
    /// </summary>
    public string GetDisplayName()
    {
        return string.IsNullOrEmpty(currentProfile.displayName) ? "Guest" : currentProfile.displayName;
    }

    /// <summary>
    /// Check if the user is an instructor.
    /// </summary>
    public bool IsInstructor()
    {
        return currentProfile.role == "Instructor";
    }
}
