using UnityEngine;
using UnityEngine.InputSystem;

public class TowerPlacer : MonoBehaviour
{
    [Header("Refs")]
    public BuildManager build;
    public Camera cam;
    public Grid grid;

    [Header("UI")]
    public TowerInfoPanel infoPanel; // 🔹 Kéo TowerInfoPanel từ Canvas trong Scene vào đây

    [Header("Masks")]
    public LayerMask groundMask;
    public LayerMask blockedMask;

    [Header("Ghost visuals")]
    public Color okColor = new Color(0f, 1f, 0f, 0.55f);
    public Color badColor = new Color(1f, 0f, 0f, 0.55f);

    GameObject ghost;
    SpriteRenderer[] ghostSRs;
    RangeCircle2D ghostRange;
    float placeRadius = 0.45f;
    Tower ghostTowerPrefab;
    bool hasSelection;
    Vector3 placePos;
    bool canPlace;

    void Awake()
    {
        if (!build) build = BuildManager.I;
        if (!cam) cam = Camera.main;

        if (groundMask.value == 0) groundMask = LayerMask.GetMask("Ground");
        if (blockedMask.value == 0) blockedMask = LayerMask.GetMask("Path", "Tower", "Enemy", "Obstacle");

        if (build) build.onSelectionChanged += OnSelectionChanged;
    }

    void OnDestroy()
    {
        if (build) build.onSelectionChanged -= OnSelectionChanged;
    }

    void Update()
    {
        if (!hasSelection) return;

        Vector3 world = GetMouseWorld();
        placePos = Snap(world);

        UpdateGhostTransform(placePos);
        canPlace = IsValidPlacement(placePos);

        ApplyGhostTint(canPlace ? okColor : badColor);

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPlace();
        }

        bool rmb = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        bool esc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        if (rmb || esc)
        {
            CancelPlacement();
        }
    }

    // ====== Selection ======
    void OnSelectionChanged(int idx)
    {
        ClearGhost();

        var item = build != null ? build.Selected : null;
        hasSelection = (item != null);
        if (!hasSelection) return;

        ghost = Instantiate(item.prefab);
        ghost.name = "[Ghost] " + item.name;
        ghost.layer = LayerMask.NameToLayer("Default");

        foreach (var c in ghost.GetComponentsInChildren<Collider2D>(true)) c.enabled = false;
        foreach (var rb in ghost.GetComponentsInChildren<Rigidbody2D>(true)) { rb.simulated = false; rb.isKinematic = true; }

        var tower = ghost.GetComponent<Tower>();
        if (tower) tower.enabled = false;
        foreach (var anim in ghost.GetComponentsInChildren<Animator>(true)) anim.enabled = false;

        ghostSRs = ghost.GetComponentsInChildren<SpriteRenderer>(true);

        placeRadius = (item.placeRadius > 0f) ? item.placeRadius : GuessPlaceRadius(item.prefab);
        ghostTowerPrefab = item.prefab.GetComponent<Tower>();

        ghostRange = ghost.AddComponent<RangeCircle2D>();
        ghostRange.radius = (ghostTowerPrefab != null) ? ghostTowerPrefab.range : 3f;
        ghostRange.color = new Color(1f, 1f, 1f, 0.35f);
    }

    float GuessPlaceRadius(GameObject prefab)
    {
        var circ = prefab.GetComponent<CircleCollider2D>();
        if (circ) return circ.radius * Mathf.Max(prefab.transform.lossyScale.x, prefab.transform.lossyScale.y);
        var box = prefab.GetComponent<BoxCollider2D>();
        if (box) return Mathf.Max(box.size.x, box.size.y) * 0.5f * Mathf.Max(prefab.transform.lossyScale.x, prefab.transform.lossyScale.y);
        return 0.45f;
    }

    void ClearGhost()
    {
        if (ghost) Destroy(ghost);
        ghost = null; ghostSRs = null; ghostRange = null;
        hasSelection = false; canPlace = false; ghostTowerPrefab = null;
    }

    // ====== Placement ======
    void TryPlace()
    {
        var sel = build.Selected;
        if (sel == null) return;

        if (!canPlace)
        {
            ToastManager.Show("Không thể đặt ở đây");
            return;
        }
        if (!build.Spend(sel.cost))
        {
            ToastManager.Show("Không đủ tiền");
            return;
        }

        // ✅ Đặt trụ thật
        var towerGO = Instantiate(sel.prefab, placePos, Quaternion.identity);
        towerGO.layer = LayerMask.NameToLayer("Tower");
        towerGO.name = $"Tower ({FindObjectsOfType<Tower>().Length})";

        // ✅ Gắn TowerSelect nếu chưa có
        var select = towerGO.GetComponent<TowerSelect>();
        if (select == null)
            select = towerGO.AddComponent<TowerSelect>();

        // ✅ Gán đúng Tower & Panel trong scene (không dùng FindObjectOfType)
        select.tower = towerGO.GetComponent<Tower>();
        select.panel = infoPanel; // 🔹 Panel thật trong scene (gán qua Inspector)

        Debug.Log($"Spawned {towerGO.name}, panel assigned = {(infoPanel != null ? infoPanel.name : "NULL")}");
        Debug.Log($"select.panel after assign = {(select.panel != null ? select.panel.name : "NULL")}");
    }

    void CancelPlacement()
    {
        build.Unselect();
        ClearGhost();
    }

    // ====== Helpers ======
    Vector3 GetMouseWorld()
    {
        Vector3 m = Vector3.zero;
        if (Mouse.current != null)
        {
            Vector2 mp = Mouse.current.position.ReadValue();
            m = new Vector3(mp.x, mp.y, Mathf.Abs(cam.transform.position.z));
        }
        else
        {
            m = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, Mathf.Abs(cam.transform.position.z));
        }
        return cam.ScreenToWorldPoint(m);
    }

    Vector3 Snap(Vector3 world)
    {
        if (grid)
        {
            Vector3Int cell = grid.WorldToCell(world);
            Vector3 cellCenter = grid.GetCellCenterWorld(cell);
            cellCenter.z = 0f;
            return cellCenter;
        }
        float step = 0.5f;
        float x = Mathf.Round(world.x / step) * step;
        float y = Mathf.Round(world.y / step) * step;
        return new Vector3(x, y, 0f);
    }

    void UpdateGhostTransform(Vector3 pos)
    {
        if (!ghost) return;
        ghost.transform.position = pos;
    }

    bool IsValidPlacement(Vector3 pos)
    {
        bool onGround =
            Physics2D.OverlapCircle(pos, 0.05f, groundMask) != null
            || (grid != null);
        if (!onGround) return false;

        bool blocked = Physics2D.OverlapCircle(pos, placeRadius, blockedMask) != null;
        return !blocked;
    }

    void ApplyGhostTint(Color c)
    {
        if (ghostSRs == null) return;
        foreach (var sr in ghostSRs) sr.color = c;
        if (ghostRange) ghostRange.color = new Color(c.r, c.g, c.b, 0.35f);
    }
}
