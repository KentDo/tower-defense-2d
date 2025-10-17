using UnityEngine;

public class HealthBarFollow : MonoBehaviour
{
    public Transform target;                    // để trống: script sẽ tự tìm Health ở cha
    public Vector3 worldOffset = new Vector3(0f, 1.0f, 0f);

    void OnEnable()
    {
        if (!target)
        {
            var h = GetComponentInParent<Health>();
            if (h) target = h.transform;
        }
    }

    void LateUpdate()
    {
        if (!target) return;
        transform.position = target.position + worldOffset;
    }
}
