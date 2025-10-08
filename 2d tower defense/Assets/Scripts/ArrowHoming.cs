using UnityEngine;

public class ArrowHoming : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 1;
    public float lifeTime = 5f;

    Transform target;
    LayerMask enemyMask;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (target != null)
        {
            Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(dir * speed * Time.deltaTime);

            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, ang - 90f);
        }
        else
        {
            transform.position += transform.up * speed * Time.deltaTime;
        }
    }

    public void Init(Transform t, LayerMask mask)
    {
        target = t;
        enemyMask = mask;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
