using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 4f;

    void Start()
    {
        // Destroy the bullet after a few seconds
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerCombat>()?.TakeDamage();
            Destroy(gameObject);
        }
        else if (!other.isTrigger) // Hit a wall
        {
            Destroy(gameObject);
        }
    }
}