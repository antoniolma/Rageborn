using UnityEngine;

public class Beholder : Enemy
{
    [Header("Beholder Settings")]
    [SerializeField] private float flyHeight = 2f;
    [SerializeField] private float hoverSpeed = 2f;
    [SerializeField] private float hoverAmplitude = 0.5f;
    [SerializeField] private float shootRange = 6f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    
    private Vector3 targetPosition;
    private float hoverOffset;
    
    protected override void Start()
    {
        // Stats do Beholder
        maxHealth = 50;
        damage = 8;
        moveSpeed = 2f;
        attackRange = 6f; // Ataque à distância
        attackCooldown = 2f;
        
        base.Start();
        
        // Beholder não usa NavMeshAgent
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
        
        hoverOffset = Random.Range(0f, Mathf.PI * 2f);
    }
    
    protected override void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Flip do sprite
        HandleSpriteFlip();
        
        // Comportamento de voo
        HandleFlying(distanceToPlayer);
        
        // Ataque à distância
        if (distanceToPlayer <= shootRange && Time.time >= lastAttackTime + attackCooldown)
        {
            ShootProjectile();
        }
        
        // Mantém a barra de vida
        if (healthBarCanvas != null)
        {
            healthBarCanvas.transform.rotation = Camera.main.transform.rotation;
        }
    }
    
    private void HandleFlying(float distanceToPlayer)
    {
        // Mantém distância ideal do jogador
        float idealDistance = shootRange * 0.7f;
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        
        if (distanceToPlayer > idealDistance)
        {
            // Move em direção ao jogador
            targetPosition = player.position - directionToPlayer * idealDistance;
        }
        else if (distanceToPlayer < idealDistance * 0.5f)
        {
            // Recua se muito próximo
            targetPosition = player.position + directionToPlayer * idealDistance;
        }
        else
        {
            // Orbita ao redor do jogador
            Vector3 perpendicular = new Vector3(-directionToPlayer.y, directionToPlayer.x, 0);
            targetPosition = player.position + perpendicular * idealDistance;
        }
        
        // Adiciona altura de voo
        targetPosition.y += flyHeight;
        
        // Adiciona movimento de hover (flutuação)
        float hover = Mathf.Sin(Time.time * hoverSpeed + hoverOffset) * hoverAmplitude;
        targetPosition.y += hover;
        
        // Move suavemente para a posição alvo
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
    
    private void ShootProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Beholder não tem projectile configurado!");
            return;
        }
        
        lastAttackTime = Time.time;
        
        Vector3 spawnPosition = shootPoint != null ? shootPoint.position : transform.position;
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        
        // Configura o projétil
        EnemyProjectile projectileScript = projectile.GetComponent<EnemyProjectile>();
        if (projectileScript != null)
        {
            Vector2 direction = (player.position - spawnPosition).normalized;
            projectileScript.Initialize(direction, damage);
        }
        
        // Toca som de ataque
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
        
        Debug.Log("👁️ Beholder atirou projétil!");
    }
    
    protected override void AttackPlayer()
    {
        // Beholder não faz ataque corpo a corpo
        // Usa ShootProjectile() ao invés
    }
}