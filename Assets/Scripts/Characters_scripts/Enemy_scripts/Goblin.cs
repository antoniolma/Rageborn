using UnityEngine;

public class Goblin : Enemy
{
    [Header("Goblin Dash Settings")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashCooldown = 2.5f;
    [SerializeField] private float dashDuration = 0.4f;
    [SerializeField] private float dashRange = 6f;
    [SerializeField] private TrailRenderer dashTrail; // Efeito visual de dash
    
    private float lastDashTime = -999f;
    private bool isDashing = false;
    private Vector2 dashDirection;
    private float dashEndTime;
    private Rigidbody2D rb;
    
    protected override void Start()
    {
        // 👹 Stats do Goblin - RÁPIDO E FRACO
        damage = 7; // Dano LEVE
        moveSpeed = 4.5f; // RÁPIDO
        attackRange = 1f;
        attackCooldown = 0.7f; // Ataque rápido
        
        base.Start();
        
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        // Goblin tem POUCA VIDA
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.SetMaxHealth(30); // Vida BAIXA
        }
        
        // Desativa trail no início
        if (dashTrail != null)
        {
            dashTrail.emitting = false;
        }
    }
    
    protected override void Update()
    {
        if (player == null) return;
        
        // Se estiver dashando
        if (isDashing)
        {
            HandleDash();
            return;
        }
        
        base.Update();
        
        // Tenta dar dash se estiver no alcance
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (Time.time >= lastDashTime + dashCooldown && 
            distanceToPlayer <= dashRange && 
            distanceToPlayer > attackRange * 1.5f)
        {
            StartDash();
        }
    }
    
    private void StartDash()
    {
        isDashing = true;
        lastDashTime = Time.time;
        dashEndTime = Time.time + dashDuration;
        
        // Calcula direção do dash DIRETO NO PLAYER
        dashDirection = (player.position - transform.position).normalized;
        
        // Para o NavMeshAgent durante o dash
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
        
        // Ativa trail visual
        if (dashTrail != null)
        {
            dashTrail.emitting = true;
        }
        
        Debug.Log("🏃 Goblin dando DASH!");
    }
    
    private void HandleDash()
    {
        if (Time.time >= dashEndTime)
        {
            // Termina o dash
            isDashing = false;
            
            // Reativa o NavMeshAgent
            if (navAgent != null)
            {
                navAgent.enabled = true;
                // Força reposicionamento no NavMesh
                if (navAgent.isOnNavMesh)
                {
                    navAgent.SetDestination(transform.position);
                }
            }
            
            // Desativa trail
            if (dashTrail != null)
            {
                dashTrail.emitting = false;
            }
            
            return;
        }
        
        // Move RÁPIDO na direção do dash
        Vector2 movement = dashDirection * dashSpeed * Time.deltaTime;
        
        if (rb != null)
        {
            rb.MovePosition(rb.position + movement);
        }
        else
        {
            transform.position += (Vector3)movement;
        }
        
        // Flip do sprite durante dash
        if (flipSpriteBasedOnDirection)
        {
            if (dashDirection.x < 0)
                transform.localScale = new Vector3(-1, 1, 1);
            else if (dashDirection.x > 0)
                transform.localScale = new Vector3(1, 1, 1);
        }
    }
    
    protected override void AttackPlayer()
    {
        // Ataque rápido do goblin
        base.AttackPlayer();
        
        Debug.Log("👹 Goblin atacou!");
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Se dash acerta o player durante o dash, causa dano
        if (isDashing && collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
                Debug.Log("💥 Goblin acertou dash no player!");
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Desenha range do dash
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dashRange);
    }
}