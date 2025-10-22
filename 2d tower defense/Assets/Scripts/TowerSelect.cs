using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class TowerSelect : MonoBehaviour, IPointerClickHandler
{
    public Tower tower;                // tự gán nếu null
    public TowerInfoPanel panel;       // kéo panel từ Scene vào Inspector

    void Awake()
    {
        if (tower == null) tower = GetComponent<Tower>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (tower == null || panel == null) return;

        if (panel.CurrentTower == tower)
            panel.Hide();
        else
            panel.Show(tower);
    }

}
