using UnityEngine;

// ✅ Enum CoinType removido daqui - já existe em CoinType.cs

public class CoinPickup : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private CoinType coinType = CoinType.Copper;
    [SerializeField] private int coinValue = 1;
    
    [Header("Magnet Settings")]
    [SerializeField] private float magnetRange = 3f;
    [SerializeField] private float moveSpeed = 8f;
    
    [Header("Float Animation")]
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatHeight = 0.3f;
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private Transform player;
    private bool isBeingAttracted = false;
    private Vector3 startPosition;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        startPosition = transform.position;
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        SetCoinValue();
    }
    
    void SetCoinValue()
    {
        switch (coinType)
        {
            case CoinType.Copper:
                coinValue = 1;
                break;
            case CoinType.Silver:
                coinValue = 5;
                break;
            case CoinType.Gold:
                coinValue = 10;
                break;
        }
    }
    
    public void SetCoinType(CoinType type)
    {
        coinType = type;
        SetCoinValue();
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Atrai a moeda quando o jogador está próximo
        if (distanceToPlayer <= magnetRange)
        {
            isBeingAttracted = true;
        }
        
        if (isBeingAttracted)
        {
            // Move em direção ao player
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
        else
        {
            // Animação de flutuação
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CurrencyManager.Instance.AddRunCoins(coinValue);
            
            // Efeito visual simples de coleta
            if (spriteRenderer != null)
            {
                StartCoroutine(CollectAnimation());
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    
    System.Collections.IEnumerator CollectAnimation()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Diminui o tamanho
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            
            // Move um pouco pra cima
            transform.position += Vector3.up * Time.deltaTime * 2f;
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }
}