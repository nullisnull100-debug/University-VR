using UnityEngine;
using Photon.Pun;

/// <summary>
/// Networked player that syncs position, rotation, and animations across clients.
/// Attach to the player prefab that will be instantiated via PhotonNetwork.Instantiate().
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class NetworkPlayer : MonoBehaviourPun, IPunObservable
{
    [Header("Components")]
    public Transform headTransform;
    public Transform leftHandTransform;
    public Transform rightHandTransform;
    public Renderer playerRenderer;

    [Header("Interpolation")]
    public float positionLerpSpeed = 10f;
    public float rotationLerpSpeed = 10f;

    [Header("Local Player")]
    public GameObject localPlayerUI;
    public MonoBehaviour[] componentsToDisableForRemote;

    [Header("Player Info")]
    public string playerName;
    public bool isInstructor;

    // Network sync variables
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private Vector3 networkHeadPosition;
    private Quaternion networkHeadRotation;

    // Reference to local player controller
    private PlayerController localController;

    void Awake()
    {
        networkPosition = transform.position;
        networkRotation = transform.rotation;
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            SetupLocalPlayer();
        }
        else
        {
            SetupRemotePlayer();
        }

        // Set player name from Photon nickname
        playerName = photonView.Owner.NickName;
    }

    void SetupLocalPlayer()
    {
        Debug.Log("Setting up local network player");
        
        // Enable local player UI
        if (localPlayerUI != null)
        {
            localPlayerUI.SetActive(true);
        }

        // Try to find and configure local player controller
        localController = GetComponent<PlayerController>();
        if (localController == null)
        {
            localController = gameObject.AddComponent<PlayerController>();
        }

        // Set as main camera target
        if (Camera.main != null)
        {
            var camController = Camera.main.GetComponent<CameraController>();
            if (camController != null)
            {
                camController.SetTarget(transform);
            }
        }
    }

    void SetupRemotePlayer()
    {
        Debug.Log($"Setting up remote player: {photonView.Owner.NickName}");
        
        // Disable components that should only run on local player
        foreach (var component in componentsToDisableForRemote)
        {
            if (component != null)
            {
                component.enabled = false;
            }
        }

        // Disable local player UI for remote players
        if (localPlayerUI != null)
        {
            localPlayerUI.SetActive(false);
        }

        // Disable any local input components
        var controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        var charController = GetComponent<CharacterController>();
        if (charController != null)
        {
            charController.enabled = false;
        }

        // Set a different color for remote players
        SetPlayerColor(GetPlayerColor(photonView.Owner.ActorNumber));
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            // Interpolate position and rotation for remote players
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * positionLerpSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * rotationLerpSpeed);

            if (headTransform != null)
            {
                headTransform.localPosition = Vector3.Lerp(headTransform.localPosition, networkHeadPosition, Time.deltaTime * positionLerpSpeed);
                headTransform.localRotation = Quaternion.Lerp(headTransform.localRotation, networkHeadRotation, Time.deltaTime * rotationLerpSpeed);
            }
        }
    }

    // Called by Photon to sync data
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Local player sends data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            
            if (headTransform != null)
            {
                stream.SendNext(headTransform.localPosition);
                stream.SendNext(headTransform.localRotation);
            }
            else
            {
                stream.SendNext(Vector3.zero);
                stream.SendNext(Quaternion.identity);
            }
        }
        else
        {
            // Remote player receives data
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            networkHeadPosition = (Vector3)stream.ReceiveNext();
            networkHeadRotation = (Quaternion)stream.ReceiveNext();
        }
    }

    private Material cachedMaterial;

    void SetPlayerColor(Color color)
    {
        if (playerRenderer != null)
        {
            // Cache the material instance to avoid repeated allocation
            if (cachedMaterial == null)
            {
                cachedMaterial = playerRenderer.material;
            }
            
            if (cachedMaterial.HasProperty("_Color"))
            {
                cachedMaterial.color = color;
            }
        }
    }

    Color GetPlayerColor(int actorNumber)
    {
        // Generate distinct colors for different players
        Color[] colors = new Color[]
        {
            new Color(0.2f, 0.6f, 1.0f), // Blue
            new Color(1.0f, 0.4f, 0.4f), // Red
            new Color(0.4f, 1.0f, 0.4f), // Green
            new Color(1.0f, 1.0f, 0.4f), // Yellow
            new Color(1.0f, 0.6f, 0.2f), // Orange
            new Color(0.8f, 0.4f, 1.0f), // Purple
            new Color(0.4f, 1.0f, 1.0f), // Cyan
            new Color(1.0f, 0.6f, 0.8f)  // Pink
        };
        
        return colors[(actorNumber - 1) % colors.Length];
    }

    /// <summary>
    /// Get this player's display name.
    /// </summary>
    public string GetPlayerName()
    {
        return string.IsNullOrEmpty(playerName) ? $"Player {photonView.Owner.ActorNumber}" : playerName;
    }

    /// <summary>
    /// Check if this is the local player.
    /// </summary>
    public bool IsLocalPlayer()
    {
        return photonView.IsMine;
    }

    /// <summary>
    /// RPC to sync player role across network.
    /// </summary>
    [PunRPC]
    public void SetInstructor(bool instructor)
    {
        isInstructor = instructor;
        // Visual indication for instructor
        if (instructor && playerRenderer != null)
        {
            SetPlayerColor(new Color(1f, 0.8f, 0.2f)); // Gold color for instructor
        }
    }

    /// <summary>
    /// Call this to set local player as instructor.
    /// </summary>
    public void BecomeInstructor()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SetInstructor", RpcTarget.AllBuffered, true);
        }
    }
}
