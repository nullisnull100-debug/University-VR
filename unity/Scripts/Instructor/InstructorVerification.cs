using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Handles instructor verification through multiple methods:
/// 1. Secret code provided by school/company
/// 2. Badge/QR code scanning
/// 3. Institutional email verification
/// </summary>
public class InstructorVerification : MonoBehaviour
{
    [System.Serializable]
    public class InstructorCredentials
    {
        public string instructorId;
        public string institutionCode;
        public string verificationToken;
        public bool isVerified;
        public System.DateTime verificationTime;
        public string verificationMethod; // "secret_code", "badge_scan", "email"
    }

    [Header("UI References")]
    public GameObject verificationPanel;
    public InputField secretCodeInput;
    public InputField institutionCodeInput;
    public InputField emailInput;
    public Button verifySecretButton;
    public Button scanBadgeButton;
    public Button verifyEmailButton;
    public Text statusText;
    public Text errorText;

    [Header("Badge Scanning")]
    public GameObject badgeScannerPanel;
    public RawImage cameraPreview;
    public Text scanInstructionsText;

    [Header("Settings")]
    public string validInstitutionPrefix = "EDU";
    public int secretCodeLength = 8;
    public bool requireEmailVerification = false;

    [Header("Events")]
    public UnityEvent onVerificationSuccess;
    public UnityEvent onVerificationFailed;
    public UnityEvent<InstructorCredentials> onCredentialsVerified;

    private InstructorCredentials currentCredentials;
    private WebCamTexture webCamTexture;
    private bool isScanning;

    public static InstructorVerification Instance { get; private set; }
    public InstructorCredentials Credentials => currentCredentials;
    public bool IsVerified => currentCredentials?.isVerified ?? false;

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

        currentCredentials = new InstructorCredentials();
        LoadSavedCredentials();
    }

    void Start()
    {
        SetupUI();
    }

    void SetupUI()
    {
        if (verifySecretButton != null)
        {
            verifySecretButton.onClick.AddListener(VerifySecretCode);
        }

        if (scanBadgeButton != null)
        {
            scanBadgeButton.onClick.AddListener(StartBadgeScan);
        }

        if (verifyEmailButton != null)
        {
            verifyEmailButton.onClick.AddListener(SendEmailVerification);
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (statusText != null)
        {
            statusText.text = IsVerified 
                ? $"✓ Verified as Instructor ({currentCredentials.verificationMethod})"
                : "Not verified - Please verify your instructor credentials";
        }
    }

    /// <summary>
    /// Show the verification panel.
    /// </summary>
    public void ShowVerificationPanel()
    {
        if (verificationPanel != null)
        {
            verificationPanel.SetActive(true);
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Hide the verification panel.
    /// </summary>
    public void HideVerificationPanel()
    {
        if (verificationPanel != null)
        {
            verificationPanel.SetActive(false);
        }

        StopBadgeScan();
    }

    /// <summary>
    /// Verify instructor using a secret code provided by institution.
    /// </summary>
    public void VerifySecretCode()
    {
        ClearError();

        string secretCode = secretCodeInput?.text?.Trim() ?? "";
        string institutionCode = institutionCodeInput?.text?.Trim() ?? "";

        if (string.IsNullOrEmpty(secretCode))
        {
            ShowError("Please enter your secret verification code.");
            return;
        }

        if (string.IsNullOrEmpty(institutionCode))
        {
            ShowError("Please enter your institution code.");
            return;
        }

        // Validate institution code format
        if (!institutionCode.StartsWith(validInstitutionPrefix))
        {
            ShowError($"Invalid institution code. Must start with '{validInstitutionPrefix}'.");
            return;
        }

        // Validate secret code length
        if (secretCode.Length < secretCodeLength)
        {
            ShowError($"Secret code must be at least {secretCodeLength} characters.");
            return;
        }

        // Generate verification token (hash of secret + institution)
        string verificationToken = GenerateVerificationToken(secretCode, institutionCode);

        // In production, this would verify against a backend server
        // For demo purposes, we accept codes that pass validation
        if (ValidateSecretCode(secretCode, institutionCode))
        {
            CompleteVerification("secret_code", institutionCode, verificationToken);
        }
        else
        {
            ShowError("Invalid verification code. Please contact your institution.");
            onVerificationFailed?.Invoke();
        }
    }

    /// <summary>
    /// Start badge/QR code scanning using device camera.
    /// </summary>
    public void StartBadgeScan()
    {
        if (badgeScannerPanel != null)
        {
            badgeScannerPanel.SetActive(true);
        }

        // Start camera for badge scanning
        if (WebCamTexture.devices.Length > 0)
        {
            webCamTexture = new WebCamTexture();
            
            if (cameraPreview != null)
            {
                cameraPreview.texture = webCamTexture;
            }
            
            webCamTexture.Play();
            isScanning = true;

            if (scanInstructionsText != null)
            {
                scanInstructionsText.text = "Hold your instructor badge up to the camera...";
            }

            // Stop any existing scan coroutine and start new one
            StopCoroutine(nameof(ScanForBadge));
            scanCoroutine = StartCoroutine(ScanForBadge());
        }
        else
        {
            ShowError("No camera available for badge scanning.");
        }
    }

    private Coroutine scanCoroutine;

    /// <summary>
    /// Stop badge scanning.
    /// </summary>
    public void StopBadgeScan()
    {
        isScanning = false;

        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }

        if (badgeScannerPanel != null)
        {
            badgeScannerPanel.SetActive(false);
        }
    }

    System.Collections.IEnumerator ScanForBadge()
    {
        while (isScanning)
        {
            // In production, this would use a QR code library to scan
            // For demo, we simulate scanning after a delay
            yield return new WaitForSeconds(0.5f);

            // Simulate QR code detection
            // In real implementation, use ZXing or similar library
            string scannedCode = SimulateBadgeScan();
            
            if (!string.IsNullOrEmpty(scannedCode))
            {
                ProcessBadgeCode(scannedCode);
                yield break;
            }
        }
    }

    /// <summary>
    /// Placeholder for QR code scanning.
    /// In production, integrate with a QR library like ZXing.
    /// For demo purposes, this always returns null (no detection).
    /// To test badge scanning manually, call EnterBadgeCodeManually() with a test code.
    /// Example: "INST:EDU_UNIVERSITY:PROF123:TOKEN456"
    /// </summary>
    string SimulateBadgeScan()
    {
        // TODO: Integrate QR code library (ZXing, ZBar, or similar)
        // The library would decode the camera feed and return the QR code content
        return null;
    }

    void ProcessBadgeCode(string badgeCode)
    {
        StopBadgeScan();

        // Parse badge code (expected format: INST:institution_code:instructor_id:token)
        string[] parts = badgeCode.Split(':');
        
        if (parts.Length >= 4 && parts[0] == "INST")
        {
            string institutionCode = parts[1];
            string instructorId = parts[2];
            string token = parts[3];

            currentCredentials.instructorId = instructorId;
            CompleteVerification("badge_scan", institutionCode, token);
        }
        else
        {
            ShowError("Invalid badge code. Please try again or use another verification method.");
            onVerificationFailed?.Invoke();
        }
    }

    /// <summary>
    /// Send email verification link to institutional email.
    /// </summary>
    public void SendEmailVerification()
    {
        ClearError();

        string email = emailInput?.text?.Trim() ?? "";

        if (string.IsNullOrEmpty(email))
        {
            ShowError("Please enter your institutional email address.");
            return;
        }

        // Basic email validation
        if (!email.Contains("@") || !email.Contains("."))
        {
            ShowError("Please enter a valid email address.");
            return;
        }

        // Check for institutional email domain
        if (!IsInstitutionalEmail(email))
        {
            ShowError("Please use your official institutional email (.edu or approved domain).");
            return;
        }

        // In production, this would send an actual verification email
        ShowStatus($"Verification email sent to {email}. Please check your inbox.");
        
        // For demo, auto-verify after showing message
        string verificationToken = GenerateVerificationToken(email, System.DateTime.UtcNow.ToString());
        
        // Simulate email verification delay
        StartCoroutine(SimulateEmailVerification(email, verificationToken));
    }

    System.Collections.IEnumerator SimulateEmailVerification(string email, string token)
    {
        yield return new WaitForSeconds(2f);
        
        // Extract institution from email domain
        string domain = email.Split('@')[1];
        string institutionCode = "EDU_" + domain.Split('.')[0].ToUpper();
        
        CompleteVerification("email", institutionCode, token);
    }

    bool IsInstitutionalEmail(string email)
    {
        string domain = email.ToLower();
        return domain.EndsWith(".edu") || 
               domain.Contains("university") || 
               domain.Contains("college") ||
               domain.Contains("school");
    }

    void CompleteVerification(string method, string institutionCode, string token)
    {
        currentCredentials.institutionCode = institutionCode;
        currentCredentials.verificationToken = token;
        currentCredentials.isVerified = true;
        currentCredentials.verificationTime = System.DateTime.UtcNow;
        currentCredentials.verificationMethod = method;

        SaveCredentials();
        UpdateUI();
        
        ShowStatus("✓ Verification successful! You now have instructor privileges.");
        
        onVerificationSuccess?.Invoke();
        onCredentialsVerified?.Invoke(currentCredentials);

        // Hide panel after short delay
        StartCoroutine(HideAfterDelay(2f));
    }

    System.Collections.IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideVerificationPanel();
    }

    bool ValidateSecretCode(string secretCode, string institutionCode)
    {
        // In production, validate against backend server
        // For demo, accept codes that meet format requirements
        return secretCode.Length >= secretCodeLength && 
               institutionCode.StartsWith(validInstitutionPrefix);
    }

    string GenerateVerificationToken(string input1, string input2)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            // Use cryptographically secure random bytes for better security
            byte[] randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            
            string combined = input1 + input2 + System.Convert.ToBase64String(randomBytes);
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return System.Convert.ToBase64String(bytes).Substring(0, 32);
        }
    }

    void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.color = Color.red;
        }
        Debug.LogWarning($"Instructor verification error: {message}");
    }

    void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = Color.green;
        }
        Debug.Log($"Instructor verification: {message}");
    }

    void ClearError()
    {
        if (errorText != null)
        {
            errorText.text = "";
        }
    }

    void SaveCredentials()
    {
        PlayerPrefs.SetString("Instructor_Id", currentCredentials.instructorId ?? "");
        PlayerPrefs.SetString("Instructor_Institution", currentCredentials.institutionCode ?? "");
        PlayerPrefs.SetString("Instructor_Token", currentCredentials.verificationToken ?? "");
        PlayerPrefs.SetInt("Instructor_Verified", currentCredentials.isVerified ? 1 : 0);
        PlayerPrefs.SetString("Instructor_Method", currentCredentials.verificationMethod ?? "");
        PlayerPrefs.SetString("Instructor_Time", currentCredentials.verificationTime.ToString("o"));
        PlayerPrefs.Save();
    }

    void LoadSavedCredentials()
    {
        currentCredentials.instructorId = PlayerPrefs.GetString("Instructor_Id", "");
        currentCredentials.institutionCode = PlayerPrefs.GetString("Instructor_Institution", "");
        currentCredentials.verificationToken = PlayerPrefs.GetString("Instructor_Token", "");
        currentCredentials.isVerified = PlayerPrefs.GetInt("Instructor_Verified", 0) == 1;
        currentCredentials.verificationMethod = PlayerPrefs.GetString("Instructor_Method", "");
        
        string timeStr = PlayerPrefs.GetString("Instructor_Time", "");
        if (!string.IsNullOrEmpty(timeStr))
        {
            System.DateTime.TryParse(timeStr, out currentCredentials.verificationTime);
        }

        // Check if verification has expired (e.g., after 30 days)
        if (currentCredentials.isVerified)
        {
            var elapsed = System.DateTime.UtcNow - currentCredentials.verificationTime;
            if (elapsed.TotalDays > 30)
            {
                currentCredentials.isVerified = false;
                SaveCredentials();
            }
        }
    }

    /// <summary>
    /// Clear all saved credentials and require re-verification.
    /// </summary>
    public void ClearCredentials()
    {
        PlayerPrefs.DeleteKey("Instructor_Id");
        PlayerPrefs.DeleteKey("Instructor_Institution");
        PlayerPrefs.DeleteKey("Instructor_Token");
        PlayerPrefs.DeleteKey("Instructor_Verified");
        PlayerPrefs.DeleteKey("Instructor_Method");
        PlayerPrefs.DeleteKey("Instructor_Time");
        PlayerPrefs.Save();

        currentCredentials = new InstructorCredentials();
        UpdateUI();
    }

    /// <summary>
    /// Manual badge code entry for testing or when camera isn't available.
    /// </summary>
    public void EnterBadgeCodeManually(string code)
    {
        ProcessBadgeCode(code);
    }
}
