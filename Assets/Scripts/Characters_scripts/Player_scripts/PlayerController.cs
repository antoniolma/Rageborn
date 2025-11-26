using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    
    private Rigidbody2D rb;
    private bool isDead = false;
    
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        Debug.Log($"Player HP: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Player healed! HP: {currentHealth}/{maxHealth}");
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log("💀 Player morreu!");
        
        // Chama o Game Over
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.TriggerGameOver();
        }
        else
        {
            Debug.LogWarning("⚠️ GameOverManager não encontrado!");
        }
        
        gameObject.SetActive(false);
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log($"❤️ Vida máxima aumentada em {amount}! Nova vida máxima: {maxHealth}");
    }
    
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    
    // Método para resetar o player (útil para reiniciar o jogo)
    public void ResetPlayer()
    {
        isDead = false;
        currentHealth = maxHealth;
        gameObject.SetActive(true);
    }
}