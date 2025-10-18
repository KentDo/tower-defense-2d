using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Đặt trụ kiểu TD: chọn trụ ở BuildManager, ghost theo chuột, 
/// xanh/đỏ hợp lệ, LMB để đặt, RMB/Esc để hủy.
/// </summary>
public class TowerPlacer : MonoBehaviour
{
    [Header("Refs")]
    public BuildManager build;                // nếu để trống -> tự tìm BuildManager.I
    public Camera cam;                        // nếu trống -> Camera.main
    public Grid grid;                         // optional: snap theo Grid Unity

    [Header("Masks")]
    public LayerMask groundMask;              // = Ground
    public LayerMask blockedMask;             // = Path + Tower + Enemy + Obstacle

    [Header("Ghost visuals")]
    public Color okColor = new Color(0f, 1f, 0f, 0.55f);
    public Color badColor = new Color(1f, 0f, 0f, 0.55f);

    GameObject ghost;                         // instance ghost hiện tại
    SpriteRenderer[] ghostSRs;                // tất cả SR của ghost
    RangeCircle2D ghostRange;                 // vẽ vòng tầm bắn
    float placeRadius = 0.45f;                // bán kính kiểm tra chồng lấn
    Tower ghostTowerPrefab;                   // tham chiếu prefab để đọc range
    bool hasSelection;                        // có trụ đang chọn
    Vector3 placePos;                         // vị trí snap cuối cùng
    bool canPlace;                            // hợp lệ để đặt

    void Awake()
    {
        if (!build) build = BuildManager.I;
        if (!cam) cam = Camera.main;

        // default masks nếu chưa kéo trong Inspector
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
        // Không có trụ để đặt -> chỉ nghe sự kiện chọn
        if (!hasSelection) return;

        // di chuyển ghost theo chuột (+ snap)
        Vector3 world = GetMouseWorld();
        placePos = Snap(world);

        UpdateGhostTransform(placePos);

        // kiểm tra hợp lệ
        canPlace = IsValidPlacement(placePos);

        // đổi màu ghost theo trạng thái
        ApplyGhostTint(canPlace ? okColor : badColor);

        // LMB đặt
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPlace();
        }

        // RMB hoặc Esc -> hủy
        bool rmb = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        bool esc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        if (rmb || esc)  // ✅
        {
            CancelPlacement();
        }
    }

    // ====== Selection from BuildManager ======
    void OnSelectionChanged(int idx)
    {
        ClearGhost();

        var item = build != null ? build.Selected : null;
        hasSelection = (item != null);
        if (!hasSelection) return;

        ghost = Instantiate(item.prefab);
        ghost.name = "[Ghost] " + item.name;
        ghost.layer = LayerMask.NameToLayer("Default");

        // 1) TẮT va chạm & rigidbody
        foreach (var c in ghost.GetComponentsInChildren<Collider2D>(true)) c.enabled = false;
        foreach (var rb in ghost.GetComponentsInChildren<Rigidbody2D>(true)) { rb.simulated = false; rb.isKinematic = true; }

        // 2) TẮT script hành vi (Tower, Animator, v.v.) để ghost KHÔNG bắn
        var tower = ghost.GetComponent<Tower>();
        if (tower) tower.enabled = false;
        foreach (var anim in ghost.GetComponentsInChildren<Animator>(true)) anim.enabled = false;

        // 3) Lấy renderer để tô màu xanh/đỏ
        ghostSRs = ghost.GetComponentsInChildren<SpriteRenderer>(true);

        // 4) Bán kính kiểm tra & range vòng tầm bắn
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

        // Đặt trụ thực sự
        var towerGO = Instantiate(sel.prefab, placePos, Quaternion.identity);
        towerGO.layer = LayerMask.NameToLayer("Tower");

        // Sau khi đặt xong: hủy ghost hoặc giữ để đặt tiếp (tuỳ thiết kế)
        // Ở đây: giữ chế độ đặt liên tục
        // Nếu muốn hủy ngay: build.Unselect();

        // Update: nếu ghost có RangeCircle, giữ nguyên để người chơi đặt liên tục
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
            // fallback an toàn khi không có thiết bị chuột
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
        // snap theo 0.5f nếu không có Grid
        float step = 0.5f;
        float x = Mathf.Round(world.x / step) * step;
        float y = Mathf.Round(world.y / step) * step;
        return new Vector3(x, y, 0f);
    }

    void UpdateGhostTransform(Vector3 pos)
    {
        if (!ghost) return;
        ghost.transform.position = pos;
        // có thể xoay ghost cho đẹp nếu cần
    }

    bool IsValidPlacement(Vector3 pos)
    {
        // 1) Phải nằm trên Ground (hoặc nếu có Grid thì cho phép snap như Ground)
        bool onGround =
            Physics2D.OverlapCircle(pos, 0.05f, groundMask) != null
            || (grid != null); // cho phép nếu bạn dùng Grid snap và không muốn thêm collider

        if (!onGround) return false;

        // 2) Không đè lên Path/Tower/Enemy/Obstacle trong bán kính trụ
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
