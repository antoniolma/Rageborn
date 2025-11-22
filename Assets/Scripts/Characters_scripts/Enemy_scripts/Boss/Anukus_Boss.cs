using UnityEngine;
using System.Collections;

public class Anukus_Boss : Enemy
{
    [Header("⚔️ ANUKUS - O IRMÃO FORTE ⚔️")]
    [Tooltip("Anukus é o irmão do meio, forte e brutal com ataques de espada corpo a corpo!")]
    
    [Header("Movimento")]
    [SerializeField] private float normalMoveSpeed = 5f;
    [SerializeField] private float meleeAttackRange = 2.5f; // Distância para começar ataque
    [SerializeField] private float backstepDistance = 1.5f; // Distância que recua antes de atacar
    
    [Header("⚔️ Sword Attack")]
    [SerializeField] private float attackCooldownTime = 3f;
    [SerializeField] private float backstepDuration = 0.15f; // Tempo recuando (mais rápido)
    [SerializeField] private float chargeFlashDuration = 0.1f; // Tempo piscando antes de atacar (muito mais rápido!)
    [SerializeField] private int swordDamage = 25;
    [SerializeField] private float attackHitboxDuration = 0.3f; // Janela de tempo que o ataque causa dano
    [SerializeField] private Collider2D attackHitbox; // ⚠️ IMPORTANTE: Arraste aqui o Collider2D da espada (objeto filho)!
    [SerializeField] private float swordHitboxDistance = 1.5f; // Distância da hitbox em relação ao centro do boss (aumentado)
    [SerializeField] private float hitboxActivationDelay = 0.15f; // Delay antes de ativar a hitbox (espera a animação)
    
    [Header("Fase 2 - Enfurecido (40% HP)")]
    [SerializeField] private float phase2MoveSpeed = 7f;
    [SerializeField] private float phase2AttackCooldown = 2f;
    [SerializeField] private int phase2SwordDamage = 35;
    [SerializeField] private Color phase2Color = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private ParticleSystem rageParticles;
    [SerializeField] private bool phase2DoubleAttack = true; // Na fase 2 ataca duas vezes
    
    [Header("📊 Visual")]
    [SerializeField] private SpriteRenderer bossSprite;
    [SerializeField] private Animator animator;
    [SerializeField] private float deathAnimationDuration = 1.5f;
    
    // Estado
    private enum AnukusState
    {
        Chasing,        // Perseguindo o player
        Backstep,       // Recuando antes de atacar
        Charging,       // Piscando antes de atacar
        Attacking,      // Executando o ataque
        Cooldown        // Esperando antes de atacar novamente
    }
    
    private AnukusState currentState = AnukusState.Chasing;
    private int currentPhase = 1;
    private float stateStartTime;
    // lastAttackTime já existe na classe base Enemy como protected
    
    private Vector2 attackDirection;
    private Vector2 backstepStartPosition;
    private Vector2 lastPosition;
    private Vector2 lastFacingDirection = Vector2.down;
    private bool isCurrentlyMoving = false;
    private float movementCheckInterval = 0.1f;
    private float lastMovementCheck = 0f;
    private bool isDead = false;
    private bool hasDealtDamageThisAttack = false;
    private int attackCount = 0; // Para fase 2 double attack
    
    private Vector2 initialPosition; // Posição inicial para retornar
    private float playerLostTime = 0f; // Tempo desde que perdeu o player
    private const float RETURN_TO_START_DELAY = 3f; // Tempo antes de voltar ao início
    
    private Rigidbody2D rb;
    private EnemyHealth bossHealth;
    private Coroutine chargeFlashCoroutine;
    private Color originalColor;
    
    // ✅ Armazena a velocidade base da fase atual (sem debuffs)
    private float currentBaseMoveSpeed;
    
    protected override void Start()
    {
        base.Start();
        
        // ✅ FORÇA valores do boss, IGNORANDO completamente Enemy.moveSpeed!
        moveSpeed = normalMoveSpeed;
        currentBaseMoveSpeed = normalMoveSpeed;
        
        // ✅ SOBRESCREVE NavMeshAgent também
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.speed = normalMoveSpeed;
        }
        
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        bossHealth = GetComponent<EnemyHealth>();
        if (bossHealth == null)
        {
            bossHealth = gameObject.AddComponent<EnemyHealth>();
        }
        
        if (bossSprite == null)
        {
            bossSprite = GetComponentInChildren<SpriteRenderer>();
        }
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (bossSprite != null)
        {
            originalColor = bossSprite.color;
        }
        
        if (rageParticles != null)
        {
            rageParticles.Stop();
            rageParticles.gameObject.SetActive(false);
        }
        
        // Desativa a hitbox no início
        if (attackHitbox != null)
        {
            attackHitbox.enabled = false;
        }
        
        // Guarda a posição inicial
        initialPosition = transform.position;
        lastPosition = transform.position;
        
        ChangeState(AnukusState.Chasing);
        
        Debug.Log("⚜️ ANUKUS (O IRMÃO FORTE) ENTROU NA LUTA! ⚜️");
        Debug.Log($"⚜️ ANUKUS - Velocidades configuradas:");
        Debug.Log($"  • normalMoveSpeed (SerializeField): {normalMoveSpeed}");
        Debug.Log($"  • phase2MoveSpeed (SerializeField): {phase2MoveSpeed}");
        Debug.Log($"  • moveSpeed (atual): {moveSpeed}");
        Debug.Log($"  • navAgent.speed: {(navAgent != null ? navAgent.speed.ToString() : "N/A")}");
    }
    
    protected override void Update()
    {
        if (isDead) return;
        
        // ✅ GARANTE que moveSpeed nunca fica diferente do esperado
        // (previne bugs de sincronização com a classe base)
        EnsureCorrectSpeed();
        
        // Se o player morreu ou desapareceu, volta para a posição inicial
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            HandlePlayerLost();
            return;
        }
        
        // Player está vivo, reseta o timer
        playerLostTime = 0f;
        
        // Verifica se a vida chegou a zero ANTES do EnemyHealth processar
        if (bossHealth != null && bossHealth.GetCurrentHealth() <= 0 && !isDead)
        {
            HandleDeath();
            return;
        }
        
        UpdatePhase();
        
        // Gerencia comportamento baseado no estado
        switch (currentState)
        {
            case AnukusState.Chasing:
                HandleChasing();
                break;
            case AnukusState.Backstep:
                HandleBackstep();
                break;
            case AnukusState.Charging:
                HandleCharging();
                break;
            case AnukusState.Attacking:
                HandleAttacking();
                break;
            case AnukusState.Cooldown:
                HandleCooldown();
                break;
        }
    }
    
    protected override void HandleSpriteFlip()
    {
        // Desabilitado - o flip é controlado pelas funções de animação
    }
    
    void ChangeState(AnukusState newState)
    {
        currentState = newState;
        stateStartTime = Time.time;
        
        switch (newState)
        {
            case AnukusState.Chasing:
                if (navAgent != null)
                    navAgent.enabled = true;
                break;
                
            case AnukusState.Backstep:
                if (navAgent != null)
                    navAgent.enabled = false;
                backstepStartPosition = transform.position;
                // Calcula direção do ataque (para o player)
                attackDirection = (player.position - transform.position).normalized;
                break;
                
            case AnukusState.Charging:
                if (rb != null)
                    rb.linearVelocity = Vector2.zero;
                // Inicia flash de aviso
                if (chargeFlashCoroutine != null)
                    StopCoroutine(chargeFlashCoroutine);
                chargeFlashCoroutine = StartCoroutine(ChargeFlashEffect());
                break;
                
            case AnukusState.Attacking:
                hasDealtDamageThisAttack = false;
                // Para o flash
                if (chargeFlashCoroutine != null)
                {
                    StopCoroutine(chargeFlashCoroutine);
                    chargeFlashCoroutine = null;
                }
                if (bossSprite != null)
                    bossSprite.color = currentPhase >= 2 ? phase2Color : originalColor;
                break;
                
            case AnukusState.Cooldown:
                lastAttackTime = Time.time;
                break;
        }
    }
    
    // ========================================
    // PERSEGUINDO
    // ========================================
    void HandleChasing()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // ✅ NÃO sobrescreve moveSpeed - respeita debuffs aplicados!
        // moveSpeed já foi modificado por EnemyStatusEffects se houver debuff
        
        // Move em direção ao player
        Vector2 direction = (player.position - transform.position).normalized;
        
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            // ✅ Usa moveSpeed atual (que pode estar com debuff)
            navAgent.speed = moveSpeed;
            navAgent.SetDestination(player.position);
        }
        else if (rb != null)
        {
            rb.MovePosition(rb.position + direction * moveSpeed * Time.deltaTime);
        }
        
        // Atualiza animação
        UpdateMovementAnimation(direction);
        
        // Verifica se está em posição de ataque (alinhado horizontal ou verticalmente)
        float currentCooldown = currentPhase >= 2 ? phase2AttackCooldown : attackCooldownTime;
        
        if (Time.time >= lastAttackTime + currentCooldown)
        {
            float distX = Mathf.Abs(transform.position.x - player.position.x);
            float distY = Mathf.Abs(transform.position.y - player.position.y);
            
            // Ataca se estiver alinhado horizontalmente (mesmo Y, perto em X)
            bool alignedHorizontally = distY <= meleeAttackRange * 0.5f && distX <= meleeAttackRange;
            
            // Ataca se estiver alinhado verticalmente (mesmo X, perto em Y)
            bool alignedVertically = distX <= meleeAttackRange * 0.5f && distY <= meleeAttackRange;
            
            if (alignedHorizontally || alignedVertically)
            {
                ChangeState(AnukusState.Backstep);
            }
        }
    }
    
    // ========================================
    // RECUANDO (BACKSTEP)
    // ========================================
    void HandleBackstep()
    {
        float elapsed = Time.time - stateStartTime;
        
        // Recua rapidamente na direção oposta ao player
        Vector2 backstepDirection = -attackDirection;
        float backstepProgress = elapsed / backstepDuration;
        
        if (backstepProgress < 1f)
        {
            // Ainda recuando
            Vector2 targetPos = backstepStartPosition + backstepDirection * backstepDistance;
            transform.position = Vector2.Lerp(backstepStartPosition, targetPos, backstepProgress);
            
            // Mantém animação idle olhando para o player
            UpdateMovementAnimation(attackDirection);
        }
        else
        {
            // Terminou de recuar, começa a carregar
            ChangeState(AnukusState.Charging);
        }
    }
    
    // ========================================
    // CARREGANDO ATAQUE (PISCANDO)
    // ========================================
    void HandleCharging()
    {
        // Para no lugar enquanto carrega
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        
        // Mantém olhando para o player
        UpdateMovementAnimation(attackDirection);
        
        // Após tempo de carga, ataca!
        if (Time.time >= stateStartTime + chargeFlashDuration)
        {
            ChangeState(AnukusState.Attacking);
        }
    }
    
    // ========================================
    // ATACANDO
    // ========================================
    void HandleAttacking()
    {
        float elapsed = Time.time - stateStartTime;
        
        // Toca animação de ataque no início de CADA golpe (importante para fase 2)
        if (elapsed < 0.05f)
        {
            PlayAttackAnimation(attackDirection);
            
            // ⚔️ POSICIONA A HITBOX IMEDIATAMENTE (mas ainda desativada)
            if (attackHitbox != null)
            {
                PositionSwordHitbox(attackDirection);
                attackHitbox.enabled = false; // Garante que está desativada
            }
            
            Debug.Log($"⚔️ Anukus iniciou golpe #{attackCount + 1}");
        }
        
        // ⚔️ ATIVA A HITBOX DA ESPADA após o delay (espera a animação começar)
        if (elapsed >= hitboxActivationDelay && elapsed < hitboxActivationDelay + 0.05f)
        {
            if (attackHitbox != null)
            {
                attackHitbox.enabled = true;
                Debug.Log($"⚔️ ANUKUS ATIVOU A HITBOX DA ESPADA (Golpe #{attackCount + 1})");
            }
        }
        
        // Terminou a janela de ataque
        if (elapsed >= attackHitboxDuration)
        {
            // Desativa a hitbox
            if (attackHitbox != null)
            {
                attackHitbox.enabled = false;
            }
            
            attackCount++;
            hasDealtDamageThisAttack = false; // Reseta para o próximo golpe
            
            // Fase 2: ataca duas vezes
            if (currentPhase >= 2 && phase2DoubleAttack && attackCount < 2)
            {
                // Reinicia o ataque para segundo golpe IMEDIATAMENTE
                stateStartTime = Time.time;
                Debug.Log($"💥 FASE 2: Preparando segundo golpe!");
            }
            else
            {
                // Terminou todos os ataques
                attackCount = 0;
                ChangeState(AnukusState.Cooldown);
            }
        }
    }
    
    // ========================================
    // DETECÇÃO DE DANO (usando o collider filho da espada)
    // ========================================
    
    /// <summary>
    /// Posiciona a hitbox da espada na direção correta antes de ativar
    /// </summary>
    void PositionSwordHitbox(Vector2 direction)
    {
        if (attackHitbox == null) return;
        
        Transform hitboxTransform = attackHitbox.transform;
        
        // Força direção pura (só X ou só Y)
        Vector2 pureDirection;
        float angle;
        
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal
            if (direction.x > 0)
            {
                pureDirection = new Vector2(swordHitboxDistance, 0);
                angle = 0; // Direita
            }
            else
            {
                pureDirection = new Vector2(-swordHitboxDistance, 0);
                angle = 180; // GAMBIARRA: Mantém 0° mas posição negativa
            }
        }
        else
        {
            // Vertical
            if (direction.y > 0)
            {
                pureDirection = new Vector2(0, swordHitboxDistance);
                angle = 90; // Cima
            }
            else
            {
                pureDirection = new Vector2(0, -swordHitboxDistance);
                angle = -90; // Baixo
            }
        }
        
        // Posiciona e rotaciona
        hitboxTransform.localPosition = pureDirection;
        hitboxTransform.localRotation = Quaternion.Euler(0, 0, angle);
        
        // Isso inverte todo o sistema de coordenadas local!
        if (direction.x < 0 && Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // O pai está com scale (-1, 1, 1), então:
            // - Posição negativa vira positiva
            // - Precisamos compensar indo MAIS pra direita (em coordenadas locais = esquerda em world)
            hitboxTransform.localRotation = Quaternion.Euler(0, 0, 0); // SEM rotação (o flip do pai já inverte)
            hitboxTransform.localPosition = new Vector3(swordHitboxDistance, 0, 0); // POSITIVO em local = esquerda em world
            Debug.Log($"🎭 GAMBIARRA ESQUERDA! localPos: {hitboxTransform.localPosition} (será invertido pelo flip do pai)");
        }
        
        Debug.Log($"🗡️ Hitbox posicionada em: {hitboxTransform.localPosition} | Ângulo: {hitboxTransform.localRotation.eulerAngles.z}° | Direção: {direction} | ParentScale: {transform.localScale.x}");
    }
    
    public void OnSwordHit(Collider2D collision)
    {
        // Método público chamado pelo AnukusSwordHitbox quando a espada colide
        if (currentState == AnukusState.Attacking && !hasDealtDamageThisAttack && collision.CompareTag("Player"))
        {
            PlayerController pc = collision.GetComponent<PlayerController>();
            if (pc != null)
            {
                int currentDamage = currentPhase >= 2 ? phase2SwordDamage : swordDamage;
                pc.TakeDamage(currentDamage);
                hasDealtDamageThisAttack = true;
                Debug.Log($"⚔️💥 ANUKUS ACERTOU UM GOLPE! ({currentDamage} dano) Tag: {collision.tag} 💥⚔️");
            }
        }
    }
    
    // ========================================
    // COOLDOWN
    // ========================================
    void HandleCooldown()
    {
        // Breve pausa antes de voltar a perseguir
        if (Time.time >= stateStartTime + 0.3f)
        {
            ChangeState(AnukusState.Chasing);
        }
    }
    
    // ========================================
    // RETORNO À POSIÇÃO INICIAL
    // ========================================
    void HandlePlayerLost()
    {
        playerLostTime += Time.deltaTime;
        
        // Após alguns segundos sem encontrar o player, volta ao início
        if (playerLostTime >= RETURN_TO_START_DELAY)
        {
            ReturnToInitialPosition();
        }
        else
        {
            // Para no lugar enquanto espera
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            
            UpdateMovementAnimation(lastFacingDirection);
        }
    }
    
    void ReturnToInitialPosition()
    {
        float distanceToStart = Vector2.Distance(transform.position, initialPosition);
        
        // Já chegou na posição inicial
        if (distanceToStart < 0.1f)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
            
            UpdateMovementAnimation(Vector2.down);
            return;
        }
        
        // Move de volta para a posição inicial
        Vector2 direction = (initialPosition - (Vector2)transform.position).normalized;
        
        if (rb != null)
            rb.linearVelocity = direction * (moveSpeed * 0.5f); // Volta em velocidade reduzida
        
        UpdateMovementAnimation(direction);
    }
    
    // ========================================
    // ANIMAÇÕES
    // ========================================
    void UpdateMovementAnimation(Vector2 direction)
    {
        if (animator == null || !animator.isActiveAndEnabled) return;
        
        // Salva a direção atual
        if (direction.magnitude > 0.1f)
            lastFacingDirection = direction;
        
        // Detecta movimento
        if (Time.time - lastMovementCheck >= movementCheckInterval)
        {
            Vector2 currentPosition = transform.position;
            float distanceMoved = Vector2.Distance(currentPosition, lastPosition);
            isCurrentlyMoving = distanceMoved > 0.05f;
            lastPosition = currentPosition;
            lastMovementCheck = Time.time;
        }
        
        try
        {
            string animPrefix = isCurrentlyMoving ? "Walk" : "Idle";
            
            // Determina direção
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                animator.Play(animPrefix + "Side");
                transform.localScale = direction.x > 0 ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
            }
            else
            {
                if (direction.y > 0)
                    animator.Play(animPrefix + "Up");
                else
                    animator.Play(animPrefix + "Down");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Erro ao atualizar animação de movimento: {e.Message}");
        }
    }
    
    void PlayAttackAnimation(Vector2 direction)
    {
        if (animator == null || !animator.isActiveAndEnabled) return;
        
        // Proteção extra contra Animator Controller corrompido
        try
        {
            // Determina direção do ataque
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Tenta tocar a animação, mas não gera erro se não existir
                if (HasAnimationState("AttackSide"))
                    animator.Play("AttackSide");
                
                transform.localScale = direction.x > 0 ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
            }
            else
            {
                if (direction.y > 0)
                {
                    if (HasAnimationState("AttackUp"))
                        animator.Play("AttackUp");
                }
                else
                {
                    if (HasAnimationState("AttackDown"))
                        animator.Play("AttackDown");
                    else
                        animator.Play("IdleDown"); // Fallback
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Erro ao tocar animação de ataque: {e.Message}");
        }
    }
    
    bool HasAnimationState(string stateName)
    {
        if (animator == null) return false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            // Se encontrar qualquer parâmetro, significa que o animator está configurado
        }
        
        // Tenta tocar e captura exceção silenciosamente
        return true; // Por enquanto assume que existe
    }
    
    // ========================================
    // FASES
    // ========================================
    void UpdatePhase()
    {
        if (bossHealth == null) return;
        
        float healthPercent = bossHealth.GetHealthPercentage();
        
        if (healthPercent <= 0.4f && currentPhase < 2)
        {
            EnterPhase2();
        }
    }
    
    void EnterPhase2()
    {
        currentPhase = 2;
        
        // ✅ Atualiza velocidade base para fase 2
        currentBaseMoveSpeed = phase2MoveSpeed;
        moveSpeed = phase2MoveSpeed;
        
        // Atualiza NavMeshAgent se existir
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.speed = phase2MoveSpeed;
        }
        
        if (bossSprite != null)
        {
            bossSprite.color = phase2Color;
        }
        
        if (rageParticles != null)
        {
            rageParticles.gameObject.SetActive(true);
            rageParticles.Play();
        }
        
        Debug.Log("⚔️💥 ANUKUS ENFURECEU! GOLPES DUPLOS! 💥⚔️");
    }
    
    // ========================================
    // MORTE
    // ========================================
    void HandleDeath()
    {
        if (isDead) return;
        
        isDead = true;
        
        Debug.Log("💀⚔️ ANUKUS MORREU - INICIANDO ANIMAÇÃO DE MORTE! ⚔️💀");
        
        // Bloqueia EnemyHealth
        if (bossHealth != null)
            bossHealth.enabled = false;
        
        CancelInvoke();
        StopAllCoroutines();
        
        // Desabilita colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
            col.enabled = false;
        
        // Para movimento
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }
        
        if (navAgent != null)
            navAgent.enabled = false;
        
        if (rageParticles != null)
            rageParticles.Stop();
        
        if (chargeFlashCoroutine != null)
            StopCoroutine(chargeFlashCoroutine);
        
        if (bossSprite != null)
            bossSprite.color = originalColor;
        
        PlayDeathAnimation();
        
        StartCoroutine(DestroyAfterDeathAnimation());
    }
    
    IEnumerator DestroyAfterDeathAnimation()
    {
        Debug.Log($"⏰ Aguardando {deathAnimationDuration} segundos para animação de morte...");
        yield return new WaitForSeconds(deathAnimationDuration);
        Debug.Log("💥 DESTRUINDO ANUKUS AGORA!");
        Destroy(gameObject);
    }
    
    void PlayDeathAnimation()
    {
        if (animator == null)
        {
            Debug.LogError("❌ ANIMATOR É NULL!");
            return;
        }
        
        if (Mathf.Abs(lastFacingDirection.x) > Mathf.Abs(lastFacingDirection.y))
        {
            animator.Play("DieSide");
            transform.localScale = lastFacingDirection.x > 0 ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
        }
        else
        {
            if (lastFacingDirection.y > 0)
                animator.Play("DieUp");
            else
                animator.Play("DieDown");
        }
    }
    
    // ========================================
    // EFEITO VISUAL DE CHARGE
    // ========================================
    IEnumerator ChargeFlashEffect()
    {
        Color yellow = Color.yellow;
        Color baseColor = currentPhase >= 2 ? phase2Color : originalColor;
        float flashSpeed = 20f; // Flash muito mais rápido
        
        while (true)
        {
            if (bossSprite != null)
            {
                float t = Mathf.PingPong(Time.time * flashSpeed, 1f);
                bossSprite.color = Color.Lerp(baseColor, yellow, t);
            }
            yield return null;
        }
    }
    
    void OnDrawGizmos()
    {
        if (player == null) return;
        
        // Range de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
        
        // Direção do ataque
        if (currentState == AnukusState.Attacking || currentState == AnukusState.Charging)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(attackDirection * meleeAttackRange));
        }
    }
    
    // ========================================
    // CORREÇÃO DE VELOCIDADE
    // ========================================
    
    /// <summary>
    /// Garante que a velocidade está correta (previne conflitos com classe base)
    /// </summary>
    void EnsureCorrectSpeed()
    {
        float expectedBaseSpeed = currentPhase >= 2 ? phase2MoveSpeed : normalMoveSpeed;
        
        // Se moveSpeed está muito diferente da velocidade esperada, corrige
        // (permite pequenas variações por debuffs, mas detecta resets indevidos)
        if (moveSpeed > expectedBaseSpeed * 1.1f)
        {
            Debug.LogWarning($"⚠️ ANUKUS: Velocidade incorreta detectada ({moveSpeed}), corrigindo para {expectedBaseSpeed}");
            moveSpeed = expectedBaseSpeed;
            
            if (navAgent != null && navAgent.isActiveAndEnabled)
            {
                navAgent.speed = expectedBaseSpeed;
            }
        }
    }
    
    // ========================================
    // MÉTODOS PÚBLICOS
    // ========================================
    
    /// <summary>
    /// Retorna a velocidade base atual (sem debuffs) para que EnemyStatusEffects possa restaurar corretamente
    /// </summary>
    public override float GetBaseMoveSpeed()
    {
        return currentPhase >= 2 ? phase2MoveSpeed : normalMoveSpeed;
    }
}
