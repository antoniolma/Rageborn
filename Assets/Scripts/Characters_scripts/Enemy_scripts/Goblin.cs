using UnityEngine;
using System.Collections;

public class Goblin : Enemy
{
    [Header("👹 Goblin Settings")]
    [SerializeField] private float normalMoveSpeed = 4.5f;
    [SerializeField] private float minDistanceFromPlayer = 4f; // Distância mínima para preparar dash
    [SerializeField] private float maxDistanceFromPlayer = 8f; // Distância máxima
    [SerializeField] private float positioningTolerance = 1f; // Tolerância no eixo Y
    
    [Header("💨 Dash Attack")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashCooldown = 2.5f;
    [SerializeField] private float dashChargeTime = 0.6f; // Tempo preparando o dash
    [SerializeField] private float dashDuration = 0.5f; // Duração máxima do dash
    [SerializeField] private int dashDamage = 7;
    [SerializeField] private TrailRenderer dashTrail;
    
    [Header("📊 Visual")]
    [SerializeField] private SpriteRenderer goblinSprite;
    [SerializeField] private Animator animator;
    
    // Estado
    private enum GoblinState
    {
        Positioning,    // Se posicionando no mesmo Y do player
        Charging,       // Preparando o dash
        Dashing,        // Executando o dash
        Recovering      // Recuperando após o dash
    }
    
    private GoblinState currentState = GoblinState.Positioning;
    private float lastDashTime = -999f;
    private float stateStartTime;
    
    private Vector2 dashDirection;
    private Vector2 targetPosition;
    private Vector2 lastPosition;
    private bool isCurrentlyMoving = false;
    private float movementCheckInterval = 0.1f;
    private float lastMovementCheck = 0f;
    
    private Vector2 dashStartPosition; // Posição onde o dash começou
    private float maxDashDistance = 15f; // Distância máxima do dash
    
    private Rigidbody2D rb;
    private Coroutine chargeFlashCoroutine;
    private Color originalColor;
    
    protected override void Start()
    {
        // 👹 Stats do Goblin - RÁPIDO E FRACO
        damage = dashDamage;
        moveSpeed = normalMoveSpeed;
        attackRange = 1f;
        attackCooldown = 0.7f;
        
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
        
        // Setup visual
        if (goblinSprite == null)
        {
            goblinSprite = GetComponentInChildren<SpriteRenderer>();
        }
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (goblinSprite != null)
        {
            originalColor = goblinSprite.color;
        }
        
        // Desativa trail no início
        if (dashTrail != null)
        {
            dashTrail.emitting = false;
        }
        
        lastPosition = transform.position;
        ChangeState(GoblinState.Positioning);
        
        Debug.Log("👹 Goblin pronto para atacar!");
    }
    
    protected override void Update()
    {
        if (player == null) return;
        
        // Gerencia comportamento baseado no estado
        switch (currentState)
        {
            case GoblinState.Positioning:
                HandlePositioning();
                break;
            case GoblinState.Charging:
                HandleCharging();
                break;
            case GoblinState.Dashing:
                HandleDashing();
                break;
            case GoblinState.Recovering:
                HandleRecovering();
                break;
        }
    }
    
    protected override void HandleSpriteFlip()
    {
        // Desabilitado - o flip é controlado pelas funções de animação
    }
    
    void ChangeState(GoblinState newState)
    {
        currentState = newState;
        stateStartTime = Time.time;
        
        // Ações ao entrar no estado
        switch (newState)
        {
            case GoblinState.Charging:
                if (navAgent != null)
                    navAgent.enabled = false;
                if (rb != null)
                    rb.linearVelocity = Vector2.zero;
                // Inicia flash amarelo
                if (chargeFlashCoroutine != null)
                    StopCoroutine(chargeFlashCoroutine);
                chargeFlashCoroutine = StartCoroutine(ChargeFlashEffect());
                break;
                
            case GoblinState.Dashing:
                if (navAgent != null)
                    navAgent.enabled = false;
                if (dashTrail != null)
                    dashTrail.emitting = true;
                // Para o flash
                if (chargeFlashCoroutine != null)
                {
                    StopCoroutine(chargeFlashCoroutine);
                    chargeFlashCoroutine = null;
                }
                if (goblinSprite != null)
                    goblinSprite.color = originalColor;
                // Salva posição inicial do dash
                dashStartPosition = transform.position;
                break;
                
            case GoblinState.Recovering:
                if (dashTrail != null)
                    dashTrail.emitting = false;
                if (rb != null)
                    rb.linearVelocity = Vector2.zero;
                break;
                
            case GoblinState.Positioning:
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
        
        // Calcula posição alvo: mesmo Y do player, mas numa distância X segura
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
            if (Time.time >= lastDashTime + dashCooldown)
            {
                ChangeState(GoblinState.Charging);
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
        
        // Calcula direção do dash APENAS HORIZONTAL
        float horizontalDirection = player.position.x > transform.position.x ? 1f : -1f;
        dashDirection = new Vector2(horizontalDirection, 0f);
        
        // Atualiza animação para olhar na direção do player
        UpdateMovementAnimation(dashDirection);
        
        // Após tempo de carga, dasha!
        if (Time.time >= stateStartTime + dashChargeTime)
        {
            ChangeState(GoblinState.Dashing);
            lastDashTime = Time.time;
        }
    }
    
    // ========================================
    // DASHANDO
    // ========================================
    void HandleDashing()
    {
        // Move rapidamente na direção horizontal
        if (rb != null)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
        }
        else
        {
            transform.position += (Vector3)(dashDirection * dashSpeed * Time.deltaTime);
        }
        
        // Atualiza animação de ataque (só horizontal) - apenas uma vez
        if (Time.time - stateStartTime < 0.1f)
        {
            UpdateAttackAnimation(dashDirection);
        }
        
        // Verifica se o tempo de dash acabou OU se percorreu distância máxima
        float dashTime = Time.time - stateStartTime;
        float distanceTraveled = Vector2.Distance(transform.position, dashStartPosition);
        
        if (dashTime >= dashDuration || distanceTraveled >= maxDashDistance)
        {
            ChangeState(GoblinState.Recovering);
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
            ChangeState(GoblinState.Positioning);
        }
    }
    
    // ========================================
    // ANIMAÇÕES
    // ========================================
    void UpdateMovementAnimation(Vector2 direction)
    {
        if (animator == null) return;
        
        // Detecta se está se movendo
        if (Time.time - lastMovementCheck >= movementCheckInterval)
        {
            Vector2 currentPosition = transform.position;
            float distanceMoved = Vector2.Distance(currentPosition, lastPosition);
            isCurrentlyMoving = distanceMoved > 0.05f;
            lastPosition = currentPosition;
            lastMovementCheck = Time.time;
        }
        
        // Escolhe prefixo baseado em movimento
        string animPrefix = "Walk";
        
        // Normaliza direção para achar qual é a predominante
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Movimento horizontal predominante        
            if (direction.x > 0) {
                animator.Play(animPrefix + "Right");
            }
            else {
                animator.Play(animPrefix + "Left");
            }
        }
        else
        {
            // Movimento vertical predominante
            if (direction.y > 0)
                animator.Play(animPrefix + "Up");
            else
                animator.Play(animPrefix + "Down");
        }
    }
    
    void UpdateAttackAnimation(Vector2 direction)
    {
        if (animator == null) return;
        
        // Ataque só em horizontal (esquerda/direita)     
        if (direction.x > 0) {
            animator.Play("AttackRight");
        } else {
            animator.Play("AttackLeft");
        }
    }
    
    // ========================================
    // EFEITO VISUAL DE CHARGE
    // ========================================
    IEnumerator ChargeFlashEffect()
    {
        Color yellowFlash = Color.yellow;
        float flashSpeed = 10f;
        
        while (true)
        {
            if (goblinSprite != null)
            {
                float lerp = Mathf.PingPong(Time.time * flashSpeed, 1);
                goblinSprite.color = Color.Lerp(originalColor, yellowFlash, lerp);
            }
            yield return null;
        }
    }
    
    
    // ========================================
    // COLISÃO
    // ========================================
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Se dash acerta o player durante o dash, causa dano
        if (currentState == GoblinState.Dashing && collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(dashDamage);
                Debug.Log("💥 Goblin acertou dash no player!");
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Também detecta via trigger
        if (currentState == GoblinState.Dashing && collision.CompareTag("Player"))
        {
            PlayerController pc = collision.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamage(dashDamage);
                Debug.Log("💥 Goblin acertou dash no player!");
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        
        // Desenha range do dash
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, minDistanceFromPlayer);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxDistanceFromPlayer);
        
        // Linha até a posição alvo
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, targetPosition);
    }
}