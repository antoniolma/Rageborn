using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    
    private Rigidbody2D rb;
    
    void Start()
    {

        DontDestroyOnLoad(gameObject);

        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(int damage)
    {
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
        Debug.Log("Player morreu!");
        // TODO: Implementar lógica de morte (restart, menu, etc)
        gameObject.SetActive(false);
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth += amount; // Também cura o valor aumentado
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log($"❤️ Vida máxima aumentada em {amount}! Nova vida máxima: {maxHealth}");
    }
    
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}
