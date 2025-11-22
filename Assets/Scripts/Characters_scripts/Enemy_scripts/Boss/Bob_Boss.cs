using UnityEngine;
using System.Collections;

public class Bob_Boss : Enemy
{
    [Header("⚡ BOB - O IRMÃO ÁGIL ⚡")]
    [Tooltip("Bob é o irmão mais novo, rápido e focado apenas em ataques de dash!")]
    
    [Header("Movimento")]
    [SerializeField] private float normalMoveSpeed = 9f;
    [SerializeField] private float minDistanceFromPlayer = 8f; // Distância mínima para preparar dash
    [SerializeField] private float maxDistanceFromPlayer = 15f; // Distância máxima
    [SerializeField] private float positioningTolerance = 1f; // Tolerância no eixo X
    
    [Header("💨 Dash Attack")]
    [SerializeField] private float dashSpeed = 60f; // BEM RÁPIDO!
    [SerializeField] private float dashCooldown = 2.5f;
    [SerializeField] private float dashChargeTime = 0.8f; // Tempo preparando o dash
    [SerializeField] private int dashDamage = 20;
    [SerializeField] private int phase2DashDamage = 35; // Dano aumentado na fase 2
    [SerializeField] private TrailRenderer dashTrail;
    
    [Header("Fase 2 - Enfurecido (40% HP)")]
    [SerializeField] private float phase2DashCooldown = 1.5f; // Dasha mais frequente
    [SerializeField] private float phase2DashSpeed = 85f; // EXTREMAMENTE RÁPIDO!
    [SerializeField] private Color phase2Color = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private ParticleSystem rageParticles;
    
    [Header("📊 Visual")]
    [SerializeField] private SpriteRenderer bossSprite;
    [SerializeField] private Animator animator;
    
    // Estado
    private enum BobState
    {
        Positioning,    // Se posicionando no mesmo Y do player (mesma altura)
        Charging,       // Preparando o dash
        Dashing,        // Executando o dash
        Recovering      // Recuperando após o dash
    }
    
    private BobState currentState = BobState.Positioning;
    private int currentPhase = 1;
    private float lastDashTime = -999f;
    private float stateStartTime;
    
    private Vector2 dashDirection;
    private Vector2 targetPosition; // Posição alvo ao se posicionar
    private bool dashAnimationSet = false; // Evita trocar animação durante dash
    private Vector2 lastPosition; // Para detectar movimento
    private bool isCurrentlyMoving = false; // Estado de movimento atual
    private float movementCheckInterval = 0.1f; // Intervalo entre checks
    private float lastMovementCheck = 0f;
    private Vector2 lastFacingDirection = Vector2.right; // Última direção que estava olhando
    private bool isDead = false;
    
    private Vector2 initialPosition; // Posição inicial para retornar
    private float playerLostTime = 0f; // Tempo desde que perdeu o player
    private const float RETURN_TO_START_DELAY = 3f; // Tempo antes de voltar ao início
    
    private Rigidbody2D rb;
    private EnemyHealth bossHealth;
    
    // Charge indicator
    private Coroutine chargeFlashCoroutine;
    private Color originalColor;
    
    // ✅ Armazena a velocidade base da fase atual (sem debuffs)
    private float currentBaseMoveSpeed;
    
    // ❄️ Multiplier de dash (afetado por congelamento)
    private float dashSpeedMultiplier = 1f;
    
    protected override void Start()
    {
        base.Start();
        
        // ✅ FORÇA valores do boss, IGNORANDO completamente Enemy.moveSpeed!
        moveSpeed = normalMoveSpeed;
        currentBaseMoveSpeed = normalMoveSpeed;
        dashSpeedMultiplier = 1f; // ❄️ Garante que dash começa em 100%
        
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
        if (bossHealth != null)
        {
            bossHealth.OnDeath += HandleDeath; // Registra callback de morte
        }
        
        if (bossSprite == null)
        {
            bossSprite = GetComponentInChildren<SpriteRenderer>();
        }
        
        // Pega Animator se não foi atribuído
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Salva cor original
        if (bossSprite != null)
        {
            originalColor = bossSprite.color;
        }
        
        if (dashTrail != null)
        {
            dashTrail.emitting = false;
        }
        
        // Garante que partículas começam desativadas
        if (rageParticles != null)
        {
            rageParticles.Stop();
            rageParticles.gameObject.SetActive(false);
        }
        
        // Guarda a posição inicial
        initialPosition = transform.position;
        lastPosition = transform.position;
        
        ChangeState(BobState.Positioning);
        
        Debug.Log("⚡ BOB (O IRMÃO ÁGIL) ENTROU NA LUTA! ⚡");
        Debug.Log($"⚡ BOB - Velocidades configuradas:");
        Debug.Log($"  • normalMoveSpeed (SerializeField): {normalMoveSpeed}");
        Debug.Log($"  • dashSpeed (SerializeField): {dashSpeed}");
        Debug.Log($"  • moveSpeed (atual): {moveSpeed}");
        Debug.Log($"  • dashSpeedMultiplier: {dashSpeedMultiplier}x");
        Debug.Log($"  • navAgent.speed: {(navAgent != null ? navAgent.speed.ToString() : "N/A")}");
    }
    
    protected override void Update()
    {
        if (isDead) return;
        
        // Se o player morreu ou desapareceu, volta para a posição inicial
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            HandlePlayerLost();
            return;
        }
        
        // Player está vivo, reseta o timer
        playerLostTime = 0f;
        
        UpdatePhase();
        
        // Debug: Mostra vida a cada 2 segundos
        if (Time.frameCount % 120 == 0 && bossHealth != null)
        {
            // Debug.Log($"💚 BOB HP: {bossHealth.GetCurrentHealth()}/{bossHealth.GetMaxHealth()} ({bossHealth.GetHealthPercentage() * 100:F0}%)");
        }
        
        // Gerencia comportamento baseado no estado
        switch (currentState)
        {
            case BobState.Positioning:
                HandlePositioning();
                break;
            case BobState.Charging:
                HandleCharging();
                break;
            case BobState.Dashing:
                HandleDashing();
                break;
            case BobState.Recovering:
                HandleRecovering();
                break;
        }
    }
    
    protected override void HandleSpriteFlip()
    {
        // Desabilitado - o flip é controlado pelas funções de animação
        // para evitar conflitos entre sistemas
    }
    
    void ChangeState(BobState newState)
    {
        currentState = newState;
        stateStartTime = Time.time;
        dashAnimationSet = false; // Reseta flag ao mudar estado
        
        // Ações ao entrar no estado
        switch (newState)
        {
            case BobState.Charging:
                if (navAgent != null)
                    navAgent.enabled = false;
                if (rb != null)
                    rb.linearVelocity = Vector2.zero; // Para completamente
                // Inicia flash amarelo
                if (chargeFlashCoroutine != null)
                    StopCoroutine(chargeFlashCoroutine);
                chargeFlashCoroutine = StartCoroutine(ChargeFlashEffect());
                break;
                
            case BobState.Dashing:
                if (navAgent != null)
                    navAgent.enabled = false; // Desabilita navAgent durante dash
                if (dashTrail != null)
                    dashTrail.emitting = true;
                // Para o flash e restaura cor
                if (chargeFlashCoroutine != null)
                {
                    StopCoroutine(chargeFlashCoroutine);
                    chargeFlashCoroutine = null;
                }
                if (bossSprite != null)
                    bossSprite.color = currentPhase >= 2 ? phase2Color : originalColor;
                break;
                
            case BobState.Recovering:
                if (dashTrail != null)
                    dashTrail.emitting = false;
                if (rb != null)
                    rb.linearVelocity = Vector2.zero; // Para o movimento
                break;
                
            case BobState.Positioning:
                if (navAgent != null)
                    navAgent.enabled = true;
                break;
        }
    }
    
    // ========================================
    // POSICIONAMENTO
    // ========================================
    void HandlePositioning()
    {
        if (player == null) return;
        
        // Calcula posição alvo: mesmo Y do player, mas numa distância X segura (esquerda/direita)
        float playerX = player.position.x;
        float playerY = player.position.y;
        
        // Decide se fica à esquerda ou à direita do player
        float currentDistX = transform.position.x - playerX;
        float targetX;
        
        if (Mathf.Abs(currentDistX) < minDistanceFromPlayer)
        {
            // Está muito perto, se afasta
            targetX = playerX + (currentDistX > 0 ? maxDistanceFromPlayer : -maxDistanceFromPlayer);
        }
        else if (Mathf.Abs(currentDistX) > maxDistanceFromPlayer)
        {
            // Está muito longe, se aproxima
            targetX = playerX + (currentDistX > 0 ? minDistanceFromPlayer : -minDistanceFromPlayer);
        }
        else
        {
            targetX = transform.position.x;
        }
        
        targetPosition = new Vector2(targetX, playerY);
        
        // Move em direção à posição alvo
        Vector2 currentPos = transform.position;
        Vector2 direction = (targetPosition - currentPos).normalized;
        
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            // ✅ Usa moveSpeed atual (que pode estar com debuff)
            navAgent.speed = moveSpeed;
            navAgent.SetDestination(targetPosition);
        }
        else if (rb != null)
        {
            rb.MovePosition(currentPos + direction * moveSpeed * Time.deltaTime);
        }
        
        // Atualiza animações de movimento
        UpdateMovementAnimation(direction);
        
        // Verifica se está alinhado no eixo Y (mesma altura)
        float distX = Mathf.Abs(transform.position.x - playerX);
        float distY = Mathf.Abs(transform.position.y - playerY);
        
        if (distY <= positioningTolerance && 
            distX >= minDistanceFromPlayer && 
            distX <= maxDistanceFromPlayer)
        {
            // Está posicionado! Pode dashar
            float currentCooldown = currentPhase >= 2 ? phase2DashCooldown : dashCooldown;
            
            if (Time.time >= lastDashTime + currentCooldown)
            {
                ChangeState(BobState.Charging);
            }
        }
    }
    
    // ========================================
    // CARREGANDO DASH
    // ========================================
    void HandleCharging()
    {
        // Para no lugar enquanto carrega
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Calcula direção do dash APENAS HORIZONTAL (esquerda ou direita)
        float horizontalDirection = player.position.x > transform.position.x ? 1f : -1f;
        dashDirection = new Vector2(horizontalDirection, 0f);
        
        // Atualiza animação para olhar na direção do player
        UpdateMovementAnimation(dashDirection);
        
        // Após tempo de carga, dasha!
        if (Time.time >= stateStartTime + dashChargeTime)
        {
            ChangeState(BobState.Dashing);
            lastDashTime = Time.time;
        }
    }
    
    // ========================================
    // DASHANDO
    // ========================================
    void HandleDashing()
    {
        float baseDashSpeed = currentPhase >= 2 ? phase2DashSpeed : dashSpeed;
        
        // ❄️ Aplica multiplier de congelamento ao dash
        float currentDashSpeed = baseDashSpeed * dashSpeedMultiplier;
        
        // Usa velocity para movimento consistente e rápido
        if (rb != null)
        {
            rb.linearVelocity = dashDirection * currentDashSpeed;
        }
        else
        {
            transform.position += (Vector3)(dashDirection * currentDashSpeed * Time.deltaTime);
        }
        
        // Define animação de dash APENAS UMA VEZ no início
        if (!dashAnimationSet)
        {
            UpdateDashAnimation(dashDirection);
            dashAnimationSet = true;
        }
        
        // Verifica se saiu do mapa ou atingiu distância máxima
        float distanceTraveled = Vector2.Distance(transform.position, targetPosition);
        
        if (distanceTraveled > 30f) // Atravessou o mapa
        {
            ChangeState(BobState.Recovering);
        }
    }
    
    // ========================================
    // RECUPERANDO
    // ========================================
    void HandleRecovering()
    {
        // Breve pausa antes de voltar a se posicionar
        if (Time.time >= stateStartTime + 0.2f)
        {
            ChangeState(BobState.Positioning);
        }
    }
    
    // ========================================
    // ANIMAÇÕES
    // ========================================
    void UpdateMovementAnimation(Vector2 direction)
    {
        if (animator == null) return;
        
        // Salva a direção atual
        if (direction.magnitude > 0.1f)
            lastFacingDirection = direction;
        
        // Detecta se está se movendo apenas periodicamente para evitar flickering
        if (Time.time - lastMovementCheck >= movementCheckInterval)
        {
            Vector2 currentPosition = transform.position;
            float distanceMoved = Vector2.Distance(currentPosition, lastPosition);
            
            // Threshold maior para considerar movimento (0.05 unidades em 0.1s)
            isCurrentlyMoving = distanceMoved > 0.05f;
            
            lastPosition = currentPosition;
            lastMovementCheck = Time.time;
        }
        
        // Escolhe prefixo baseado em movimento
        string animPrefix = isCurrentlyMoving ? "Walk" : "Idle";
        
        // Normaliza direção para achar qual é a predominante
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Movimento horizontal predominante
            animator.Play(animPrefix + "Side");
            
            // Inverte sprite baseado na direção
            if (direction.x > 0)
            {
                // Olhando para DIREITA - sprite normal
                transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                // Olhando para ESQUERDA - sprite invertido
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        else
        {
            // Movimento vertical predominante
            if (direction.y > 0)
            {
                // Olhando para CIMA
                animator.Play(animPrefix + "Up");
            }
            else
            {
                // Olhando para BAIXO
                animator.Play(animPrefix + "Down");
            }
        }
    }
    
    void UpdateDashAnimation(Vector2 direction)
    {
        if (animator == null) return;
        
        // Sempre usa animação lateral (já que só faz dash horizontal)
        animator.Play("DashSide");
        
        // Inverte sprite baseado na direção
        if (direction.x > 0)
        {
            // Dashando para DIREITA - sprite normal
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            // Dashando para ESQUERDA - sprite invertido
            transform.localScale = new Vector3(-1, 1, 1);
        }
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
        
        // ✅ Atualiza velocidade base para fase 2 (mas NÃO sobrescreve se tiver debuff)
        // Nota: Bob não muda velocidade de movimento na fase 2, só velocidade de dash
        // Mantemos a normalMoveSpeed como base
        currentBaseMoveSpeed = normalMoveSpeed;
        
        // Se não estiver com debuff, aplica velocidade normal
        // (Se estiver com debuff, o EnemyStatusEffects já modificou moveSpeed)
        if (moveSpeed == normalMoveSpeed)
        {
            moveSpeed = normalMoveSpeed;
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
        
        Debug.Log("⚡💥 BOB ENFURECEU! DASHES MAIS RÁPIDOS! 💥⚡");
    }
    
    // ========================================
    // MORTE
    // ========================================
    void HandleDeath()
    {
        if (isDead) return;
        
        isDead = true;
        
        // Para todo movimento
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        
        if (navAgent != null)
            navAgent.enabled = false;
        
        // Para efeitos visuais
        if (dashTrail != null)
            dashTrail.emitting = false;
        
        if (chargeFlashCoroutine != null)
            StopCoroutine(chargeFlashCoroutine);
        
        // Toca animação de morte baseada na última direção
        PlayDeathAnimation();
        
        // Debug.Log("💀⚡ BOB FOI DERROTADO! ⚡💀");
    }
    
    void PlayDeathAnimation()
    {
        if (animator == null) return;
        
        // Determina qual animação de morte usar baseado na última direção
        if (Mathf.Abs(lastFacingDirection.x) > Mathf.Abs(lastFacingDirection.y))
        {
            // Estava olhando horizontalmente
            animator.Play("DieSide");
            
            // Mantém o flip correto
            if (lastFacingDirection.x > 0)
                transform.localScale = new Vector3(1, 1, 1);
            else
                transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            // Estava olhando verticalmente
            if (lastFacingDirection.y > 0)
                animator.Play("DieUp");
            else
                animator.Play("DieDown");
        }
    }
    
    void OnDestroy()
    {
        // Remove callback ao destruir
        if (bossHealth != null)
            bossHealth.OnDeath -= HandleDeath;
    }
    
    // ========================================
    // RETORNO À POSIÇÃO INICIAL
    // ========================================
    void HandlePlayerLost()
    {
        playerLostTime += Time.deltaTime;
        
        // Para o trail durante o retorno
        if (dashTrail != null)
            dashTrail.emitting = false;
        
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
            
            UpdateMovementAnimation(Vector2.right);
            return;
        }
        
        // Move de volta para a posição inicial
        Vector2 direction = (initialPosition - (Vector2)transform.position).normalized;
        
        if (rb != null)
            rb.linearVelocity = direction * (moveSpeed * 0.5f); // Volta em velocidade reduzida
        
        UpdateMovementAnimation(direction);
    }
    
    // ========================================
    // COLISÃO
    // ========================================
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"🔍 BOB colidiu com: {collision.gameObject.name}, Tag: {collision.gameObject.tag}, Estado: {currentState}");
        
        if (currentState == BobState.Dashing && collision.gameObject.CompareTag("Player"))
        {
            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null)
            {
                int currentDamage = currentPhase >= 2 ? phase2DashDamage : dashDamage;
                pc.TakeDamage(currentDamage);
                Debug.Log($"💥⚡ BOB ACERTOU UM DASH NO PLAYER! ({currentDamage} dano) ⚡💥");
            }
        }
    }
    
    // Adiciona trigger para detectar bullets e player
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"🔍 BOB trigger com: {collision.gameObject.name}, Tag: {collision.gameObject.tag}, Estado: {currentState}");
        
        // Dano durante dash
        if (currentState == BobState.Dashing && collision.CompareTag("Player"))
        {
            PlayerController pc = collision.GetComponent<PlayerController>();
            if (pc != null)
            {
                int currentDamage = currentPhase >= 2 ? phase2DashDamage : dashDamage;
                pc.TakeDamage(currentDamage);
                Debug.Log($"💥⚡ BOB ACERTOU UM DASH NO PLAYER (TRIGGER)! ({currentDamage} dano) ⚡💥");
            }
        }
        
        // Detecta bullets
        if (collision.GetComponent<Bullet>() != null)
        {
            Debug.Log("🎯 BOB FOI ATINGIDO POR BULLET!");
        }
    }
    
    // ========================================
    // EFEITO VISUAL DE CHARGE
    // ========================================
    IEnumerator ChargeFlashEffect()
    {
        Color yellow = Color.yellow;
        Color baseColor = currentPhase >= 2 ? phase2Color : originalColor;
        
        float flashSpeed = 8f; // Velocidade do piscar
        
        while (true)
        {
            if (bossSprite != null)
            {
                // Alterna entre amarelo e cor base
                float t = Mathf.PingPong(Time.time * flashSpeed, 1f);
                bossSprite.color = Color.Lerp(baseColor, yellow, t);
            }
            yield return null;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        
        // Linha mostrando alinhamento Y (horizontal)
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            new Vector3(transform.position.x - 10, player.position.y, 0),
            new Vector3(transform.position.x + 10, player.position.y, 0)
        );
        
        // Distâncias min/max
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, maxDistanceFromPlayer);
        
        // Posição alvo
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(targetPosition, 0.5f);
    }
    
    // ========================================
    // CORREÇÃO DE VELOCIDADE
    // ========================================
    
    /// <summary>
    /// Garante que a velocidade está correta (previne conflitos com classe base)
    /// </summary>
    void EnsureCorrectSpeed()
    {
        // Bob sempre usa normalMoveSpeed (não muda entre fases)
        float expectedSpeed = normalMoveSpeed;
        
        // Se moveSpeed está muito diferente da velocidade esperada, corrige
        if (moveSpeed > expectedSpeed * 1.1f)
        {
            Debug.LogWarning($"⚠️ BOB: Velocidade incorreta detectada ({moveSpeed}), corrigindo para {expectedSpeed}");
            moveSpeed = expectedSpeed;
            
            if (navAgent != null && navAgent.isActiveAndEnabled)
            {
                navAgent.speed = expectedSpeed;
            }
        }
    }
    
    // ========================================
    // MÉTODOS PÚBLICOS
    // ========================================
    
    /// <summary>
    /// Retorna a velocidade base atual (sem debuffs) para que EnemyStatusEffects possa restaurar corretamente
    /// Bob não muda velocidade de movimento entre fases, só velocidade de dash
    /// </summary>
    public override float GetBaseMoveSpeed()
    {
        return normalMoveSpeed;
    }
    
    /// <summary>
    /// Define o multiplier de velocidade de dash (usado por debuffs como congelamento)
    /// </summary>
    public void SetDashSpeedMultiplier(float multiplier)
    {
        dashSpeedMultiplier = Mathf.Clamp(multiplier, 0.1f, 2f); // Limita entre 10% e 200%
        Debug.Log($"⚡ BOB - Dash speed multiplier ajustado para {dashSpeedMultiplier:F2}x");
    }
    
    /// <summary>
    /// Retorna o multiplier atual de dash speed
    /// </summary>
    public float GetDashSpeedMultiplier()
    {
        return dashSpeedMultiplier;
    }
}
