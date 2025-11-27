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
    private Rigidbody2D rb; // ✅ ADICIONADO
    
    protected override void Start()
    {
        // Stats do Beholder
        maxHealth = 50;
        damage = 8;
        moveSpeed = 2f;
        attackRange = 6f;
        attackCooldown = 2f;
        
        base.Start();
        
        // ✅ CONFIGURAÇÃO PARA VOO
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
        
        // ✅ Configura Rigidbody2D para voo
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f; // ✅ SEM GRAVIDADE
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // ✅ NÃO GIRA
        }
        
        // ✅ Configura Collider como Trigger (não colide fisicamente)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true; // ✅ PASSA ATRAVÉS DE PAREDES
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
        
        // ✅ Move usando Rigidbody2D (mais suave e física-friendly)
        if (rb != null)
        {
            Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime);
            rb.MovePosition(newPosition);
        }
        else
        {
            // Fallback
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
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
        if (attackSound != null)
        {
            if (audioSource != null)
            {
                audioSource.volume = 0.75f;
                audioSource.PlayOneShot(attackSound);
            }
            else
            {
                GameObject soundObject = new GameObject("AttackSound");
                soundObject.transform.position = transform.position;
                AudioSource tempAudioSource = soundObject.AddComponent<AudioSource>();
                tempAudioSource.clip = attackSound;
                tempAudioSource.volume = 0.75f;
                tempAudioSource.Play();
                Destroy(soundObject, attackSound.length);
            }
        }
        
        Debug.Log("👁️ Beholder atirou projétil!");
    }
    
    protected override void AttackPlayer()
    {
        // Beholder não faz ataque corpo a corpo
    }
    
    // ✅ IMPORTANTE: Não colidir com player fisicamente, só detectar
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Pode adicionar lógica aqui se precisar
    }
}