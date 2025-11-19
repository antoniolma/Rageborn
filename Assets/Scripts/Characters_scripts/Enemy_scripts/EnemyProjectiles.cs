using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifetime = 5f;
    
    [Header("Visual (Optional)")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private TrailRenderer trail;
    
    private Rigidbody2D rb;
    private bool isInitialized = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
        }
        
        // Auto-destrói após lifetime
        Destroy(gameObject, lifetime);
    }
    
    // Método para inicializar o projétil (chamado pelos inimigos)
    public void Initialize(Vector2 direction, int damageAmount, float projectileSpeed = -1)
    {
        isInitialized = true;
        damage = damageAmount;
        
        if (projectileSpeed > 0)
        {
            speed = projectileSpeed;
        }
        
        // Aplica velocidade
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
        
        // Rotaciona para a direção de movimento
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Acerta o player
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
                Debug.Log($"💥 Projétil inimigo acertou player! Dano: {damage}");
            }
            
            SpawnHitEffect();
            Destroy(gameObject);
        }
        
        // Acerta parede/obstáculo
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Acerta o player (se usar Collision)
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
                Debug.Log($"💥 Projétil inimigo acertou player! Dano: {damage}");
            }
            
            SpawnHitEffect();
            Destroy(gameObject);
        }
        
        // Acerta parede/obstáculo
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Obstacle"))
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
    }
    
    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }
}