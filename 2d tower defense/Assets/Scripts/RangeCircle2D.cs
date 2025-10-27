using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RangeCircle2D : MonoBehaviour
{
    public float radius = 3f;
    public int segments = 64;
    public float width = 0.03f;
    public Color color = new Color(1f, 1f, 1f, 0.35f);

    LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = segments;
        lr.startWidth = lr.endWidth = width;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = color;
        Rebuild();
    }

    void OnValidate()
    {
        if (lr == null) lr = GetComponent<LineRenderer>();
        if (lr == null) return;
        lr.startColor = lr.endColor = color;
        Rebuild();
    }

    public void Rebuild()
    {
        if (lr == null || segments < 3) return;
        float aStep = Mathf.PI * 2f / segments;
        for (int i = 0; i < segments; i++)
        {
            float a = i * aStep;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f));
        }
    }
}
