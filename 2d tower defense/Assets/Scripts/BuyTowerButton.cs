using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Nút mua trụ — tự động rebind BuildManager khi đổi scene / retry.
/// Hoạt động tốt khi Canvas là DontDestroyOnLoad (Awake có thể chạy trước map).
/// </summary>
[RequireComponent(typeof(Graphic))]
public class BuyTowerButton : MonoBehaviour, IPointerClickHandler
{
    [Header("Tower Index (khớp BuildManager.towers)")]
    public int index = 0;

    [Header("UI (tuỳ chọn)")]
    public TMP_Text priceTMP;
    public Text priceText;
    public GameObject lockOverlay;

    [Header("Feedback (tuỳ chọn)")]
    public bool flashRedOnDeny = true;
    public float flashTime = 0.12f;

    [Header("Optional direct ref (nếu có)")]
    public BuildManager buildOverride; // cho phép kéo thẳng từ scene nếu muốn

    BuildManager bm;
    TowerItem data;
    Color? priceOrig;

    void Awake()
    {
        if (priceTMP) priceOrig = priceTMP.color;
        else if (priceText) priceOrig = priceText.color;

        GetComponent<Graphic>().raycastTarget = true;

        // Đừng ép bind trong Awake() (có thể chạy trước khi map spawn BuildManager)
        // Chúng ta bind trong Start() + sau khi sceneLoaded.
    }

    void Start()
    {
        // Bind trễ: đợi 1 frame để BuildManager trong map kịp Awake
        StartCoroutine(DeferredInitialBind());
    }

    IEnumerator DeferredInitialBind()
    {
        // Nếu đang chuyển scene (Retry) → chờ đến frame sau
        yield return null;

        // Thử bind 1 lần
        TryBindBuildManager(false);

        // Nếu vẫn chưa có, poll nhẹ vài lần (trong ~0.5s)
        float t = 0f;
        while (bm == null && t < 0.5f)
        {
            TryBindBuildManager(false);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        TryBindData();
        RefreshAffordable();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // Nếu đã có bm (ví dụ scene reload xong) → listen coins
        if (bm) bm.onCoinsChanged += OnCoinsChanged;
        RefreshAffordable();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (bm) bm.onCoinsChanged -= OnCoinsChanged;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Khi map mới load xong, bind lại chắc chắn
        StartCoroutine(BindAfterSceneLoaded());
    }

    IEnumerator BindAfterSceneLoaded()
    {
        // Chờ 1 frame để BuildManager trong scene mới Awake trước
        yield return null;
        TryBindBuildManager(false);

        // Thêm 1–2 frame dự phòng nếu vẫn null
        if (bm == null) { yield return null; TryBindBuildManager(false); }
        if (bm == null) { yield return null; TryBindBuildManager(false); }

        TryBindData();
        RefreshAffordable();

        // Re-subscribe coin event
        if (bm) { bm.onCoinsChanged -= OnCoinsChanged; bm.onCoinsChanged += OnCoinsChanged; }
    }

    void TryBindBuildManager(bool logIfNull = true)
    {
        // Ưu tiên override nếu được kéo sẵn
        if (buildOverride != null) bm = buildOverride;

        if (bm == null) bm = BuildManager.I;
        if (bm == null) bm = FindObjectOfType<BuildManager>();

        if (bm == null && logIfNull)
            Debug.LogWarning("[BuyTowerButton] BuildManager vẫn null (map chưa spawn?)");
    }

    void TryBindData()
    {
        if (!bm) { data = null; return; }
        if (bm.towers == null || index < 0 || index >= bm.towers.Length)
        {
            data = null; return;
        }

        data = bm.towers[index];
        string costStr = (data != null) ? data.cost.ToString() : "";
        if (priceTMP) priceTMP.text = costStr;
        if (priceText) priceText.text = costStr;
    }

    void OnCoinsChanged(int _) => RefreshAffordable();

    void RefreshAffordable()
    {
        if (!bm || data == null) return;

        bool ok = bm.CanAfford(data.cost);
        if (lockOverlay) lockOverlay.SetActive(!ok);

        if (priceOrig.HasValue)
        {
            var onCol = new Color(1f, 0.95f, 0.6f);
            var offCol = new Color(0.7f, 0.7f, 0.7f);
            if (priceTMP) priceTMP.color = ok ? onCol : offCol;
            if (priceText) priceText.color = ok ? onCol : offCol;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // Fallback: người dùng click lần đầu mà bm chưa bind → thử bind gấp (có log)
        if (bm == null) { TryBindBuildManager(true); TryBindData(); }

        if (bm == null)
        {
            // Nếu bạn dùng HUD/Bootstrap spawn BuildManager trễ hơn, có thể dùng direct ref buildOverride để không bị null ở đây.
            return;
        }

        if (data == null)
        {
            Debug.LogWarning($"[BuyTowerButton] towers[{index}] null hoặc index sai");
            return;
        }

        if (bm.CanAfford(data.cost))
        {
            bm.SelectIndex(index);
        }
        else if (flashRedOnDeny)
        {
            StopAllCoroutines();
            StartCoroutine(FlashPrice(Color.red));
        }
    }

    IEnumerator FlashPrice(Color c)
    {
        if (priceTMP) priceTMP.color = c;
        if (priceText) priceText.color = c;
        yield return new WaitForSeconds(flashTime);
        if (priceOrig.HasValue)
        {
            if (priceTMP) priceTMP.color = priceOrig.Value;
            if (priceText) priceText.color = priceOrig.Value;
        }
    }
}
