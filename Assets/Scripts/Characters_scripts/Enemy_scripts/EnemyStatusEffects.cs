using UnityEngine;
using System.Collections;

/// <summary>
/// Gerencia efeitos de status nos inimigos (queimadura, congelamento, veneno, etc.)
/// </summary>
public class EnemyStatusEffects : MonoBehaviour
{
    [Header("Efeito de Fogo 🔥")]
    [SerializeField] private int burnTicksRemaining = 0;
    [SerializeField] private int burnDamagePerTick = 5;
    [SerializeField] private float burnTickInterval = 0.8f;
    [SerializeField] private int burnMaxTicks = 3;
    private Coroutine burnCoroutine;
    
    [Header("Efeito de Gelo ❄️")]
    [SerializeField] private bool isFrozen = false;
    [SerializeField] private float frozenSpeedMultiplier = 0.66f; // 2/3 da velocidade original
    [SerializeField] private float freezeDuration = 1.2f;
    private Coroutine freezeCoroutine;
    private float originalMoveSpeed; // ✅ Salva a velocidade ANTES do freeze
    
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color burnFlashColor = new Color(1f, 0.5f, 0f); // Laranja
    [SerializeField] private Color freezeColorDark = new Color(0.3f, 0.5f, 0.8f); // Azul escuro
    [SerializeField] private Color freezeColorLight = new Color(0.6f, 0.8f, 1f); // Azul claro
    [SerializeField] private float freezeFlashSpeed = 3f; // Velocidade da piscada azul
    
    // Components
    private EnemyHealth enemyHealth;
    private Enemy enemy;
    private DamageFlash damageFlash;
    private Color originalColor;
    
    void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        enemy = GetComponent<Enemy>();
        damageFlash = GetComponent<DamageFlash>();
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }
    
    void Start()
    {
        // Salva a velocidade original do inimigo
        if (enemy != null)
            originalMoveSpeed = enemy.GetMoveSpeed();
    }
    
    // ========================================
    // EFEITO DE FOGO 🔥
    // ========================================
    
    /// <summary>
    /// Aplica efeito de queimadura que causa dano ao longo do tempo
    /// </summary>
    public void ApplyBurnEffect(int damagePerTick)
    {
        burnDamagePerTick = damagePerTick;
        
        // Se já está queimando, reinicia o efeito
        if (burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
        }
        
        burnTicksRemaining = burnMaxTicks;
        burnCoroutine = StartCoroutine(BurnCoroutine());
        
        Debug.Log($"🔥 {gameObject.name} está QUEIMANDO! ({burnMaxTicks} ticks de {burnDamagePerTick} dano cada)");
    }
    
    private IEnumerator BurnCoroutine()
    {
        int tickCount = 0; // Contador de ticks executados
        
        while (tickCount < burnMaxTicks)
        {
            yield return new WaitForSeconds(burnTickInterval);
            
            // Verifica se o inimigo ainda existe
            if (enemyHealth == null || enemyHealth.GetCurrentHealth() <= 0)
            {
                burnTicksRemaining = 0;
                burnCoroutine = null;
                yield break;
            }
            
            // ⚠️ IMPORTANTE: applyStatusEffects = FALSE para NÃO criar loop infinito!
            // O dano de queimadura NÃO deve reaplicar a queimadura
            enemyHealth.TakeDamage(burnDamagePerTick, WeaponType.Fire, applyStatusEffects: false);
            tickCount++; // Incrementa contador
            burnTicksRemaining = burnMaxTicks - tickCount; // Atualiza ticks restantes
            
            Debug.Log($"🔥 {gameObject.name} sofreu {burnDamagePerTick} de dano por queimadura! Tick {tickCount}/{burnMaxTicks}");
        }
        
        burnTicksRemaining = 0;
        burnCoroutine = null;
        Debug.Log($"🔥 Efeito de queimadura terminou em {gameObject.name} (3 ticks completos)");
    }
    
    // ========================================
    // EFEITO DE GELO ❄️
    // ========================================
    
    /// <summary>
    /// Aplica efeito de congelamento que reduz velocidade
    /// </summary>
    public void ApplyFreezeEffect()
    {
        // ❄️ Se já está congelado, REINICIA o timer (não staceia!)
        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
            Debug.Log($"❄️ {gameObject.name} já estava congelado - REINICIANDO timer!");
            
            // Restaura velocidade antes de reaplicar (evita stacking)
            if (isFrozen)
            {
                RemoveSpeedReduction();
            }
        }
        else
        {
            Debug.Log($"❄️ {gameObject.name} foi CONGELADO! Velocidade reduzida para {frozenSpeedMultiplier * 100}% por {freezeDuration}s");
        }
        
        freezeCoroutine = StartCoroutine(FreezeCoroutine());
    }
    
    private IEnumerator FreezeCoroutine()
    {
        isFrozen = true;
        
        // Salva a cor atual antes de aplicar o freeze
        Color colorBeforeFreeze = spriteRenderer != null ? spriteRenderer.color : Color.white;
        
        // Aplica redução de velocidade
        ApplySpeedReduction();
        
        // ❄️ PISCA ENTRE AZUL ESCURO E AZUL CLARO durante toda a duração
        float freezeStartTime = Time.time;
        
        while (Time.time - freezeStartTime < freezeDuration)
        {
            if (spriteRenderer != null)
            {
                // Alterna entre azul escuro e azul claro
                float t = Mathf.PingPong(Time.time * freezeFlashSpeed, 1f);
                spriteRenderer.color = Color.Lerp(freezeColorDark, freezeColorLight, t);
            }
            
            yield return null; // Espera próximo frame
        }
        
        // Remove efeito de congelamento
        isFrozen = false;
        RemoveSpeedReduction();
        
        // Restaura cor que estava ANTES do freeze (pode ser cor de fase 2, etc)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = colorBeforeFreeze;
        }
        
        freezeCoroutine = null;
        Debug.Log($"❄️ Efeito de congelamento terminou em {gameObject.name}");
    }
    
    private void ApplySpeedReduction()
    {
        if (enemy == null) return;
        
        // ✅ Salva a velocidade ATUAL (antes do freeze)
        // Se já estava congelado, isso pega a velocidade BASE (após RemoveSpeedReduction)
        originalMoveSpeed = enemy.GetMoveSpeed();
        
        // Aplica redução de velocidade de movimento
        float reducedSpeed = originalMoveSpeed * frozenSpeedMultiplier;
        enemy.SetMoveSpeed(reducedSpeed);
        
        // ❄️⚡ Se for Bob, também reduz velocidade de DASH!
        Bob_Boss bobBoss = enemy as Bob_Boss;
        if (bobBoss != null)
        {
            bobBoss.SetDashSpeedMultiplier(frozenSpeedMultiplier);
            Debug.Log($"❄️⚡ BOB - Dash também congelado! Velocidade de dash reduzida para {frozenSpeedMultiplier * 100}%");
        }
        
        Debug.Log($"❄️ Velocidade reduzida: {originalMoveSpeed:F1} → {reducedSpeed:F1} (sem stack!)");
    }
    
    private void RemoveSpeedReduction()
    {
        if (enemy == null) return;
        
        // ✅ IMPORTANTE: Restaura para a velocidade BASE, não a velocidade salva!
        // Isso garante que se o boss mudou de fase durante o freeze, usa a velocidade da nova fase
        float baseSpeed = enemy.GetBaseMoveSpeed();
        enemy.SetMoveSpeed(baseSpeed);
        
        // ❄️⚡ Se for Bob, também restaura velocidade de DASH!
        Bob_Boss bobBoss = enemy as Bob_Boss;
        if (bobBoss != null)
        {
            bobBoss.SetDashSpeedMultiplier(1f); // Restaura para 100%
            Debug.Log($"❄️⚡ BOB - Dash descongelado! Velocidade de dash restaurada para 100%");
        }
        
        Debug.Log($"❄️ Velocidade restaurada para velocidade base: {baseSpeed}");
    }
    
    // ========================================
    // GETTERS
    // ========================================
    
    public bool IsBurning() => burnCoroutine != null;
    public bool IsFrozen() => isFrozen;
    public int GetBurnTicksRemaining() => burnTicksRemaining;
    
    // ========================================
    // CLEANUP
    // ========================================
    
    void OnDestroy()
    {
        // Para todas as coroutines quando o inimigo morre
        if (burnCoroutine != null)
            StopCoroutine(burnCoroutine);
        
        if (freezeCoroutine != null)
            StopCoroutine(freezeCoroutine);
    }
}
