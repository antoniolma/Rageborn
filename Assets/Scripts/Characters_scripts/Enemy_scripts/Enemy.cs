using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int damage = 10;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;
    
    [Header("Visual")]
    [SerializeField] private bool flipSpriteBasedOnDirection = true;
    
    [Header("UI (Optional)")]
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private Canvas healthBarCanvas;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip attackSound;
    private AudioSource audioSource;
    
    [Header("Drops (Optional)")]
    [SerializeField] private GameObject[] dropItems;
    [SerializeField] private float dropChance = 0.3f;
    
    private int currentHealth;
    private Transform player;
    private NavMeshAgent navAgent;
    private float lastAttackTime;
    private DamageFlash damageFlash;
    
    void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Transform>();
        navAgent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        damageFlash = GetComponent<DamageFlash>();
        
        if (navAgent != null)
        {
            navAgent.updateRotation = false;
            navAgent.updateUpAxis = false;
        }
        
        // Configura a barra de vida do inimigo
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
        }
        
        // Faz a barra de vida olhar sempre para a câmera
        if (healthBarCanvas != null)
        {
            healthBarCanvas.worldCamera = Camera.main;
        }
        
        Debug.Log($"👾 Enemy spawned! HP: {currentHealth}/{maxHealth}");
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Flip do sprite baseado na direção (ADICIONADO do segundo arquivo)
        if (flipSpriteBasedOnDirection)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            if (direction.x < 0)
                transform.localScale = new Vector3(-1, 1, 1); // Olha pra esquerda
            else if (direction.x > 0)
                transform.localScale = new Vector3(1, 1, 1); // Olha pra direita
        }
        
        // Se estiver longe, persegue o jogador
        if (distanceToPlayer > attackRange)
        {
            if (navAgent != null && navAgent.isActiveAndEnabled)
            {
                navAgent.SetDestination(player.position);
            }
        }
        // Se estiver perto, ataca
        else if (Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
        }
        
        // Mantém a barra de vida sempre virada para a câmera
        if (healthBarCanvas != null)
        {
            healthBarCanvas.transform.rotation = Camera.main.transform.rotation;
        }
    }
    
    private void AttackPlayer()
    {
        lastAttackTime = Time.time;
        PlayerController playerController = player.GetComponent<PlayerController>();
        
        if (playerController != null)
        {
            playerController.TakeDamage(damage);
            Debug.Log($"👾 Enemy atacou Player! Dano: {damage}");
            
            // Toca som de ataque
            if (audioSource != null && attackSound != null)
            {
                audioSource.PlayOneShot(attackSound);
            }
        }
    }
    
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);
        
        Debug.Log($"👾 Enemy HP: {currentHealth}/{maxHealth}");
        
        // Atualiza UI
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }
        
        // Feedback visual
        if (damageFlash != null)
        {
            damageFlash.Flash();
        }
        
        // Toca som de dano
        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        Debug.Log("💀 Enemy morreu!");
        
        // Toca som de morte
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Drop de itens
        DropItems();
        
        // TODO: Add XP, score, etc
        
        Destroy(gameObject);
    }
    
    private void DropItems()
    {
        if (dropItems.Length == 0) return;
        
        // Chance de dropar item
        if (Random.value <= dropChance)
        {
            int randomIndex = Random.Range(0, dropItems.Length);
            GameObject drop = dropItems[randomIndex];
            
            if (drop != null)
            {
                Instantiate(drop, transform.position, Quaternion.identity);
                Debug.Log($"💎 Enemy dropou: {drop.name}");
            }
        }
    }
}