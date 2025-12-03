using UnityEngine;
using Photon.Pun;

/// <summary>
/// Monitors student engagement by tracking:
/// - Tab focus (detecting if student switches to other applications)
/// - Time spent in classroom
/// - Interaction frequency
/// - Head movement/activity
/// Reports data back to instructor admin panel.
/// </summary>
public class StudentEngagementMonitor : MonoBehaviourPun
{
    [Header("Monitoring Settings")]
    public float reportInterval = 5f;
    public float idleThreshold = 30f;
    public float interactionWeight = 10f;

    [Header("Debug")]
    public bool showDebugInfo = false;

    // Engagement metrics
    private float engagementScore = 100f;
    private bool isTabFocused = true;
    private float totalTimeInSession;
    private float lastInteractionTime;
    private float lastMovementTime;
    private Vector3 lastHeadPosition;
    private Quaternion lastHeadRotation;
    private int interactionCount;
    private float idleTime;

    // Warning state
    private bool hasSentFocusWarning;
    private float focusLostTime;

    public float EngagementScore => engagementScore;
    public bool IsTabFocused => isTabFocused;
    public float TotalTimeInSession => totalTimeInSession;
    public float IdleTime => idleTime;

    void Start()
    {
        lastInteractionTime = Time.time;
        lastMovementTime = Time.time;

        // Only monitor if this is the local player
        if (photonView.IsMine)
        {
            InvokeRepeating(nameof(ReportEngagement), reportInterval, reportInterval);
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        totalTimeInSession += Time.deltaTime;

        TrackTabFocus();
        TrackMovement();
        TrackIdleState();
        CalculateEngagementScore();
    }

    void TrackTabFocus()
    {
        bool wasFocused = isTabFocused;
        isTabFocused = Application.isFocused;

        if (wasFocused && !isTabFocused)
        {
            // Tab lost focus
            focusLostTime = Time.time;
            Debug.Log("Student lost application focus");
        }
        else if (!wasFocused && isTabFocused)
        {
            // Tab regained focus
            float timeAway = Time.time - focusLostTime;
            Debug.Log($"Student regained focus after {timeAway:F1} seconds");
            hasSentFocusWarning = false;
        }

        // Send warning if unfocused for too long
        if (!isTabFocused && !hasSentFocusWarning && Time.time - focusLostTime > 10f)
        {
            hasSentFocusWarning = true;
            // Instructor will be notified through engagement data
        }
    }

    void TrackMovement()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 currentPos = cam.transform.position;
        Quaternion currentRot = cam.transform.rotation;

        // Check for significant movement
        float posDelta = Vector3.Distance(currentPos, lastHeadPosition);
        float rotDelta = Quaternion.Angle(currentRot, lastHeadRotation);

        if (posDelta > 0.01f || rotDelta > 1f)
        {
            lastMovementTime = Time.time;
        }

        lastHeadPosition = currentPos;
        lastHeadRotation = currentRot;
    }

    void TrackIdleState()
    {
        float timeSinceMovement = Time.time - lastMovementTime;
        float timeSinceInteraction = Time.time - lastInteractionTime;

        idleTime = Mathf.Max(timeSinceMovement, timeSinceInteraction);

        // Reduce engagement score if idle
        if (idleTime > idleThreshold)
        {
            engagementScore = Mathf.Max(0f, engagementScore - Time.deltaTime * 2f);
        }
    }

    void CalculateEngagementScore()
    {
        float baseScore = 100f;

        // Penalty for being unfocused
        if (!isTabFocused)
        {
            float unfocusedTime = Time.time - focusLostTime;
            baseScore -= Mathf.Min(50f, unfocusedTime * 2f);
        }

        // Penalty for being idle
        if (idleTime > idleThreshold)
        {
            baseScore -= Mathf.Min(30f, (idleTime - idleThreshold) * 0.5f);
        }

        // Bonus for interactions
        baseScore += Mathf.Min(20f, interactionCount * interactionWeight);

        engagementScore = Mathf.Clamp(baseScore, 0f, 100f);
    }

    /// <summary>
    /// Record an interaction (clicking, speaking, etc.)
    /// </summary>
    public void RecordInteraction()
    {
        lastInteractionTime = Time.time;
        interactionCount++;
        
        // Boost engagement score slightly
        engagementScore = Mathf.Min(100f, engagementScore + 5f);
    }

    // Cached reference for performance
    private InstructorAdminPanel cachedAdminPanel;

    /// <summary>
    /// Report engagement data to instructor.
    /// </summary>
    void ReportEngagement()
    {
        if (!PhotonNetwork.InRoom) return;

        // Use cached reference or singleton
        if (cachedAdminPanel == null)
        {
            cachedAdminPanel = InstructorAdminPanel.Instance;
            if (cachedAdminPanel == null)
            {
                cachedAdminPanel = FindObjectOfType<InstructorAdminPanel>();
            }
        }
        
        if (cachedAdminPanel != null && cachedAdminPanel.photonView != null)
        {
            cachedAdminPanel.photonView.RPC("RPC_ReceiveEngagementData", RpcTarget.MasterClient,
                PhotonNetwork.LocalPlayer.ActorNumber,
                isTabFocused,
                engagementScore);
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo || !photonView.IsMine) return;

        GUILayout.BeginArea(new Rect(Screen.width - 260, 10, 250, 120));
        GUILayout.Label($"Engagement Score: {engagementScore:F0}%");
        GUILayout.Label($"Tab Focused: {isTabFocused}");
        GUILayout.Label($"Idle Time: {idleTime:F1}s");
        GUILayout.Label($"Session Time: {totalTimeInSession / 60f:F1} min");
        GUILayout.Label($"Interactions: {interactionCount}");
        GUILayout.EndArea();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        isTabFocused = hasFocus;
        
        if (!hasFocus)
        {
            focusLostTime = Time.time;
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            isTabFocused = false;
            focusLostTime = Time.time;
        }
    }

    /// <summary>
    /// Get a summary of engagement data.
    /// </summary>
    public string GetEngagementSummary()
    {
        return $"Score: {engagementScore:F0}%, " +
               $"Focused: {isTabFocused}, " +
               $"Idle: {idleTime:F0}s, " +
               $"Time: {totalTimeInSession / 60f:F1}min";
    }
}
