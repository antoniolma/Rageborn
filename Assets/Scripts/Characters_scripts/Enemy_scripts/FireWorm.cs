using UnityEngine;

public class FireWorm : Enemy
{
    [Header("FireWorm Settings")]
    [SerializeField] private float keepDistanceRange = 7f; // Mantém distância do player
    [SerializeField] private float minDistanceFromPlayer = 5f; // Distância mínima
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private Transform mouthPosition;
    [SerializeField] private int fireballsPerBurst = 3;
    [SerializeField] private float timeBetweenFireballs = 0.3f;
    [SerializeField] private float burstCooldown = 3f;
    [SerializeField] private float fireballSpeed = 8f;
    
    private int fireballsShot = 0;
    private float nextFireballTime = 0f;
    private bool isShooting = false;
    private float lastBurstTime = -999f;
    
    protected override void Start()
    {
        // 🔥 Stats do FireWorm
        damage = 15; // Dano ALTO
        moveSpeed = 2f; // Movimento médio
        attackRange = keepDistanceRange;
        attackCooldown = burstCooldown;
        
        base.Start();
        
        // FireWorm precisa de EnemyHealth configurado com vida média
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.SetMaxHealth(50); // Vida MÉDIA
        }
    }
    
    protected override void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Flip do sprite
        HandleSpriteFlip();
        
        // Se está atirando, não se move
        if (isShooting)
        {
            HandleFireballBurst();
            return;
        }
        
        // Comportamento de manter distância
        HandleDistanceKeeping(distanceToPlayer);
        
        // Atira rajadas de bolas de fogo
        if (Time.time >= lastBurstTime + burstCooldown && distanceToPlayer <= keepDistanceRange)
        {
            StartFireballBurst();
        }
    }
    
    private void HandleDistanceKeeping(float distanceToPlayer)
    {
        if (navAgent == null || !navAgent.isActiveAndEnabled) return;
        
        // Se está muito perto, RECUA
        if (distanceToPlayer < minDistanceFromPlayer)
        {
            Vector3 retreatDirection = (transform.position - player.position).normalized;
            Vector3 retreatPosition = transform.position + retreatDirection * 2f;
            navAgent.SetDestination(retreatPosition);
        }
        // Se está longe demais, avança (mas mantém distância)
        else if (distanceToPlayer > keepDistanceRange)
        {
            Vector3 approachDirection = (player.position - transform.position).normalized;
            Vector3 targetPosition = player.position - approachDirection * (keepDistanceRange - 1f);
            navAgent.SetDestination(targetPosition);
        }
        // Na distância ideal, fica parado
        else
        {
            navAgent.SetDestination(transform.position);
        }
    }
    
    private void StartFireballBurst()
    {
        isShooting = true;
        fireballsShot = 0;
        nextFireballTime = Time.time;
        lastBurstTime = Time.time;
        
        // Para o movimento
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.isStopped = true;
        }
        
        Debug.Log("🔥 FireWorm começando rajada de bolas de fogo!");
    }
    
    private void HandleFireballBurst()
    {
        if (Time.time >= nextFireballTime)
        {
            ShootFireball();
            fireballsShot++;
            nextFireballTime = Time.time + timeBetweenFireballs;
            
            if (fireballsShot >= fireballsPerBurst)
            {
                // Terminou a rajada
                isShooting = false;
                
                // Retoma o movimento
                if (navAgent != null && navAgent.isActiveAndEnabled)
                {
                    navAgent.isStopped = false;
                }
            }
        }
    }
    
    private void ShootFireball()
    {
        if (fireballPrefab == null)
        {
            Debug.LogWarning("⚠️ FireWorm não tem fireball configurado!");
            return;
        }
        
        Vector3 spawnPosition = mouthPosition != null ? mouthPosition.position : transform.position;
        GameObject fireball = Instantiate(fireballPrefab, spawnPosition, Quaternion.identity);
        
        // Calcula direção para o player
        Vector2 direction = (player.position - spawnPosition).normalized;
        
        // Configura o projétil
        EnemyProjectile fireballScript = fireball.GetComponent<EnemyProjectile>();
        if (fireballScript != null)
        {
            // Pequeno spread aleatório
            float spread = Random.Range(-5f, 5f);
            Vector2 spreadDirection = Quaternion.Euler(0, 0, spread) * direction;
            fireballScript.Initialize(spreadDirection, damage, fireballSpeed);
        }
        else
        {
            // Fallback: move o projétil manualmente
            Rigidbody2D rb = fireball.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * fireballSpeed;
            }
            
            Destroy(fireball, 5f);
        }
        
        // Toca som de ataque
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
        
        Debug.Log("🔥 FireWorm atirou bola de fogo!");
    }
    
    protected override void AttackPlayer()
    {
        // FireWorm não ataca corpo a corpo
        // Usa apenas fireballs
    }
    
    void OnDrawGizmosSelected()
    {
        // Desenha ranges
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, keepDistanceRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDistanceFromPlayer);
    }
}