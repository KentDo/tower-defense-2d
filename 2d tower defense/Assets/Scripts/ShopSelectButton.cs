using UnityEngine;

public class ShopSelectButton : MonoBehaviour
{
    public int index; // 0,1,2...
    public void OnClickSelect()
    {
        if (BuildManager.I) BuildManager.I.SelectIndex(index);
    }
}
