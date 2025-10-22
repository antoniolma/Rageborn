using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }
    
    void Update()
    {
        // Input de movimentação (WASD ou setas)
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize(); // Evita movimento mais rápido na diagonal
    }
    
    void FixedUpdate()
    {
        // Aplica movimento usando física
        rb.linearVelocity = moveInput * moveSpeed;
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
    
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}
