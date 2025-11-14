using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple drawable whiteboard for demo presentations.
/// Left mouse button (or pointer) draws lines on the whiteboard surface.
/// Attach this to the whiteboard GameObject (a flat plane facing the classroom).
/// The script will create LineRenderers parented under this object.
/// </summary>
[RequireComponent(typeof(Collider))]
public class WhiteboardInteractable : MonoBehaviour
{
    [Header("Drawing")]
    public Camera raycastCamera; // if null, Camera.main will be used
    public Material lineMaterial;
    public float lineWidth = 0.025f;
    public KeyCode clearKey = KeyCode.C;

    List<LineRenderer> lines = new List<LineRenderer>();
    LineRenderer currentLine;

    void Start()
    {
        if (raycastCamera == null) raycastCamera = Camera.main;
        if (lineMaterial == null)
        {
            // fallback simple material
            lineMaterial = new Material(Shader.Find("Sprites/Default"));
            lineMaterial.color = Color.black;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(clearKey))
        {
            ClearBoard();
        }

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

    void TryBeginStroke()
    {
        Ray r = raycastCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(r, out RaycastHit hit))
        {
            if (hit.collider == this.GetComponent<Collider>())
            {
                // start new line
                GameObject go = new GameObject("Line");
                go.transform.parent = this.transform;
                currentLine = go.AddComponent<LineRenderer>();
                currentLine.material = lineMaterial;
                currentLine.startWidth = lineWidth;
                currentLine.endWidth = lineWidth;
                currentLine.useWorldSpace = true;
                currentLine.textureMode = LineTextureMode.Stretch;
                currentLine.numCapVertices = 4;
                lines.Add(currentLine);

                Vector3 hitPos = hit.point + (hit.normal * 0.001f);
                currentLine.positionCount = 1;
                currentLine.SetPosition(0, hitPos);
            }
        }
    }

    void ContinueStroke()
    {
        Ray r = raycastCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(r, out RaycastHit hit))
        {
            if (hit.collider == this.GetComponent<Collider>())
            {
                Vector3 hitPos = hit.point + (hit.normal * 0.001f);
                // avoid adding too-close points
                if (currentLine.positionCount == 0 || Vector3.Distance(currentLine.GetPosition(currentLine.positionCount - 1), hitPos) > 0.01f)
                {
                    currentLine.positionCount++;
                    currentLine.SetPosition(currentLine.positionCount - 1, hitPos);
                }
            }
        }
    }

    void EndStroke()
    {
        currentLine = null;
    }

    public void ClearBoard()
    {
        foreach (var l in lines)
        {
            if (l != null) Destroy(l.gameObject);
        }
        lines.Clear();
        currentLine = null;
    }
}
