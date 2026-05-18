using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum State { Patrol, Alert, Chase, Attack }

    [Header("Current State")]
    public State currentState = State.Patrol;

    [Header("Weapon Settings")]
    public bool hasGun = false;
    public float shootRange = 12f;
    public float shootCooldown = 2f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public float waitAtPoint = 1.5f;

    [Header("Detection")]
    public float sightRange = 10f;
    public float sightAngle = 100f;
    public float hearingRange = 15f;

    [Header("Melee Settings")]
    public float chaseSpeed = 5f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;

    [HideInInspector] public Vector3 lastKnownSoundPos;

    private Transform player;
    private int patrolIndex = 0;
    private float waitTimer = 0f;
    private float attackTimer = 0f;
    private bool isKnockedDown = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (isKnockedDown || player == null) return;

        switch (currentState)
        {
            case State.Patrol: HandlePatrol(); break;
            case State.Alert:  HandleAlert();  break;
            case State.Chase:  HandleChase();  break;
            case State.Attack: HandleAttack(); break;
        }
    }

    void HandlePatrol()
    {
        if (patrolPoints.Length == 0) { LookForPlayer(); return; }

        Transform target = patrolPoints[patrolIndex];
        Vector3 direction = target.position - transform.position;
        direction.y = 0;

        if (direction.magnitude < 0.4f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitAtPoint)
            {
                waitTimer = 0f;
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            }
        }
        else
        {
            MoveInDirection(direction.normalized, patrolSpeed);
        }

        LookForPlayer();
    }

    void HandleAlert()
    {
        Vector3 dir = lastKnownSoundPos - transform.position;
        dir.y = 0;

        if (dir.magnitude > 1f) MoveInDirection(dir.normalized, patrolSpeed);
        else
        {
            waitTimer += Time.deltaTime;
            if (waitTimer > 3f)
            {
                waitTimer = 0f;
                currentState = State.Patrol;
            }
        }
        LookForPlayer();
    }

    void HandleChase()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0;

        if (dir.magnitude > sightRange * 1.5f)
        {
            lastKnownSoundPos = player.position;
            currentState = State.Alert;
            return;
        }

        float currentAttackRange = hasGun ? shootRange : attackRange;

        if (dir.magnitude < currentAttackRange)
        {
            currentState = State.Attack;
            return;
        }

        MoveInDirection(dir.normalized, chaseSpeed);
    }

    void HandleAttack()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0;

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir.normalized);

        float currentAttackRange = hasGun ? shootRange : attackRange;

        if (dir.magnitude > currentAttackRange * 1.5f)
        {
            currentState = State.Chase;
            return;
        }

        attackTimer += Time.deltaTime;
        float cooldown = hasGun ? shootCooldown : attackCooldown;

        if (attackTimer >= cooldown)
        {
            attackTimer = 0f;
            if (hasGun) ShootGun();
            else player.GetComponent<PlayerCombat>()?.TakeDamage();
        }
    }

    void ShootGun()
    {
        if (bulletPrefab != null)
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + transform.forward * 1f + Vector3.up * 0.5f;
            Instantiate(bulletPrefab, spawnPos, transform.rotation);
        }
    }

    void LookForPlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.magnitude > sightRange) return;

        float angle = Vector3.Angle(transform.forward, dir.normalized);
        if (angle < sightAngle / 2f)
        {
            RaycastHit hit;
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            if (Physics.Raycast(origin, dir.normalized, out hit, sightRange))
            {
                if (hit.collider.CompareTag("Player"))
                    currentState = State.Chase;
            }
        }
    }

    public void HearSound(Vector3 soundPosition)
    {
        if (currentState == State.Chase || currentState == State.Attack) return;
        lastKnownSoundPos = soundPosition;
        currentState = State.Alert;
    }

    void MoveInDirection(Vector3 dir, float speed)
    {
        transform.position += dir * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    public void SetKnockedDown(bool value)
    {
        isKnockedDown = value;
        if (rb != null) rb.linearVelocity = Vector3.zero;
    }
}