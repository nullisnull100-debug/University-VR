using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Lobby UI for connecting to the VR Classroom network.
/// Handles server connection, room creation/joining, and player list display.
/// </summary>
public class NetworkLobbyUI : MonoBehaviourPunCallbacks
{
    [Header("Connection Panel")]
    public GameObject connectionPanel;
    public InputField playerNameInput;
    public Button connectButton;
    public Text connectionStatusText;

    [Header("Lobby Panel")]
    public GameObject lobbyPanel;
    public InputField roomNameInput;
    public Button joinRoomButton;
    public Button createRoomButton;
    public Button quickJoinButton;
    public Transform roomListContainer;
    public GameObject roomListItemPrefab;

    [Header("Room Panel")]
    public GameObject roomPanel;
    public Text roomNameText;
    public Text playerCountText;
    public Transform playerListContainer;
    public GameObject playerListItemPrefab;
    public Button leaveRoomButton;
    public Button startButton;

    [Header("Voice Settings")]
    public Toggle muteToggle;
    public Toggle pushToTalkToggle;
    public Slider volumeSlider;

    [Header("References")]
    public NetworkManager networkManager;
    public VoiceChatManager voiceChatManager;

    private void Start()
    {
        SetupUI();
        ShowConnectionPanel();
    }

    void SetupUI()
    {
        // Connection panel
        if (connectButton != null)
        {
            connectButton.onClick.AddListener(OnConnectClick);
        }

        // Lobby panel
        if (joinRoomButton != null)
        {
            joinRoomButton.onClick.AddListener(OnJoinRoomClick);
        }
        if (createRoomButton != null)
        {
            createRoomButton.onClick.AddListener(OnCreateRoomClick);
        }
        if (quickJoinButton != null)
        {
            quickJoinButton.onClick.AddListener(OnQuickJoinClick);
        }

        // Room panel
        if (leaveRoomButton != null)
        {
            leaveRoomButton.onClick.AddListener(OnLeaveRoomClick);
        }
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartClick);
        }

        // Voice settings
        if (muteToggle != null)
        {
            muteToggle.onValueChanged.AddListener(OnMuteToggle);
        }
        if (pushToTalkToggle != null)
        {
            pushToTalkToggle.onValueChanged.AddListener(OnPushToTalkToggle);
        }
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        // Get references if not assigned
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
        }
        if (voiceChatManager == null)
        {
            voiceChatManager = FindObjectOfType<VoiceChatManager>();
        }
    }

    void ShowConnectionPanel()
    {
        if (connectionPanel != null) connectionPanel.SetActive(true);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (roomPanel != null) roomPanel.SetActive(false);
    }

    void ShowLobbyPanel()
    {
        if (connectionPanel != null) connectionPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(true);
        if (roomPanel != null) roomPanel.SetActive(false);
    }

    void ShowRoomPanel()
    {
        if (connectionPanel != null) connectionPanel.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
        if (roomPanel != null) roomPanel.SetActive(true);
        UpdateRoomInfo();
    }

    void UpdateConnectionStatus(string status)
    {
        if (connectionStatusText != null)
        {
            connectionStatusText.text = status;
        }
    }

    void UpdateRoomInfo()
    {
        if (!PhotonNetwork.InRoom) return;

        if (roomNameText != null)
        {
            roomNameText.text = $"Room: {PhotonNetwork.CurrentRoom.Name}";
        }

        if (playerCountText != null)
        {
            playerCountText.text = $"Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        }

        // Update player list
        UpdatePlayerList();

        // Only master client can start
        if (startButton != null)
        {
            startButton.interactable = PhotonNetwork.IsMasterClient;
        }
    }

    void UpdatePlayerList()
    {
        if (playerListContainer == null || playerListItemPrefab == null) return;

        // Clear existing items
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        // Add player items
        foreach (var player in PhotonNetwork.PlayerList)
        {
            GameObject item = Instantiate(playerListItemPrefab, playerListContainer);
            Text itemText = item.GetComponentInChildren<Text>();
            if (itemText != null)
            {
                string prefix = player.IsMasterClient ? "[Host] " : "";
                string suffix = player.IsLocal ? " (You)" : "";
                itemText.text = $"{prefix}{player.NickName}{suffix}";
            }
        }
    }

    // Button callbacks
    void OnConnectClick()
    {
        string playerName = playerNameInput != null ? playerNameInput.text.Trim() : "";
        
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = $"Player_{Random.Range(1000, 9999)}";
        }

        UpdateConnectionStatus("Connecting...");
        
        if (networkManager != null)
        {
            networkManager.Connect(playerName);
        }
        else
        {
            PhotonNetwork.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    void OnJoinRoomClick()
    {
        string roomName = roomNameInput != null ? roomNameInput.text.Trim() : "";
        
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogWarning("Please enter a room name");
            return;
        }

        PhotonNetwork.JoinRoom(roomName);
    }

    void OnCreateRoomClick()
    {
        string roomName = roomNameInput != null ? roomNameInput.text.Trim() : "";
        
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = $"Classroom_{Random.Range(1000, 9999)}";
        }

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 25,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(roomName, options);
    }

    void OnQuickJoinClick()
    {
        // Join any available room or create a new one
        if (networkManager != null)
        {
            networkManager.JoinClassroom();
        }
        else
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    void OnLeaveRoomClick()
    {
        PhotonNetwork.LeaveRoom();
    }

    void OnStartClick()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Hide lobby UI and start the classroom
            if (roomPanel != null) roomPanel.SetActive(false);
            
            // Lock cursor for gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("Starting classroom session...");
        }
    }

    void OnMuteToggle(bool muted)
    {
        if (voiceChatManager != null)
        {
            voiceChatManager.SetMuted(muted);
        }
    }

    void OnPushToTalkToggle(bool enabled)
    {
        if (voiceChatManager != null)
        {
            voiceChatManager.SetPushToTalk(enabled);
        }
    }

    void OnVolumeChanged(float volume)
    {
        if (voiceChatManager != null)
        {
            voiceChatManager.SetSpeakerVolume(volume);
        }
    }

    // Photon Callbacks
    public override void OnConnectedToMaster()
    {
        UpdateConnectionStatus("Connected!");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        ShowLobbyPanel();
    }

    public override void OnJoinedRoom()
    {
        ShowRoomPanel();
    }

    public override void OnLeftRoom()
    {
        ShowLobbyPanel();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateRoomInfo();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateRoomInfo();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No rooms available, creating a new one...");
        OnCreateRoomClick();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join room: {message}");
        UpdateConnectionStatus($"Failed: {message}");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        UpdateConnectionStatus($"Disconnected: {cause}");
        ShowConnectionPanel();
    }

    public override void OnRoomListUpdate(System.Collections.Generic.List<RoomInfo> roomList)
    {
        UpdateRoomList(roomList);
    }

    void UpdateRoomList(System.Collections.Generic.List<RoomInfo> rooms)
    {
        if (roomListContainer == null || roomListItemPrefab == null) return;

        // Clear existing items
        foreach (Transform child in roomListContainer)
        {
            Destroy(child.gameObject);
        }

        // Add room items
        foreach (var room in rooms)
        {
            if (room.RemovedFromList || !room.IsOpen) continue;

            GameObject item = Instantiate(roomListItemPrefab, roomListContainer);
            
            Text itemText = item.GetComponentInChildren<Text>();
            if (itemText != null)
            {
                itemText.text = $"{room.Name} ({room.PlayerCount}/{room.MaxPlayers})";
            }

            Button itemButton = item.GetComponent<Button>();
            if (itemButton != null)
            {
                string roomName = room.Name;
                itemButton.onClick.AddListener(() => PhotonNetwork.JoinRoom(roomName));
            }
        }
    }
}
