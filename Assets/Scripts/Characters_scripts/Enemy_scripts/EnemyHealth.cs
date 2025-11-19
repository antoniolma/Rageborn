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
        
        // Se não encontrou EnemyHealthBar mas encontrou HealthBar (antigo), avisa
        if (healthBarScript == null)
        {
            healthBarScript = GetComponentInChildren<EnemyHealthBar>();
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return; // Já está morto
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        
        Debug.Log($"👾 {gameObject.name} tomou {damage} de dano! HP: {currentHealth}/{maxHealth}");
        
        // ===== FEEDBACK VISUAL =====
        
        // 1. Flash de dano (se tiver DamageFlash)
        if (damageFlash != null)
        {
            damageFlash.Flash();
        }
        else if (spriteRenderer != null)
        {
            // Fallback: flash manual
            StartCoroutine(FlashDamage());
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
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
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
    
    System.Collections.IEnumerator FlashDamage()
    {
        if (spriteRenderer == null) yield break;
        
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = damageColor;
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