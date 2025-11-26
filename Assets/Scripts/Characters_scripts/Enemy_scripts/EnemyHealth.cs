using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 50;
    private int currentHealth;
    
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    
    [Header("UI")]
    [SerializeField] private EnemyHealthBar healthBarScript;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    private AudioSource audioSource;
    
    [Header("Death Effects")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private float deathDelay = 0.1f;
    
    [Header("Screen Shake on Death")]
    [SerializeField] private bool shakeOnDeath = true;
    [SerializeField] private float shakeIntensity = 0.3f;
    [SerializeField] private float shakeDuration = 0.2f;
    
    // Components
    private DamageFlash damageFlash;
    private EnemyCoinDropper coinDropper;
    private EnemyStatusEffects statusEffects;
    
    // Event que o spawner escuta
    public event Action OnDeath;
    
    void Start()
    {
        currentHealth = maxHealth;
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        audioSource = GetComponent<AudioSource>();
        damageFlash = GetComponent<DamageFlash>();
        coinDropper = GetComponent<EnemyCoinDropper>();
        statusEffects = GetComponent<EnemyStatusEffects>();
        
        // Se não tem EnemyStatusEffects, adiciona automaticamente
        if (statusEffects == null)
        {
            statusEffects = gameObject.AddComponent<EnemyStatusEffects>();
        }
        
        // Se não encontrou EnemyHealthBar mas encontrou HealthBar (antigo), avisa
        if (healthBarScript == null)
        {
            healthBarScript = GetComponentInChildren<EnemyHealthBar>();
        }
    }
    
    public void TakeDamage(int damage, WeaponType weaponType = WeaponType.Fire, bool applyStatusEffects = true)
    {
        if (currentHealth <= 0) return; // Já está morto
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        
        Debug.Log($"👾 {gameObject.name} tomou {damage} de dano de {WeaponTypeHelper.GetWeaponName(weaponType)}! HP: {currentHealth}/{maxHealth}");
        
        // ===== FEEDBACK VISUAL =====
        
        // 1. Flash de dano (se tiver DamageFlash) com cor baseada no tipo de arma
        // ⚠️ EXCETO para Ice - o gelo tem efeito visual próprio (azul piscando)
        if (weaponType != WeaponType.Ice)
        {
            if (damageFlash != null)
            {
                damageFlash.FlashWithWeaponType(weaponType);
            }
            else if (spriteRenderer != null)
            {
                // Fallback: flash manual com cor da arma
                Color weaponColor = WeaponTypeHelper.GetDamageFlashColor(weaponType);
                StartCoroutine(FlashDamage(weaponColor));
            }
        }
        
        // 2. Atualiza HealthBar
        if (healthBarScript != null)
        {
            healthBarScript.ForceUpdate();
        }
        
        // ===== FEEDBACK AUDIO =====
        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }
        
        // ===== EFEITOS ESPECIAIS POR TIPO DE ARMA =====
        // ⚠️ IMPORTANTE: Só aplica efeitos de status no hit inicial, NÃO nos ticks de queimadura!
        if (statusEffects != null && applyStatusEffects)
        {
            switch (weaponType)
            {
                case WeaponType.Fire:
                    // 🔥 Fogo: Dano ao longo do tempo (3 ticks de dano a cada 0.8s)
                    // Dano por tick = 20% (1/5) do dano inicial do golpe
                    int burnDamage = Mathf.Max(1, Mathf.RoundToInt(damage * 0.2f));
                    statusEffects.ApplyBurnEffect(burnDamage);
                    break;
                
                case WeaponType.Ice:
                    // ❄️ Gelo: Reduz velocidade para 2/3 por 1.2s
                    statusEffects.ApplyFreezeEffect();
                    break;
                
                case WeaponType.Venom:
                    // 🧪 Veneno: (pode adicionar efeito depois se quiser)
                    // Por enquanto só o dano normal
                    break;
            }
        }
        
        // ===== MORTE =====
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log($"💀 {gameObject.name} morreu!");
        
        // ===== AUDIO =====
        if (deathSound != null)
        {
            // Cria um GameObject temporário para tocar o som (não será destruído com o inimigo)
            GameObject soundObject = new GameObject("DeathSound");
            AudioSource tempAudioSource = soundObject.AddComponent<AudioSource>();
            tempAudioSource.clip = deathSound;
            tempAudioSource.volume = 0.75f; // 50% do volume
            tempAudioSource.Play();
            Destroy(soundObject, deathSound.length);
        }
        
        // ===== SCREEN SHAKE =====
        if (shakeOnDeath)
        {
            CameraShake cameraShake = Camera.main?.GetComponent<CameraShake>();
            if (cameraShake != null)
            {
                cameraShake.Shake(shakeDuration, shakeIntensity);
            }
            else
            {
                Debug.LogWarning("⚠️ CameraShake não encontrado na Main Camera!");
            }
        }
        
        // ===== EFEITO VISUAL DE MORTE =====
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // ===== DROP DE MOEDAS =====
        if (coinDropper != null)
        {
            // ✅ CORRIGIDO: Chama sem argumentos
            coinDropper.DropCoins();
        }
        
        // ===== NOTIFICA SPAWNER =====
        OnDeath?.Invoke();
        
        // ===== DESTRÓI O INIMIGO =====
        Destroy(gameObject, deathDelay);
    }
    
    System.Collections.IEnumerator FlashDamage(Color colorToFlash)
    {
        if (spriteRenderer == null) yield break;
        
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = colorToFlash;
        yield return new WaitForSeconds(flashDuration);
        
        // Verifica se ainda existe antes de restaurar cor
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
    
    // ===== MÉTODOS PÚBLICOS =====
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;
    
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        if (healthBarScript != null)
        {
            healthBarScript.ForceUpdate();
        }
    }
    
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
        
        if (healthBarScript != null)
        {
            healthBarScript.ForceUpdate();
        }
    }
}