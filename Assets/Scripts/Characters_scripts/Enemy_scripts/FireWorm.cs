using UnityEngine;

public class FireWorm : Enemy
{
    [Header("FireWorm Settings")]
    [SerializeField] private float stalkSpeed = 3f; // Velocidade perseguindo
    [SerializeField] private float chaseRange = 5f; // Distância para começar a acelerar
    [SerializeField] private float chaseSpeed = 5f; // Velocidade acelerada (chase)
    [SerializeField] private float attackRangeDistance = 2f; // Distância para atacar (bem perto)
    [SerializeField] private float horizontalAlignmentTolerance = 0.5f; // Margem para alinhamento horizontal
    [SerializeField] private int biteDamage = 20; // Dano da mordida
    [SerializeField] private float biteCooldown = 0.5f; // Tempo entre mordidas
    [SerializeField] private float attackAnimationDuration = 0.8f; // Duração da animação de ataque
    [SerializeField] private Collider2D biteHitbox; // ⚠️ Arraste aqui o Collider2D da boca (objeto filho)!
    [SerializeField] private float hitboxActivationDelay = 0.2f; // Delay antes de ativar a hitbox (espera a animação)
    [SerializeField] private float hitboxActiveDuration = 0.2f; // Quanto tempo a hitbox fica ativa
    
    private Animator animator;
    private bool isInChaseRange = false; // Player está perto (acelera)
    private bool isInAttackRange = false; // Player está MUITO perto (ataca)
    private float lastBiteTime = -999f;
    private bool isAttacking = false; // Controla se está executando ataque
    private bool hasDealtDamageThisAttack = false; // Evita dano múltiplo no mesmo ataque
    
    protected override void Start()
    {
        // 🔥 Stats do FireWorm
        damage = biteDamage;
        moveSpeed = stalkSpeed;
        attackCooldown = biteCooldown;
        
        base.Start();
        
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("⚠️ FireWorm não tem Animator component!");
        }
        
        // Desativa hitbox no início
        if (biteHitbox != null)
        {
            biteHitbox.enabled = false;
            Debug.Log($"✅ FireWorm - Hitbox configurada: {biteHitbox.name}");
            
            // Verifica se é trigger
            if (!biteHitbox.isTrigger)
            {
                Debug.LogError("⚠️ BiteHitbox precisa ter 'Is Trigger' marcado!");
            }
        }
        else
        {
            Debug.LogError("⚠️ FireWorm - HITBOX NÃO CONFIGURADA! Arraste o Collider2D no Inspector!");
        }
        
        // FireWorm precisa de EnemyHealth configurado com vida média
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.SetMaxHealth(50); // Vida MÉDIA
        }
        
        // ✅ Configura Rigidbody2D para não ser empurrado
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic; // Não é afetado por física
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Não roda
            Debug.Log("✅ FireWorm - Rigidbody2D configurado como Kinematic");
        }
    }
    
    protected override void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Flip do sprite
        HandleSpriteFlip();
        
        // Verifica ranges
        isInChaseRange = distanceToPlayer <= chaseRange;
        isInAttackRange = distanceToPlayer <= attackRangeDistance;
        
        // Verifica alinhamento horizontal (mesmo Y, com margem)
        float yDifference = Mathf.Abs(transform.position.y - player.position.y);
        bool isHorizontallyAligned = yDifference <= horizontalAlignmentTolerance;
        
        // Atualiza animação baseado na distância
        UpdateAnimation();
        
        // Persegue o jogador
        HandleChasing(distanceToPlayer);
        
        // ✅ Ataca APENAS se estiver perto E alinhado horizontalmente
        if (isInAttackRange && isHorizontallyAligned && Time.time >= lastBiteTime + biteCooldown && !isAttacking)
        {
            AttackPlayer();
        }
    }
    
    private void HandleChasing(float distanceToPlayer)
    {
        if (navAgent == null || !navAgent.isActiveAndEnabled) return;
        
        // 3 velocidades diferentes:
        // 1. Stalk (longe) - velocidade normal
        // 2. Chase (perto) - acelera mas não ataca ainda
        // 3. Attack (muito perto) - pode atacar
        float currentSpeed = stalkSpeed;
        
        if (isInChaseRange)
        {
            currentSpeed = chaseSpeed; // Acelera quando entra no chase range
        }
        
        navAgent.speed = currentSpeed;
        moveSpeed = currentSpeed;
        
        // Persegue o jogador
        navAgent.SetDestination(player.position);
    }
    
    private void UpdateAnimation()
    {
        if (animator == null) return;
        
        // ✅ PRIORIDADE: Ataque > Chase > Stalk
        if (isAttacking)
        {
            // Animação de ataque tem prioridade máxima
            // Força a reprodução para garantir que não seja interrompida
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("AttackFireWorm"))
            {
                animator.Play("AttackFireWorm", 0, 0f);
                // Debug.Log("🔥 FireWorm iniciando animação de ATAQUE!");
            }
        }
        else if (isInChaseRange)
        {
            // Chase range (perto, acelerado)
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("ChaseFireWorm"))
            {
                animator.Play("ChaseFireWorm");
            }
        }
        else
        {
            // Stalking (longe, velocidade normal)
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("StalkFireWorm"))
            {
                animator.Play("StalkFireWorm");
            }
        }
    }
    
    protected override void AttackPlayer()
    {
        lastBiteTime = Time.time;
        isAttacking = true; // Inicia animação de ataque
        hasDealtDamageThisAttack = false; // Reset flag de dano
        
        Debug.Log("🔥🔥 FireWorm ATACANDO! Hitbox será ativada após " + hitboxActivationDelay + "s");
        
        // Ativa a hitbox após o delay (quando a animação chegar na mordida)
        Invoke(nameof(ActivateBiteHitbox), hitboxActivationDelay);
        
        // Toca som de ataque (cria AudioSource temporário se necessário)
        if (attackSound != null)
        {
            if (audioSource != null)
            {
                audioSource.volume = 0.75f; // 50% do volume
                audioSource.PlayOneShot(attackSound);
            }
            else
            {
                // Se não tem AudioSource, cria um temporário
                GameObject soundObject = new GameObject("AttackSound");
                soundObject.transform.position = transform.position;
                AudioSource tempAudioSource = soundObject.AddComponent<AudioSource>();
                tempAudioSource.clip = attackSound;
                tempAudioSource.volume = 0.75f; // 50% do volume
                tempAudioSource.Play();
                Destroy(soundObject, attackSound.length);
            }
        }
        
        // Volta para animação de Stalk após a duração da animação
        Invoke(nameof(ResetAttackAnimation), attackAnimationDuration);
    }
    
    private void ActivateBiteHitbox()
    {
        if (biteHitbox != null)
        {
            biteHitbox.enabled = true;
            Debug.Log($"🔥 FireWorm - Hitbox de mordida ATIVADA! GameObject: {biteHitbox.gameObject.name}, Ativa: {biteHitbox.gameObject.activeInHierarchy}, Enabled: {biteHitbox.enabled}");
            
            // Verifica se tem o detector
            FireWormBiteDetector detector = biteHitbox.GetComponent<FireWormBiteDetector>();
            if (detector == null)
            {
                Debug.LogError("⚠️ BiteHitbox NÃO TEM FireWormBiteDetector!");
            }
            
            // Desativa após a duração
            Invoke(nameof(DeactivateBiteHitbox), hitboxActiveDuration);
        }
        else
        {
            Debug.LogError("⚠️ FireWorm - Não conseguiu ativar hitbox (null)!");
        }
    }
    
    private void DeactivateBiteHitbox()
    {
        if (biteHitbox != null)
        {
            biteHitbox.enabled = false;
            Debug.Log("🔥 FireWorm - Hitbox de mordida DESATIVADA!");
        }
    }
    
    /// <summary>
    /// Chamado quando a hitbox da mordida colide com o player
    /// </summary>
    public void OnBiteHit(Collider2D collision)
    {
        Debug.Log($"🔥 OnBiteHit chamado! Colisão com: {collision.name}, Tag: {collision.tag}, isAttacking: {isAttacking}, jáDeu dano: {hasDealtDamageThisAttack}");
        
        // Só dá dano se estiver atacando e ainda não deu dano neste ataque
        if (isAttacking && !hasDealtDamageThisAttack && collision.CompareTag("Player"))
        {
            PlayerController playerController = collision.GetComponent<PlayerController>();
            
            if (playerController != null)
            {
                playerController.TakeDamage(biteDamage);
                hasDealtDamageThisAttack = true; // Marca que já deu dano
                Debug.Log($"🔥✅ FireWorm mordeu o Player! Dano: {biteDamage}");
            }
            else
            {
                Debug.LogWarning("⚠️ Player não tem PlayerController!");
            }
        }
    }
    
    private void ResetAttackAnimation()
    {
        isAttacking = false;
        // Debug.Log("🔥 FireWorm voltando para Stalk/Chase (isAttacking = false)");
    }
    
    // ✅ SOBRESCREVE comportamento da classe base para evitar dano por colisão
    protected override void HandleCombat(float distanceToPlayer)
    {
        // FireWorm NÃO dá dano por colisão!
        // Apenas ataca quando executa AttackPlayer() manualmente
    }
    
    void OnDrawGizmosSelected()
    {
        // Desenha chase range (amarelo - acelera aqui)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // Desenha attack range (vermelho - ataca aqui)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRangeDistance);
    }
}