using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Main network manager handling connection to Photon servers and room management.
/// Handles remote player connections and synchronization across multiple clients.
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Connection Settings")]
    public string gameVersion = "1.0";
    public byte maxPlayersPerRoom = 25;
    public string defaultRoomName = "VRClassroom_Demo";

    [Header("Player Settings")]
    public GameObject networkPlayerPrefab;
    public Transform[] spawnPoints;

    [Header("Events")]
    public UnityEvent onConnectedToServer;
    public UnityEvent onJoinedRoom;
    public UnityEvent onLeftRoom;
    public UnityEvent onPlayerJoined;
    public UnityEvent onPlayerLeft;
    public UnityEvent<string> onConnectionError;

    [Header("Debug")]
    public bool autoConnect = true;
    public bool showDebugInfo = true;

    private bool isConnecting;
    private string playerName;

    public static NetworkManager Instance { get; private set; }
    public bool IsConnected => PhotonNetwork.IsConnected;
    public bool IsInRoom => PhotonNetwork.InRoom;
    public int PlayerCount => PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;

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

        // Ensure we can use PhotonNetwork.LoadLevel() and sync levels
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        if (autoConnect)
        {
            Connect();
        }
    }

    /// <summary>
    /// Connect to Photon master server.
    /// </summary>
    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Already connected to Photon.");
            return;
        }

        isConnecting = true;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Connecting to Photon master server...");
    }

    /// <summary>
    /// Connect with a specific player name.
    /// </summary>
    public void Connect(string name)
    {
        playerName = name;
        PhotonNetwork.NickName = name;
        Connect();
    }

    /// <summary>
    /// Disconnect from Photon.
    /// </summary>
    public void Disconnect()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    /// <summary>
    /// Join or create the default classroom room.
    /// </summary>
    public void JoinClassroom()
    {
        JoinRoom(defaultRoomName);
    }

    /// <summary>
    /// Join or create a specific room.
    /// </summary>
    public void JoinRoom(string roomName)
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("Not connected to Photon. Call Connect() first.");
            return;
        }

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayersPerRoom,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        Debug.Log($"Joining room: {roomName}");
    }

    /// <summary>
    /// Leave the current room.
    /// </summary>
    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    /// <summary>
    /// Set the local player's display name.
    /// </summary>
    public void SetPlayerName(string name)
    {
        playerName = name;
        PhotonNetwork.NickName = name;
    }

    // Photon Callbacks
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
        isConnecting = false;
        onConnectedToServer?.Invoke();

        // Auto-join lobby to see rooms
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Photon Lobby");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined room: {PhotonNetwork.CurrentRoom.Name} with {PhotonNetwork.CurrentRoom.PlayerCount} players");
        
        // Spawn network player
        SpawnNetworkPlayer();
        
        onJoinedRoom?.Invoke();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left room");
        onLeftRoom?.Invoke();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player joined: {newPlayer.NickName}");
        onPlayerJoined?.Invoke();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player left: {otherPlayer.NickName}");
        onPlayerLeft?.Invoke();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Disconnected from Photon: {cause}");
        isConnecting = false;
        onConnectionError?.Invoke(cause.ToString());
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join room: {message}");
        onConnectionError?.Invoke(message);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to create room: {message}");
        onConnectionError?.Invoke(message);
    }

    void SpawnNetworkPlayer()
    {
        if (networkPlayerPrefab == null)
        {
            Debug.LogWarning("No network player prefab assigned. Skipping spawn.");
            return;
        }

        // Get spawn position
        Vector3 spawnPos = GetSpawnPosition();
        Quaternion spawnRot = Quaternion.identity;

        // Instantiate networked player
        GameObject player = PhotonNetwork.Instantiate(
            networkPlayerPrefab.name, 
            spawnPos, 
            spawnRot
        );

        Debug.Log($"Spawned network player at {spawnPos}");
    }

    Vector3 GetSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Use spawn point based on player number
            int index = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;
            if (spawnPoints[index] != null)
            {
                return spawnPoints[index].position;
            }
        }

        // Default spawn with offset based on player number
        float offset = (PhotonNetwork.LocalPlayer.ActorNumber - 1) * 1.5f;
        return new Vector3(offset - 3f, 0f, 5f);
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label($"Network Status: {(IsConnected ? "Connected" : "Disconnected")}");
        GUILayout.Label($"In Room: {(IsInRoom ? PhotonNetwork.CurrentRoom.Name : "No")}");
        GUILayout.Label($"Players: {PlayerCount}");
        GUILayout.Label($"Ping: {PhotonNetwork.GetPing()} ms");
        GUILayout.EndArea();
    }

    /// <summary>
    /// Get list of players in current room.
    /// </summary>
    public Player[] GetPlayersInRoom()
    {
        if (!PhotonNetwork.InRoom) return new Player[0];
        return PhotonNetwork.PlayerList;
    }

    /// <summary>
    /// Check if local player is the master client (room host).
    /// </summary>
    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }
}
