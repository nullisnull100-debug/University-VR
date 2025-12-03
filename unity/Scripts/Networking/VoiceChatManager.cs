using UnityEngine;
using Photon.Pun;
using Photon.Voice.Unity;
using Photon.Voice.PUN;

/// <summary>
/// Voice chat manager using Photon Voice for real-time voice communication.
/// Handles microphone input, spatial audio, and voice chat controls.
/// </summary>
[RequireComponent(typeof(PhotonVoiceNetwork))]
public class VoiceChatManager : MonoBehaviourPunCallbacks
{
    [Header("Voice Settings")]
    public bool voiceEnabled = true;
    public bool pushToTalk = false;
    public KeyCode pushToTalkKey = KeyCode.T;
    public bool spatialAudio = true;
    public float maxVoiceDistance = 20f;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float microphoneVolume = 1f;
    [Range(0f, 1f)]
    public float speakerVolume = 1f;

    [Header("UI")]
    public GameObject voiceIndicator;
    public GameObject mutedIndicator;

    [Header("Debug")]
    public bool showDebugInfo = true;

    private PhotonVoiceNetwork voiceNetwork;
    private Recorder voiceRecorder;
    private bool isMuted;
    private bool isTalking;

    public static VoiceChatManager Instance { get; private set; }
    public bool IsMuted => isMuted;
    public bool IsTalking => isTalking;
    public bool IsVoiceConnected => voiceNetwork != null && voiceNetwork.ClientState == Photon.Realtime.ClientState.Joined;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        voiceNetwork = GetComponent<PhotonVoiceNetwork>();
    }

    void Start()
    {
        SetupVoice();
        UpdateUI();
    }

    void Update()
    {
        if (!voiceEnabled) return;

        HandleInput();
        UpdateTalkingState();
        UpdateUI();
    }

    void SetupVoice()
    {
        // Find or create recorder
        voiceRecorder = GetComponentInChildren<Recorder>();
        if (voiceRecorder == null)
        {
            GameObject recorderObj = new GameObject("VoiceRecorder");
            recorderObj.transform.SetParent(transform);
            voiceRecorder = recorderObj.AddComponent<Recorder>();
        }

        // Configure recorder settings
        voiceRecorder.TransmitEnabled = voiceEnabled && !isMuted;
        voiceRecorder.VoiceDetection = !pushToTalk;
        voiceRecorder.VoiceDetectionThreshold = 0.01f;

        Debug.Log("Voice chat setup complete");
    }

    void HandleInput()
    {
        // Mute toggle (M key)
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMute();
        }

        // Push to talk
        if (pushToTalk)
        {
            if (Input.GetKeyDown(pushToTalkKey))
            {
                StartTransmitting();
            }
            else if (Input.GetKeyUp(pushToTalkKey))
            {
                StopTransmitting();
            }
        }

        // Volume controls
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            SetSpeakerVolume(Mathf.Min(1f, speakerVolume + 0.1f));
        }
        else if (Input.GetKeyDown(KeyCode.PageDown))
        {
            SetSpeakerVolume(Mathf.Max(0f, speakerVolume - 0.1f));
        }
    }

    void UpdateTalkingState()
    {
        if (voiceRecorder != null)
        {
            isTalking = voiceRecorder.IsCurrentlyTransmitting;
        }
    }

    void UpdateUI()
    {
        if (voiceIndicator != null)
        {
            voiceIndicator.SetActive(isTalking && !isMuted);
        }

        if (mutedIndicator != null)
        {
            mutedIndicator.SetActive(isMuted);
        }
    }

    /// <summary>
    /// Toggle microphone mute state.
    /// </summary>
    public void ToggleMute()
    {
        SetMuted(!isMuted);
    }

    /// <summary>
    /// Set microphone mute state.
    /// </summary>
    public void SetMuted(bool muted)
    {
        isMuted = muted;
        
        if (voiceRecorder != null)
        {
            voiceRecorder.TransmitEnabled = !muted;
        }

        Debug.Log($"Voice muted: {muted}");
    }

    /// <summary>
    /// Start transmitting voice (for push-to-talk).
    /// </summary>
    public void StartTransmitting()
    {
        if (voiceRecorder != null && !isMuted)
        {
            voiceRecorder.TransmitEnabled = true;
        }
    }

    /// <summary>
    /// Stop transmitting voice (for push-to-talk).
    /// </summary>
    public void StopTransmitting()
    {
        if (voiceRecorder != null && pushToTalk)
        {
            voiceRecorder.TransmitEnabled = false;
        }
    }

    /// <summary>
    /// Enable or disable voice chat.
    /// </summary>
    public void SetVoiceEnabled(bool enabled)
    {
        voiceEnabled = enabled;
        
        if (voiceRecorder != null)
        {
            voiceRecorder.TransmitEnabled = enabled && !isMuted;
        }

        Debug.Log($"Voice enabled: {enabled}");
    }

    /// <summary>
    /// Set microphone input volume.
    /// </summary>
    public void SetMicrophoneVolume(float volume)
    {
        microphoneVolume = Mathf.Clamp01(volume);
        // Note: Actual microphone gain control may need platform-specific implementation
    }

    /// <summary>
    /// Set speaker output volume for voice chat.
    /// Note: This affects only voice audio sources, not global audio.
    /// </summary>
    public void SetSpeakerVolume(float volume)
    {
        speakerVolume = Mathf.Clamp01(volume);
        
        // Update volume on all voice speakers instead of global AudioListener
        // This keeps game audio separate from voice volume
        var voicePlayers = FindObjectsOfType<NetworkVoicePlayer>();
        foreach (var player in voicePlayers)
        {
            player.SetVolume(speakerVolume);
        }
        
        Debug.Log($"Voice volume: {(int)(volume * 100)}%");
    }

    /// <summary>
    /// Enable or disable push-to-talk mode.
    /// </summary>
    public void SetPushToTalk(bool enabled)
    {
        pushToTalk = enabled;
        
        if (voiceRecorder != null)
        {
            voiceRecorder.VoiceDetection = !enabled;
            if (!enabled)
            {
                voiceRecorder.TransmitEnabled = !isMuted;
            }
        }

        Debug.Log($"Push-to-talk: {enabled}");
    }

    /// <summary>
    /// Enable or disable spatial audio.
    /// </summary>
    public void SetSpatialAudio(bool enabled)
    {
        spatialAudio = enabled;
        // Spatial audio is configured per-speaker in NetworkVoicePlayer
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 160, 300, 100));
        GUILayout.Label($"Voice: {(voiceEnabled ? "Enabled" : "Disabled")}");
        GUILayout.Label($"Muted: {(isMuted ? "Yes" : "No")} (M to toggle)");
        GUILayout.Label($"Talking: {(isTalking ? "Yes" : "No")}");
        GUILayout.Label($"Mode: {(pushToTalk ? "Push-to-Talk (T)" : "Voice Activated")}");
        GUILayout.EndArea();
    }

    /// <summary>
    /// Get available microphone devices.
    /// </summary>
    public string[] GetMicrophoneDevices()
    {
        return Microphone.devices;
    }

    /// <summary>
    /// Set the microphone device to use.
    /// </summary>
    public void SetMicrophoneDevice(string deviceName)
    {
        if (voiceRecorder != null)
        {
            voiceRecorder.UnityMicrophoneDevice = deviceName;
            Debug.Log($"Microphone device set to: {deviceName}");
        }
    }
}
