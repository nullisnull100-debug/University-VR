using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Networked whiteboard that synchronizes drawing across all connected clients.
/// Extends the basic WhiteboardInteractable with Photon networking.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(PhotonView))]
public class NetworkedWhiteboard : MonoBehaviourPun
{
    [Header("Drawing")]
    public Camera raycastCamera;
    public Material lineMaterial;
    public float lineWidth = 0.025f;
    public KeyCode clearKey = KeyCode.C;

    [Header("Colors")]
    public Color[] availableColors = new Color[]
    {
        Color.black,
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.white
    };
    public int currentColorIndex = 0;
    public KeyCode nextColorKey = KeyCode.RightBracket;
    public KeyCode prevColorKey = KeyCode.LeftBracket;

    [Header("Network")]
    public bool onlyInstructorCanDraw = false;

    private List<LineRenderer> lines = new List<LineRenderer>();
    private LineRenderer currentLine;
    private int currentLineId = 0;
    private bool materialWarningShown;
    
    // Material pool to reuse materials by color index
    private Dictionary<int, Material> materialPool = new Dictionary<int, Material>();

    void Start()
    {
        if (raycastCamera == null) raycastCamera = Camera.main;
        
        if (lineMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                lineMaterial = new Material(shader);
                lineMaterial.color = availableColors[currentColorIndex];
            }
            
            if (!materialWarningShown)
            {
                Debug.LogWarning("NetworkedWhiteboard: No line material assigned. Using runtime-created material.");
                materialWarningShown = true;
            }
        }
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Clear board
        if (Input.GetKeyDown(clearKey))
        {
            RequestClearBoard();
        }

        // Color switching
        if (Input.GetKeyDown(nextColorKey))
        {
            currentColorIndex = (currentColorIndex + 1) % availableColors.Length;
            UpdateMaterialColor();
        }
        else if (Input.GetKeyDown(prevColorKey))
        {
            currentColorIndex = (currentColorIndex - 1 + availableColors.Length) % availableColors.Length;
            UpdateMaterialColor();
        }

        // Drawing
        if (Input.GetMouseButtonDown(0))
        {
            TryBeginStroke();
        }
        if (Input.GetMouseButton(0) && currentLine != null)
        {
            ContinueStroke();
        }
        if (Input.GetMouseButtonUp(0) && currentLine != null)
        {
            EndStroke();
        }
    }

    void UpdateMaterialColor()
    {
        if (lineMaterial != null)
        {
            lineMaterial.color = availableColors[currentColorIndex];
        }
    }

    void TryBeginStroke()
    {
        if (raycastCamera == null) return;

        Ray r = raycastCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(r, out RaycastHit hit))
        {
            if (hit.collider == GetComponent<Collider>())
            {
                Vector3 hitPos = hit.point + (hit.normal * 0.001f);
                int lineId = currentLineId++;
                Color color = availableColors[currentColorIndex];

                // Create local line
                currentLine = CreateLine(lineId, color);
                AddPointToLine(currentLine, hitPos);

                // Sync to network
                photonView.RPC("RPC_BeginStroke", RpcTarget.Others, lineId, hitPos, currentColorIndex);
            }
        }
    }

    void ContinueStroke()
    {
        if (raycastCamera == null) return;

        Ray r = raycastCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(r, out RaycastHit hit))
        {
            if (hit.collider == GetComponent<Collider>())
            {
                Vector3 hitPos = hit.point + (hit.normal * 0.001f);
                
                // Only add point if far enough from last point
                if (currentLine.positionCount == 0 || 
                    Vector3.Distance(currentLine.GetPosition(currentLine.positionCount - 1), hitPos) > 0.01f)
                {
                    AddPointToLine(currentLine, hitPos);
                    
                    // Sync to network
                    photonView.RPC("RPC_ContinueStroke", RpcTarget.Others, hitPos);
                }
            }
        }
    }

    void EndStroke()
    {
        currentLine = null;
        photonView.RPC("RPC_EndStroke", RpcTarget.Others);
    }

    LineRenderer CreateLine(int lineId, Color color)
    {
        GameObject go = new GameObject($"Line_{lineId}");
        go.transform.parent = transform;
        
        LineRenderer line = go.AddComponent<LineRenderer>();
        
        // Get or create material from pool for this color
        int colorIndex = GetColorIndex(color);
        Material mat = GetPooledMaterial(colorIndex, color);
        line.material = mat;
        
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.useWorldSpace = true;
        line.textureMode = LineTextureMode.Stretch;
        line.numCapVertices = 4;
        line.positionCount = 0;
        
        lines.Add(line);
        return line;
    }

    Material GetPooledMaterial(int colorIndex, Color color)
    {
        if (!materialPool.ContainsKey(colorIndex))
        {
            Material mat = new Material(lineMaterial);
            mat.color = color;
            materialPool[colorIndex] = mat;
        }
        return materialPool[colorIndex];
    }

    int GetColorIndex(Color color)
    {
        for (int i = 0; i < availableColors.Length; i++)
        {
            if (availableColors[i] == color) return i;
        }
        return 0;
    }

    void AddPointToLine(LineRenderer line, Vector3 point)
    {
        if (line == null) return;
        line.positionCount++;
        line.SetPosition(line.positionCount - 1, point);
    }

    void RequestClearBoard()
    {
        // Clear locally and sync to all
        photonView.RPC("RPC_ClearBoard", RpcTarget.All);
    }

    // RPCs for network synchronization
    [PunRPC]
    void RPC_BeginStroke(int lineId, Vector3 position, int colorIndex)
    {
        Color color = colorIndex < availableColors.Length ? availableColors[colorIndex] : Color.black;
        currentLine = CreateLine(lineId, color);
        AddPointToLine(currentLine, position);
    }

    [PunRPC]
    void RPC_ContinueStroke(Vector3 position)
    {
        if (currentLine != null)
        {
            AddPointToLine(currentLine, position);
        }
    }

    [PunRPC]
    void RPC_EndStroke()
    {
        currentLine = null;
    }

    [PunRPC]
    void RPC_ClearBoard()
    {
        foreach (var l in lines)
        {
            if (l != null) Destroy(l.gameObject);
        }
        lines.Clear();
        currentLine = null;
        Debug.Log("Whiteboard cleared");
    }

    /// <summary>
    /// Clear the whiteboard (local call, will sync to network).
    /// </summary>
    public void ClearBoard()
    {
        RequestClearBoard();
    }

    /// <summary>
    /// Set the current drawing color.
    /// </summary>
    public void SetColor(int colorIndex)
    {
        currentColorIndex = Mathf.Clamp(colorIndex, 0, availableColors.Length - 1);
        UpdateMaterialColor();
    }

    /// <summary>
    /// Get the current drawing color.
    /// </summary>
    public Color GetCurrentColor()
    {
        return availableColors[currentColorIndex];
    }
}
