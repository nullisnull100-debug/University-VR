using UnityEngine;

/// <summary>
/// Interactive desk that the player can sit at or examine.
/// Shows information about the desk position when interacted with.
/// </summary>
public class InteractableDesk : InteractableBase
{
    [Header("Desk Settings")]
    public int rowIndex;
    public int colIndex;
    public Transform seatPosition;
    public bool isOccupied;

    [Header("Sit Behavior")]
    public bool allowSitting = true;
    public KeyCode standUpKey = KeyCode.Space;

    private PlayerController sittingPlayer;
    private Vector3 playerOriginalPosition;
    private bool playerWasSitting;

    protected override void Awake()
    {
        base.Awake();
        interactionPrompt = "Press E to sit";
    }

    protected override void Update()
    {
        base.Update();

        // Check if player wants to stand up
        if (playerWasSitting && Input.GetKeyDown(standUpKey))
        {
            StandUp();
        }
    }

    protected override void OnInteract()
    {
        base.OnInteract();

        if (!allowSitting || isOccupied) return;

        // Find player
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            SitDown(player);
        }
    }

    void SitDown(PlayerController player)
    {
        sittingPlayer = player;
        playerOriginalPosition = player.transform.position;
        isOccupied = true;
        playerWasSitting = true;

        // Move player to seat
        Vector3 seatPos = seatPosition != null ? seatPosition.position : transform.position + Vector3.up * 0.5f;
        Quaternion seatRot = seatPosition != null ? seatPosition.rotation : transform.rotation;

        player.TeleportTo(seatPos, seatRot);
        
        // Update prompt
        interactionPrompt = "Press Space to stand up";

        Debug.Log($"Player sat at desk ({rowIndex}, {colIndex})");
    }

    void StandUp()
    {
        if (sittingPlayer == null) return;

        // Move player back or to standing position near desk
        Vector3 standPos = transform.position + transform.forward * 0.8f;
        sittingPlayer.TeleportTo(standPos, Quaternion.identity);

        sittingPlayer = null;
        isOccupied = false;
        playerWasSitting = false;
        interactionPrompt = "Press E to sit";

        Debug.Log($"Player stood up from desk ({rowIndex}, {colIndex})");
    }

    /// <summary>
    /// Get the desk's position identifier.
    /// </summary>
    public string GetDeskId()
    {
        return $"Desk_{rowIndex}_{colIndex}";
    }
}
