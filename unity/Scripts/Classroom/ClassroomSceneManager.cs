using UnityEngine;

/// <summary>
/// Main classroom scene manager that sets up the demo environment.
/// Creates the classroom structure, spawns the player, and manages scene state.
/// This script should be attached to an empty GameObject in the scene root.
/// </summary>
public class ClassroomSceneManager : MonoBehaviour
{
    [Header("Scene References")]
    public GameObject playerPrefab;
    public GameObject classroomRootPrefab;
    public Transform playerSpawnPoint;

    [Header("Runtime References")]
    public PlayerController playerController;
    public OnboardingMenu onboardingMenu;
    public ClassroomBootstrapper classroomBootstrapper;

    [Header("Scene Settings")]
    public bool showOnboardingOnStart = true;
    public bool autoSpawnPlayer = true;
    public Vector3 defaultSpawnPosition = new Vector3(0f, 0f, 5f);
    public Vector3 instructorSpawnPosition = new Vector3(0f, 0f, -2f);

    [Header("Demo Mode")]
    public bool isDemoMode = true;
    public KeyCode reloadSceneKey = KeyCode.R;
    public KeyCode toggleUIKey = KeyCode.Tab;

    private bool isInitialized;
    private GameObject playerInstance;
    private GameObject uiCanvas;

    public static ClassroomSceneManager Instance { get; private set; }

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
    }

    void Start()
    {
        InitializeScene();
    }

    void Update()
    {
        if (!isInitialized) return;

        HandleDemoInputs();
    }

    void InitializeScene()
    {
        // Find or create onboarding menu
        if (onboardingMenu == null)
        {
            onboardingMenu = FindObjectOfType<OnboardingMenu>();
        }

        // Find classroom bootstrapper
        if (classroomBootstrapper == null)
        {
            classroomBootstrapper = FindObjectOfType<ClassroomBootstrapper>();
        }

        // Subscribe to onboarding complete event
        if (onboardingMenu != null)
        {
            onboardingMenu.onOnboardingComplete.AddListener(OnOnboardingComplete);
            
            if (showOnboardingOnStart)
            {
                onboardingMenu.ShowOnboarding();
            }
            else
            {
                OnOnboardingComplete();
            }
        }
        else if (autoSpawnPlayer)
        {
            // No onboarding, spawn player directly
            SpawnPlayer();
        }

        isInitialized = true;
    }

    void OnOnboardingComplete()
    {
        Debug.Log("Onboarding complete, spawning player...");
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        if (playerInstance != null)
        {
            Debug.Log("Player already spawned.");
            return;
        }

        Vector3 spawnPos;
        Quaternion spawnRot = Quaternion.identity;

        // Determine spawn position based on role
        bool isInstructor = onboardingMenu != null && onboardingMenu.IsInstructor();
        
        if (playerSpawnPoint != null)
        {
            spawnPos = playerSpawnPoint.position;
            spawnRot = playerSpawnPoint.rotation;
        }
        else if (isInstructor)
        {
            spawnPos = instructorSpawnPosition;
            spawnRot = Quaternion.Euler(0f, 180f, 0f); // Face the class
        }
        else
        {
            spawnPos = defaultSpawnPosition;
        }

        // Spawn player
        if (playerPrefab != null)
        {
            playerInstance = Instantiate(playerPrefab, spawnPos, spawnRot);
            playerController = playerInstance.GetComponent<PlayerController>();
        }
        else
        {
            // Create a basic player if no prefab is assigned
            playerInstance = CreateBasicPlayer(spawnPos, spawnRot);
            playerController = playerInstance.GetComponent<PlayerController>();
        }

        // Set player name if available
        if (onboardingMenu != null)
        {
            string displayName = onboardingMenu.GetDisplayName();
            playerInstance.name = $"Player_{displayName}";
        }

        Debug.Log($"Player spawned at {spawnPos}");
    }

    GameObject CreateBasicPlayer(Vector3 position, Quaternion rotation)
    {
        // Create player root
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = position;
        player.transform.rotation = rotation;

        // Add character controller
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0, 0.9f, 0);

        // Add player controller
        PlayerController pc = player.AddComponent<PlayerController>();

        // Create camera holder
        GameObject cameraHolder = new GameObject("CameraHolder");
        cameraHolder.transform.SetParent(player.transform);
        cameraHolder.transform.localPosition = new Vector3(0, 1.6f, 0);

        // Create or find main camera
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            camObj.tag = "MainCamera";
            mainCam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        
        mainCam.transform.SetParent(cameraHolder.transform);
        mainCam.transform.localPosition = Vector3.zero;
        mainCam.transform.localRotation = Quaternion.identity;

        pc.playerCamera = mainCam;
        pc.firstPersonCameraPoint = cameraHolder.transform;

        // Create a simple visual representation (capsule)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "PlayerBody";
        body.transform.SetParent(player.transform);
        body.transform.localPosition = new Vector3(0, 0.9f, 0);
        body.transform.localScale = new Vector3(0.5f, 0.9f, 0.5f);
        
        // Remove collider from visual (CharacterController handles collision)
        var bodyCollider = body.GetComponent<Collider>();
        if (bodyCollider != null) Destroy(bodyCollider);

        // Make body semi-transparent in first person
        var bodyRenderer = body.GetComponent<Renderer>();
        if (bodyRenderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.2f, 0.4f, 0.8f, 0.3f);
            SetMaterialTransparent(mat);
            bodyRenderer.material = mat;
        }

        return player;
    }

    // Standard shader rendering mode constants
    private const float STANDARD_SHADER_MODE_TRANSPARENT = 3f;
    private const int TRANSPARENT_RENDER_QUEUE = 3000;

    void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Mode", STANDARD_SHADER_MODE_TRANSPARENT);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = TRANSPARENT_RENDER_QUEUE;
    }

    void HandleDemoInputs()
    {
        if (!isDemoMode) return;

        // Reload scene
        if (Input.GetKeyDown(reloadSceneKey))
        {
            ReloadScene();
        }

        // Toggle UI
        if (Input.GetKeyDown(toggleUIKey))
        {
            ToggleUI();
        }
    }

    public void ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    void ToggleUI()
    {
        if (uiCanvas == null)
        {
            uiCanvas = GameObject.Find("Canvas");
        }

        if (uiCanvas != null)
        {
            uiCanvas.SetActive(!uiCanvas.activeSelf);
        }
    }

    /// <summary>
    /// Teleport the player to a specific transform.
    /// </summary>
    public void TeleportPlayerTo(Transform destination)
    {
        if (playerController != null && destination != null)
        {
            playerController.TeleportTo(destination.position, destination.rotation);
        }
    }

    /// <summary>
    /// Get the current player instance.
    /// </summary>
    public GameObject GetPlayer()
    {
        return playerInstance;
    }

    /// <summary>
    /// Check if the scene is fully initialized.
    /// </summary>
    public bool IsReady()
    {
        return isInitialized && playerInstance != null;
    }

    void OnDestroy()
    {
        if (onboardingMenu != null)
        {
            onboardingMenu.onOnboardingComplete.RemoveListener(OnOnboardingComplete);
        }
    }
}
