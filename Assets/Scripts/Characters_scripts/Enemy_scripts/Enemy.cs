using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected int maxHealth = 50; // ✅ ADICIONADO
    [SerializeField] protected int damage = 10;
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float attackRange = 1.5f;
    [SerializeField] protected float attackCooldown = 1f;
    
    [Header("Visual")]
    [SerializeField] protected bool flipSpriteBasedOnDirection = true;
    
    [Header("UI")] // ✅ ADICIONADO
    [SerializeField] protected Canvas healthBarCanvas;
    
    [Header("Audio (Optional)")]
    [SerializeField] protected AudioClip attackSound;
    protected AudioSource audioSource;
    
    protected Transform player;
    protected NavMeshAgent navAgent;
    protected float lastAttackTime;
    protected EnemyHealth enemyHealth;
    
    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Transform>();
        navAgent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        enemyHealth = GetComponent<EnemyHealth>();
        
        // ✅ Busca o canvas da healthbar se não foi atribuído
        if (healthBarCanvas == null)
        {
            healthBarCanvas = GetComponentInChildren<Canvas>();
        }
        
        if (navAgent != null)
        {
            navAgent.updateRotation = false;
            navAgent.updateUpAxis = false;
            navAgent.speed = moveSpeed;
        }
        
        Debug.Log($"👾 {gameObject.name} spawned!");
    }
    
    protected virtual void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Flip do sprite baseado na direção
        HandleSpriteFlip();
        
        // Comportamento de movimento e ataque
        HandleCombat(distanceToPlayer);
    }
    
    protected virtual void HandleSpriteFlip()
    {
        if (!flipSpriteBasedOnDirection || player == null) return;
        
        Vector2 direction = (player.position - transform.position).normalized;
        if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
    }
    
    protected virtual void HandleCombat(float distanceToPlayer)
    {
        // Se estiver longe, persegue o jogador
        if (distanceToPlayer > attackRange)
        {
            MoveTowardsPlayer();
        }
        // Se estiver perto, ataca
        else if (Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
        }
    }
    
    protected virtual void MoveTowardsPlayer()
    {
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.SetDestination(player.position);
        }
    }
    
    protected virtual void AttackPlayer()
    {
        lastAttackTime = Time.time;
        PlayerController playerController = player.GetComponent<PlayerController>();
        
        if (playerController != null)
        {
            playerController.TakeDamage(damage);
            Debug.Log($"👾 {gameObject.name} atacou Player! Dano: {damage}");
            
            // Toca som de ataque
            if (audioSource != null && attackSound != null)
            {
                audioSource.PlayOneShot(attackSound);
            }
        }
    }
    
    // ⚠️ MANTÉM PARA COMPATIBILIDADE (mas agora só redireciona para EnemyHealth)
    public virtual void TakeDamage(int damageAmount)
    {
        if (enemyHealth != null)
        {
            // Permite que subclasses modifiquem o dano (ex: Golem com armadura)
            int finalDamage = ModifyIncomingDamage(damageAmount);
            enemyHealth.TakeDamage(finalDamage);
        }
        else
        {
            Debug.LogWarning($"⚠️ {gameObject.name} não tem EnemyHealth component!");
        }
    }
    
    // ✅ Método virtual para subclasses modificarem dano recebido
    protected virtual int ModifyIncomingDamage(int damage)
    {
        // Por padrão, retorna dano sem modificação
        // Golem vai sobrescrever para aplicar armadura
        return damage;
    }
    
    // ✅ Getters públicos
    public int GetDamage() => damage;
    public float GetMoveSpeed() => moveSpeed;
    public float GetAttackRange() => attackRange;
    public int GetMaxHealth() => maxHealth; // ✅ ADICIONADO
}