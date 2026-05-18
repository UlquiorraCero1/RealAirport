using UnityEngine;

public class HeavyEnemy : MonoBehaviour
{
    [Header("Tank Settings")]
    public int hitsToDie = 4;
    
    private int hitCount = 0;
    private EnemyHealth enemyHealth;
    private Renderer rend;
    private float lastHitTime = 0f; 

    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        rend = GetComponent<Renderer>();

        transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        if (rend != null)
            rend.material.color = new Color(0.4f, 0.1f, 0.1f);
    }
    public bool TakeArmorHit()
    {
        if (Time.time - lastHitTime < 0.05f)
        {
            return false;
        }

        lastHitTime = Time.time;
        hitCount++;

        if (rend != null)
            StartCoroutine(FlashWhite());

        return hitCount >= hitsToDie;
    }
    public void TakeHeavyHit()
    {
        if (TakeArmorHit())
        {
            enemyHealth.TakeHit();
        }
    }

    System.Collections.IEnumerator FlashWhite()
    {
        Color original = new Color(0.4f, 0.1f, 0.1f);
        rend.material.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        rend.material.color = original;
    }
}