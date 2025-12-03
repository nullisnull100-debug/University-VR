using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

/// <summary>
/// Instructor admin panel for classroom management.
/// Provides controls for:
/// - Muting/unmuting students
/// - Kicking students from class
/// - Blocking access to materials
/// - Monitoring student engagement
/// - Changing classroom environment
/// - Managing attendance and grades
/// </summary>
public class InstructorAdminPanel : MonoBehaviourPunCallbacks
{
    [System.Serializable]
    public class StudentInfo
    {
        public int actorNumber;
        public string displayName;
        public bool isMuted;
        public bool isBlocked;
        public float engagementScore;
        public bool isTabFocused;
        public bool isPresent;
        public string grade;
        public System.DateTime joinTime;
        public float totalTimeInClass;
    }

    [Header("UI References")]
    public GameObject adminPanel;
    public Transform studentListContainer;
    public GameObject studentListItemPrefab;
    public Button muteAllButton;
    public Button unmuteAllButton;
    public Button kickSelectedButton;
    public Dropdown environmentDropdown;
    public Button changeEnvironmentButton;
    public Toggle materialAccessToggle;
    public Text classStatusText;

    [Header("Attendance Panel")]
    public GameObject attendancePanel;
    public Transform attendanceListContainer;
    public Button markAttendanceButton;
    public Button exportAttendanceButton;

    [Header("Grades Panel")]
    public GameObject gradesPanel;
    public Transform gradesListContainer;
    public Dropdown gradeDropdown;
    public Button assignGradeButton;

    [Header("Engagement Monitoring")]
    public GameObject engagementPanel;
    public Text engagementSummaryText;
    public float engagementUpdateInterval = 5f;

    [Header("Settings")]
    public string[] availableEnvironments = { "Classroom", "Library", "Auditorium", "Lab", "Outdoor Campus" };
    public string[] gradeOptions = { "A", "A-", "B+", "B", "B-", "C+", "C", "C-", "D", "F", "Incomplete" };

    [Header("Events")]
    public UnityEvent<int> onStudentMuted;
    public UnityEvent<int> onStudentKicked;
    public UnityEvent<string> onEnvironmentChanged;

    private Dictionary<int, StudentInfo> students = new Dictionary<int, StudentInfo>();
    private List<int> selectedStudents = new List<int>();
    private string currentEnvironment = "Classroom";
    private bool materialsBlocked = false;

    public static InstructorAdminPanel Instance { get; private set; }
    public bool IsAdminPanelOpen => adminPanel != null && adminPanel.activeSelf;

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
        SetupUI();
        
        // Start engagement monitoring
        InvokeRepeating(nameof(UpdateEngagementData), 1f, engagementUpdateInterval);
    }

    void SetupUI()
    {
        // Environment dropdown
        if (environmentDropdown != null)
        {
            environmentDropdown.ClearOptions();
            environmentDropdown.AddOptions(new List<string>(availableEnvironments));
        }

        // Grade dropdown
        if (gradeDropdown != null)
        {
            gradeDropdown.ClearOptions();
            gradeDropdown.AddOptions(new List<string>(gradeOptions));
        }

        // Button listeners
        if (muteAllButton != null)
            muteAllButton.onClick.AddListener(MuteAllStudents);
        
        if (unmuteAllButton != null)
            unmuteAllButton.onClick.AddListener(UnmuteAllStudents);
        
        if (kickSelectedButton != null)
            kickSelectedButton.onClick.AddListener(KickSelectedStudents);
        
        if (changeEnvironmentButton != null)
            changeEnvironmentButton.onClick.AddListener(ChangeEnvironment);
        
        if (materialAccessToggle != null)
            materialAccessToggle.onValueChanged.AddListener(OnMaterialAccessToggled);
        
        if (markAttendanceButton != null)
            markAttendanceButton.onClick.AddListener(MarkAllPresent);
        
        if (exportAttendanceButton != null)
            exportAttendanceButton.onClick.AddListener(ExportAttendance);
        
        if (assignGradeButton != null)
            assignGradeButton.onClick.AddListener(AssignGradeToSelected);

        // Initially hide admin panel
        if (adminPanel != null)
            adminPanel.SetActive(false);
    }

    /// <summary>
    /// Toggle the admin panel visibility.
    /// </summary>
    public void ToggleAdminPanel()
    {
        if (adminPanel != null)
        {
            adminPanel.SetActive(!adminPanel.activeSelf);
            
            if (adminPanel.activeSelf)
            {
                RefreshStudentList();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    /// <summary>
    /// Show the admin panel.
    /// </summary>
    public void ShowAdminPanel()
    {
        if (adminPanel != null)
        {
            adminPanel.SetActive(true);
            RefreshStudentList();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Hide the admin panel.
    /// </summary>
    public void HideAdminPanel()
    {
        if (adminPanel != null)
        {
            adminPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Refresh the student list from current room players.
    /// </summary>
    public void RefreshStudentList()
    {
        if (!PhotonNetwork.InRoom) return;

        // Clear existing UI
        if (studentListContainer != null)
        {
            foreach (Transform child in studentListContainer)
            {
                Destroy(child.gameObject);
            }
        }

        // Update student dictionary
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            // Skip the instructor (local player)
            if (player.IsLocal) continue;

            if (!students.ContainsKey(player.ActorNumber))
            {
                students[player.ActorNumber] = new StudentInfo
                {
                    actorNumber = player.ActorNumber,
                    displayName = player.NickName,
                    isMuted = false,
                    isBlocked = false,
                    engagementScore = 100f,
                    isTabFocused = true,
                    isPresent = true,
                    grade = "",
                    joinTime = System.DateTime.Now,
                    totalTimeInClass = 0f
                };
            }

            // Create UI item
            CreateStudentListItem(students[player.ActorNumber]);
        }

        UpdateClassStatus();
    }

    void CreateStudentListItem(StudentInfo student)
    {
        if (studentListContainer == null || studentListItemPrefab == null) return;

        GameObject item = Instantiate(studentListItemPrefab, studentListContainer);
        
        // Setup UI components
        Text nameText = item.GetComponentInChildren<Text>();
        if (nameText != null)
        {
            string status = student.isMuted ? " [MUTED]" : "";
            status += !student.isTabFocused ? " [AFK]" : "";
            nameText.text = $"{student.displayName}{status}";
        }

        // Mute button
        Button[] buttons = item.GetComponentsInChildren<Button>();
        foreach (Button btn in buttons)
        {
            if (btn.name.Contains("Mute"))
            {
                int actorNum = student.actorNumber;
                btn.onClick.AddListener(() => ToggleMuteStudent(actorNum));
            }
            else if (btn.name.Contains("Kick"))
            {
                int actorNum = student.actorNumber;
                btn.onClick.AddListener(() => KickStudent(actorNum));
            }
        }

        // Selection toggle
        Toggle toggle = item.GetComponentInChildren<Toggle>();
        if (toggle != null)
        {
            int actorNum = student.actorNumber;
            toggle.onValueChanged.AddListener((selected) => OnStudentSelectionChanged(actorNum, selected));
        }
    }

    void OnStudentSelectionChanged(int actorNumber, bool selected)
    {
        if (selected && !selectedStudents.Contains(actorNumber))
        {
            selectedStudents.Add(actorNumber);
        }
        else if (!selected && selectedStudents.Contains(actorNumber))
        {
            selectedStudents.Remove(actorNumber);
        }
    }

    /// <summary>
    /// Mute a specific student's microphone.
    /// </summary>
    public void ToggleMuteStudent(int actorNumber)
    {
        if (students.ContainsKey(actorNumber))
        {
            students[actorNumber].isMuted = !students[actorNumber].isMuted;
            
            // Send RPC to mute the student
            photonView.RPC("RPC_SetStudentMuted", RpcTarget.All, actorNumber, students[actorNumber].isMuted);
            
            onStudentMuted?.Invoke(actorNumber);
            RefreshStudentList();

            Debug.Log($"Student {students[actorNumber].displayName} {(students[actorNumber].isMuted ? "muted" : "unmuted")}");
        }
    }

    /// <summary>
    /// Mute all students in the classroom.
    /// </summary>
    public void MuteAllStudents()
    {
        foreach (var kvp in students)
        {
            kvp.Value.isMuted = true;
        }

        photonView.RPC("RPC_MuteAllStudents", RpcTarget.All, true);
        RefreshStudentList();
        Debug.Log("All students muted");
    }

    /// <summary>
    /// Unmute all students in the classroom.
    /// </summary>
    public void UnmuteAllStudents()
    {
        foreach (var kvp in students)
        {
            kvp.Value.isMuted = false;
        }

        photonView.RPC("RPC_MuteAllStudents", RpcTarget.All, false);
        RefreshStudentList();
        Debug.Log("All students unmuted");
    }

    /// <summary>
    /// Kick a specific student from the classroom.
    /// </summary>
    public void KickStudent(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("Only the master client (instructor) can kick students.");
            return;
        }

        Player targetPlayer = null;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == actorNumber)
            {
                targetPlayer = player;
                break;
            }
        }

        if (targetPlayer != null)
        {
            string playerName = targetPlayer.NickName;
            
            // Disconnect the player
            PhotonNetwork.CloseConnection(targetPlayer);
            
            // Remove from local tracking
            if (students.ContainsKey(actorNumber))
            {
                students.Remove(actorNumber);
            }

            onStudentKicked?.Invoke(actorNumber);
            RefreshStudentList();

            Debug.Log($"Kicked student: {playerName}");
        }
    }

    /// <summary>
    /// Kick all selected students.
    /// </summary>
    public void KickSelectedStudents()
    {
        // Copy to avoid modifying collection during iteration
        int[] studentsToKick = selectedStudents.ToArray();
        selectedStudents.Clear();
        
        foreach (int actorNumber in studentsToKick)
        {
            KickStudent(actorNumber);
        }
    }

    /// <summary>
    /// Change the classroom environment.
    /// </summary>
    public void ChangeEnvironment()
    {
        if (environmentDropdown == null) return;

        string newEnvironment = availableEnvironments[environmentDropdown.value];
        
        if (newEnvironment != currentEnvironment)
        {
            currentEnvironment = newEnvironment;
            
            // Notify all clients to change environment
            photonView.RPC("RPC_ChangeEnvironment", RpcTarget.All, newEnvironment);
            
            onEnvironmentChanged?.Invoke(newEnvironment);
            
            Debug.Log($"Environment changed to: {newEnvironment}");
        }
    }

    /// <summary>
    /// Toggle material access for students.
    /// </summary>
    public void OnMaterialAccessToggled(bool blocked)
    {
        materialsBlocked = blocked;
        
        // Notify all clients about material access
        photonView.RPC("RPC_SetMaterialAccess", RpcTarget.All, !blocked);
        
        Debug.Log($"Materials {(blocked ? "blocked" : "unblocked")} for students");
    }

    /// <summary>
    /// Update engagement data for all students.
    /// </summary>
    void UpdateEngagementData()
    {
        if (!PhotonNetwork.InRoom || !IsAdminPanelOpen) return;

        // Request engagement data from all students
        photonView.RPC("RPC_RequestEngagementData", RpcTarget.Others);
        
        UpdateEngagementSummary();
    }

    void UpdateEngagementSummary()
    {
        if (engagementSummaryText == null) return;

        int totalStudents = students.Count;
        int focusedStudents = 0;
        float avgEngagement = 0f;

        foreach (var student in students.Values)
        {
            if (student.isTabFocused) focusedStudents++;
            avgEngagement += student.engagementScore;
        }

        if (totalStudents > 0)
        {
            avgEngagement /= totalStudents;
        }

        engagementSummaryText.text = $"Students: {totalStudents}\n" +
                                     $"Focused: {focusedStudents}/{totalStudents}\n" +
                                     $"Avg Engagement: {avgEngagement:F0}%";
    }

    void UpdateClassStatus()
    {
        if (classStatusText == null) return;

        int totalStudents = students.Count;
        int mutedCount = 0;
        
        foreach (var student in students.Values)
        {
            if (student.isMuted) mutedCount++;
        }

        classStatusText.text = $"Environment: {currentEnvironment}\n" +
                               $"Students: {totalStudents}\n" +
                               $"Muted: {mutedCount}\n" +
                               $"Materials: {(materialsBlocked ? "Blocked" : "Available")}";
    }

    /// <summary>
    /// Mark all current students as present for attendance.
    /// </summary>
    public void MarkAllPresent()
    {
        foreach (var student in students.Values)
        {
            student.isPresent = true;
        }
        Debug.Log("Marked all students as present");
    }

    /// <summary>
    /// Export attendance data.
    /// </summary>
    public void ExportAttendance()
    {
        string attendanceData = "Student Name,Present,Join Time,Total Time\n";
        
        foreach (var student in students.Values)
        {
            attendanceData += $"{student.displayName},{student.isPresent},{student.joinTime:g},{student.totalTimeInClass:F0} min\n";
        }

        // In production, this would save to file or send to server
        Debug.Log($"Attendance Export:\n{attendanceData}");
        
        // Copy to clipboard (Note: may not work on all platforms like WebGL/mobile)
        // For production, consider platform-specific clipboard solutions or file export
        #if UNITY_EDITOR || UNITY_STANDALONE
        GUIUtility.systemCopyBuffer = attendanceData;
        #else
        Debug.LogWarning("Clipboard copy not supported on this platform. Data logged to console.");
        #endif
    }

    /// <summary>
    /// Assign grade to selected students.
    /// </summary>
    public void AssignGradeToSelected()
    {
        if (gradeDropdown == null || selectedStudents.Count == 0) return;

        string grade = gradeOptions[gradeDropdown.value];

        foreach (int actorNumber in selectedStudents)
        {
            if (students.ContainsKey(actorNumber))
            {
                students[actorNumber].grade = grade;
                Debug.Log($"Assigned grade {grade} to {students[actorNumber].displayName}");
            }
        }

        RefreshStudentList();
    }

    // Cached references for RPC performance
    private VoiceChatManager cachedVoiceManager;
    private ClassroomSceneManager cachedSceneManager;

    VoiceChatManager GetVoiceManager()
    {
        if (cachedVoiceManager == null)
        {
            cachedVoiceManager = VoiceChatManager.Instance;
            if (cachedVoiceManager == null)
            {
                cachedVoiceManager = FindObjectOfType<VoiceChatManager>();
            }
        }
        return cachedVoiceManager;
    }

    ClassroomSceneManager GetSceneManager()
    {
        if (cachedSceneManager == null)
        {
            cachedSceneManager = ClassroomSceneManager.Instance;
            if (cachedSceneManager == null)
            {
                cachedSceneManager = FindObjectOfType<ClassroomSceneManager>();
            }
        }
        return cachedSceneManager;
    }

    // Photon RPCs
    [PunRPC]
    void RPC_SetStudentMuted(int actorNumber, bool muted)
    {
        // Student receives mute command
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            var voiceManager = GetVoiceManager();
            if (voiceManager != null)
            {
                voiceManager.SetMuted(muted);
            }
            Debug.Log($"Your microphone has been {(muted ? "muted" : "unmuted")} by the instructor.");
        }
    }

    [PunRPC]
    void RPC_MuteAllStudents(bool muted)
    {
        // All students (non-instructors) receive mute command
        if (!PhotonNetwork.IsMasterClient)
        {
            var voiceManager = GetVoiceManager();
            if (voiceManager != null)
            {
                voiceManager.SetMuted(muted);
            }
        }
    }

    [PunRPC]
    void RPC_ChangeEnvironment(string environmentName)
    {
        // Handle environment change on all clients
        currentEnvironment = environmentName;
        
        // Trigger environment loading
        var sceneManager = GetSceneManager();
        if (sceneManager != null)
        {
            // sceneManager.LoadEnvironment(environmentName);
        }
        
        Debug.Log($"Environment changed to: {environmentName}");
    }

    [PunRPC]
    void RPC_SetMaterialAccess(bool hasAccess)
    {
        // Handle material access on student clients
        if (!PhotonNetwork.IsMasterClient)
        {
            // Disable/enable study materials UI
            Debug.Log($"Study materials access: {(hasAccess ? "Granted" : "Blocked")}");
        }
    }

    [PunRPC]
    void RPC_RequestEngagementData()
    {
        // Students respond with their engagement data
        if (!PhotonNetwork.IsMasterClient)
        {
            bool isFocused = Application.isFocused;
            photonView.RPC("RPC_ReceiveEngagementData", RpcTarget.MasterClient, 
                PhotonNetwork.LocalPlayer.ActorNumber, isFocused, 100f);
        }
    }

    [PunRPC]
    void RPC_ReceiveEngagementData(int actorNumber, bool isFocused, float engagementScore)
    {
        if (students.ContainsKey(actorNumber))
        {
            students[actorNumber].isTabFocused = isFocused;
            students[actorNumber].engagementScore = engagementScore;
            
            // Flag students who are not focused
            if (!isFocused)
            {
                Debug.Log($"Student {students[actorNumber].displayName} may not be focused!");
            }
        }
    }

    // Photon callbacks
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (newPlayer.IsLocal) return;

        students[newPlayer.ActorNumber] = new StudentInfo
        {
            actorNumber = newPlayer.ActorNumber,
            displayName = newPlayer.NickName,
            isMuted = false,
            isBlocked = false,
            engagementScore = 100f,
            isTabFocused = true,
            isPresent = true,
            grade = "",
            joinTime = System.DateTime.Now,
            totalTimeInClass = 0f
        };

        if (IsAdminPanelOpen)
        {
            RefreshStudentList();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (students.ContainsKey(otherPlayer.ActorNumber))
        {
            students.Remove(otherPlayer.ActorNumber);
        }

        if (IsAdminPanelOpen)
        {
            RefreshStudentList();
        }
    }
}
