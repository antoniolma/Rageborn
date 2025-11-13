using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int damage = 10;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 5.0f;
    [SerializeField] private float attackCooldown = 1f;
    
    private int currentHealth;
    private Transform player;
    NavMeshAgent navAgent;
    private float lastAttackTime;
    
    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.updateRotation = false;
        navAgent.updateUpAxis = false;
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        Vector2 direction = (player.position - transform.position).normalized;
        if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1); // Olha pra esquerda
        else if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1); // Olha pra direita
        
        // Se estiver longe, persegue o jogador
        if (distanceToPlayer > attackRange)
        {
            navAgent.SetDestination(player.position);
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
