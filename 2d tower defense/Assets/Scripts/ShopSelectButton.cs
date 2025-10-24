using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ShopSelectButton : MonoBehaviour
{
    [Header("Config")]
    public int index = 0; // 0 hoặc 1

    [Header("UI Refs")]
    public Button buyButton;          // Nút chọn
    public Image icon;                // Icon tower
    public Text priceText;            // Text thường (nếu dùng UI Text)
    public TMP_Text priceTextTMP;     // TextMeshPro (nếu dùng TMP)
    public GameObject lockOverlay;    // Panel mờ (Inactive mặc định)

    private BuildManager bm;
    private TowerItem data;

    void Awake()
    {
        bm = BuildManager.I;
    }

    void OnEnable()
    {
        TryBindData();
        if (bm != null)
        {
            bm.onCoinsChanged += OnCoinsChanged;
        }
        RefreshAffordable();
    }

    void OnDisable()
    {
        if (bm != null)
        {
            bm.onCoinsChanged -= OnCoinsChanged;
        }
    }

    // Bấm chọn tower
    public void OnClickSelect()
    {
        if (bm == null || data == null) return;

        if (bm.CanAfford(data.cost))
        {
            bm.SelectIndex(index);
        }
        else
        {
            StartCoroutine(DenyFX());
        }
    }

    void TryBindData()
    {
        if (bm == null || bm.towers == null) return;
        if (index < 0 || index >= bm.towers.Length) return;

        data = bm.towers[index];
        SetText(priceText, priceTextTMP, data != null ? data.cost.ToString() : "");

        if (buyButton)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnClickSelect);
        }
    }

    void OnCoinsChanged(int _) => RefreshAffordable();

    void RefreshAffordable()
    {
        if (bm == null || data == null) return;

        bool affordable = bm.CanAfford(data.cost);

        if (buyButton)   buyButton.interactable = affordable;
        if (lockOverlay) lockOverlay.SetActive(!affordable);

        var colOn  = new Color(1f, 0.95f, 0.6f);
        var colOff = new Color(0.7f, 0.7f, 0.7f);
        SetColor(priceText, priceTextTMP, affordable ? colOn : colOff);
    }

    IEnumerator DenyFX()
    {
        var old = GetColor(priceText, priceTextTMP);
        SetColor(priceText, priceTextTMP, Color.red);
        yield return new WaitForSeconds(0.12f);
        SetColor(priceText, priceTextTMP, old);
    }

    // Helper cho Text hoặc TMP
    void SetText(Text t, TMP_Text tt, string s)
    { if (tt) tt.text = s; if (t) t.text = s; }

    void SetColor(Text t, TMP_Text tt, Color c)
    { if (tt) tt.color = c; if (t) t.color = c; }

    Color GetColor(Text t, TMP_Text tt)
    { return tt ? tt.color : (t ? t.color : Color.white); }
}
