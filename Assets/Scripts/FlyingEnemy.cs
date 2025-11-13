using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Flying Enemy Stats")]
    [SerializeField] private int maxHealth = 40;
    [SerializeField] private int damage = 8;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float attackRange = 5.0f;
    [SerializeField] private float attackCooldown = 1.5f;
    
    private int currentHealth;
    private Transform player;
    private float lastAttackTime;
    private Animator animator;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        if (player == null || isDead) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Voa direto até o jogador, ignora paredes
        if (distanceToPlayer > attackRange)
        {
            FlyTowardsPlayer();
            animator.SetBool("isMoving", true);
        }
        // Se estiver perto, ataca
        else
        {
            animator.SetBool("isMoving", false);
            
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
        }
    }
    
    private void FlyTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        
        // Controla direção do sprite
        if (direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
        
        transform.position = Vector2.MoveTowards(
            transform.position, 
            player.position, 
            moveSpeed * Time.deltaTime
        );
    }
    
    private void AttackPlayer()
    {
        lastAttackTime = Time.time;
        animator.SetTrigger("attack");
        
        PlayerController playerController = player.GetComponent<PlayerController>();
        
        if (playerController != null)
        {
            playerController.TakeDamage(damage);
            Debug.Log($"Flying Enemy atacou! Dano: {damage}");
        }
    }
    
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        
        currentHealth -= damageAmount;
        animator.SetTrigger("hurt");
        Debug.Log($"Flying Enemy HP: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        isDead = true;
        animator.SetBool("isMoving", false);
        animator.SetTrigger("death");
        Debug.Log("Flying Enemy morreu!");
        
        // Destroi depois da animação de morte (ajuste o tempo se necessário)
        Destroy(gameObject, 1f);
    }
}