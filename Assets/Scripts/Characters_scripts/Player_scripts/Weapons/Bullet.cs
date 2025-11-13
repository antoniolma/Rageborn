using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Camera mainCam;
    private Vector3 mousePos;
    private Rigidbody2D rb;
    
    [Header("Bullet Settings")]
    [SerializeField] private float force = 20f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifetime = 5f;
    
    [Header("Visual Effects (Optional)")]
    [SerializeField] private GameObject hitEffectPrefab;
    
    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        rb = GetComponent<Rigidbody2D>();
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        // Calcula direção
        Vector3 direction = mousePos - transform.position;
        
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.linearVelocity = new Vector2(direction.x, direction.y).normalized * force;
        }
        else
        {
            // Se for Kinematic, move manualmente
            StartCoroutine(MoveKinematic(direction.normalized));
        }
    
        // Rotaciona o bullet na direção
        Vector3 rotation = transform.position - mousePos;
        float rot = Mathf.Atan2(rotation.x, rotation.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rot + 90);
        
        // Destroi após X segundos
        Destroy(gameObject, lifetime);
    }
    
    private System.Collections.IEnumerator MoveKinematic(Vector2 direction)
    {
        while (true)
        {
            if (rb != null)
            {
                Vector2 movement = direction * force * Time.fixedDeltaTime;
                rb.MovePosition(rb.position + movement);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    void Update()
    {
        
    }
    
    // ========== SISTEMA DE COLISÃO E DANO ==========
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se colidiu com inimigo
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"💥 Bullet acertou Enemy! Dano: {damage}");
            }
            
            SpawnHitEffect();
            Destroy(gameObject);
        }
        
        // Verifica se colidiu com parede/obstáculo
        if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle"))
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
    
    // Função pública para definir o dano (útil para diferentes tipos de espada)
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
}