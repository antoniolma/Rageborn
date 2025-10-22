using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int damage = 10;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;
    
    private int currentHealth;
    private Transform player;
    private float lastAttackTime;
    
    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Se estiver longe, persegue o jogador
        if (distanceToPlayer > attackRange)
        {
            ChasePlayer();
        }
        // Se estiver perto, ataca
        else if (Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
        }
    }
    
    private void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(
            transform.position, 
            player.position, 
            moveSpeed * Time.deltaTime
        );
    }
    
    private void AttackPlayer()
    {
        lastAttackTime = Time.time;
        PlayerController playerController = player.GetComponent<PlayerController>();
        
        if (playerController != null)
        {
            playerController.TakeDamage(damage);
            Debug.Log($"Enemy atacou! Dano: {damage}");
        }
    }
    
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log($"Enemy HP: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        Debug.Log("Enemy morreu!");
        // TODO: Drop de itens, XP, etc
        Destroy(gameObject);
    }
}
