using UnityEngine;

/// <summary>
/// Spawns a simple classroom layout: rows x cols desks, instructor podium, and a whiteboard.
/// Inspector-driven: assign prefabs for DeskPrefab, PodiumPrefab, WhiteboardPrefab.
/// This is intended for a stylized investor demo (readable, optimized).
/// </summary>
public class ClassroomBootstrapper : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject DeskPrefab;
    public GameObject PodiumPrefab;
    public GameObject WhiteboardPrefab;
    public GameObject StudentAvatarPrefab; // optional ghost avatars

    [Header("Layout")]
    public int rows = 3;
    public int cols = 5;
    public Vector3 deskSpacing = new Vector3(1.6f, 0f, 1.4f);
    public Vector3 firstDeskPosition = new Vector3(-3.0f, 0f, 2.0f);

    [Header("Instructor Area")]
    public Vector3 podiumPosition = new Vector3(0f, 0f, -3.5f);
    public Vector3 whiteboardPosition = new Vector3(0f, 0f, -3.8f);

    [Header("Avatar Ghosting")]
    public bool spawnGhosts = true;
    public float ghostSpacingOffset = 0.2f;

    void Start()
    {
        if (DeskPrefab == null || PodiumPrefab == null || WhiteboardPrefab == null)
        {
            Debug.LogError("Assign DeskPrefab, PodiumPrefab, and WhiteboardPrefab in the inspector.");
            return;
        }

        SpawnDesks();
        SpawnInstructorArea();
    }

    void SpawnDesks()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 pos = firstDeskPosition + new Vector3(c * deskSpacing.x, 0, r * deskSpacing.z);
                var desk = Instantiate(DeskPrefab, pos, Quaternion.identity, this.transform);
                desk.name = $"Desk_{r}_{c}";
                // mark static for lightbaking
                desk.isStatic = true;

                if (spawnGhosts && StudentAvatarPrefab != null)
                {
                    Vector3 ghostPos = pos + new Vector3(0, ghostSpacingOffset, -0.4f);
                    var ghost = Instantiate(StudentAvatarPrefab, ghostPos, Quaternion.identity, this.transform);
                    ghost.name = $"StudentGhost_{r}_{c}";
                    // Make ghost slightly transparent if it has a renderer
                    var rend = ghost.GetComponentInChildren<Renderer>();
                    if (rend != null)
                    {
                        foreach (var rgr in rend.GetComponentsInChildren<Renderer>())
                        {
                            // Create material instances to avoid modifying shared materials
                            Material[] instanceMaterials = rgr.materials; // Creates instances automatically
                            for (int i = 0; i < instanceMaterials.Length; i++)
                            {
                                if (instanceMaterials[i] != null && instanceMaterials[i].HasProperty("_Color"))
                                {
                                    var col = instanceMaterials[i].color;
                                    col.a = 0.85f;
                                    instanceMaterials[i].color = col;
                                }
                            }
                            rgr.materials = instanceMaterials;
                        }
                    }
                }
            }
        }
    }

    void SpawnInstructorArea()
    {
        var podium = Instantiate(PodiumPrefab, podiumPosition, Quaternion.identity, this.transform);
        podium.name = "Podium";
        podium.isStatic = true;

        var whiteboard = Instantiate(WhiteboardPrefab, whiteboardPosition, Quaternion.identity, this.transform);
        whiteboard.name = "Whiteboard";
        whiteboard.isStatic = true;
    }

#if UNITY_EDITOR
    // Inspector convenience
    private void OnValidate()
    {
        if (rows < 1) rows = 1;
        if (cols < 1) cols = 1;
    }
#endif
}