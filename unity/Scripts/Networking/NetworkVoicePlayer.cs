using UnityEngine;
using Photon.Voice.Unity;

/// <summary>
/// Handles voice audio playback for remote network players.
/// Attach to the network player prefab for spatial voice audio.
/// </summary>
public class NetworkVoicePlayer : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource voiceAudioSource;
    public bool useSpatialAudio = true;
    public float minDistance = 1f;
    public float maxDistance = 20f;

    [Header("Visual Feedback")]
    public GameObject talkingIndicator;
    public Renderer avatarRenderer;
    public Color talkingColor = new Color(0.4f, 1f, 0.4f);
    
    private Speaker voiceSpeaker;
    private Color originalColor;
    private bool isTalking;
    private Material cachedMaterial;

    void Start()
    {
        SetupAudio();
        SetupSpeaker();
        
        // Cache the material instance to avoid repeated allocation
        if (avatarRenderer != null)
        {
            cachedMaterial = avatarRenderer.material;
            if (cachedMaterial.HasProperty("_Color"))
            {
                originalColor = cachedMaterial.color;
            }
        }
    }

    void Update()
    {
        UpdateTalkingState();
        UpdateVisualFeedback();
    }

    void SetupAudio()
    {
        if (voiceAudioSource == null)
        {
            voiceAudioSource = GetComponent<AudioSource>();
            if (voiceAudioSource == null)
            {
                voiceAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Configure spatial audio
        voiceAudioSource.spatialBlend = useSpatialAudio ? 1f : 0f;
        voiceAudioSource.rolloffMode = AudioRolloffMode.Linear;
        voiceAudioSource.minDistance = minDistance;
        voiceAudioSource.maxDistance = maxDistance;
        voiceAudioSource.dopplerLevel = 0f;
        voiceAudioSource.playOnAwake = false;
    }

    void SetupSpeaker()
    {
        voiceSpeaker = GetComponent<Speaker>();
        if (voiceSpeaker == null)
        {
            voiceSpeaker = gameObject.AddComponent<Speaker>();
        }
    }

    void UpdateTalkingState()
    {
        if (voiceSpeaker != null)
        {
            isTalking = voiceSpeaker.IsPlaying;
        }
        else if (voiceAudioSource != null)
        {
            isTalking = voiceAudioSource.isPlaying;
        }
    }

    void UpdateVisualFeedback()
    {
        // Show/hide talking indicator
        if (talkingIndicator != null)
        {
            talkingIndicator.SetActive(isTalking);
        }

        // Highlight avatar when talking using cached material
        if (cachedMaterial != null && cachedMaterial.HasProperty("_Color"))
        {
            cachedMaterial.color = isTalking ? talkingColor : originalColor;
        }
    }

    /// <summary>
    /// Check if this player is currently talking.
    /// </summary>
    public bool IsTalking()
    {
        return isTalking;
    }

    /// <summary>
    /// Enable or disable spatial audio for this player's voice.
    /// </summary>
    public void SetSpatialAudio(bool enabled)
    {
        useSpatialAudio = enabled;
        if (voiceAudioSource != null)
        {
            voiceAudioSource.spatialBlend = enabled ? 1f : 0f;
        }
    }

    /// <summary>
    /// Set the volume for this player's voice.
    /// </summary>
    public void SetVolume(float volume)
    {
        if (voiceAudioSource != null)
        {
            voiceAudioSource.volume = Mathf.Clamp01(volume);
        }
    }

    /// <summary>
    /// Mute or unmute this player's voice.
    /// </summary>
    public void SetMuted(bool muted)
    {
        if (voiceAudioSource != null)
        {
            voiceAudioSource.mute = muted;
        }
    }
}
