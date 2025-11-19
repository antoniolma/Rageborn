using UnityEngine;

public class EnemyCoinDropper : MonoBehaviour
{
    [Header("Coin Drop Settings")]
    [SerializeField] private CoinDropData coinDropData;
    [SerializeField] private bool guaranteedDrop = true;
    [Range(0f, 100f)]
    [SerializeField] private float dropChance = 100f;
    
    public void DropCoins()
    {
        if (coinDropData == null)
        {
            Debug.LogWarning("CoinDropData não configurado!");
            return;
        }
        
        // Verifica se vai dropar alguma moeda
        if (!guaranteedDrop && Random.Range(0f, 100f) > dropChance)
        {
            return;
        }
        
        // Pega o tipo de moeda aleatório baseado nas chances
        CoinDropData.CoinDropChance selectedCoin = coinDropData.GetRandomCoinDrop();
        
        if (selectedCoin.coinPrefab == null)
        {
            Debug.LogWarning($"Prefab da moeda {selectedCoin.coinType} não configurado!");
            return;
        }
        
        // Quantidade de moedas a dropar
        int coinsAmount = Random.Range(selectedCoin.minAmount, selectedCoin.maxAmount + 1);
        
        for (int i = 0; i < coinsAmount; i++)
        {
            // Posição aleatória ao redor do inimigo
            Vector2 randomOffset = Random.insideUnitCircle * coinDropData.dropRadius;
            Vector3 dropPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            
            GameObject coin = Instantiate(selectedCoin.coinPrefab, dropPosition, Quaternion.identity);
            
            // Configura o tipo da moeda
            CoinPickup coinScript = coin.GetComponent<CoinPickup>();
            if (coinScript != null)
            {
                coinScript.SetCoinType(selectedCoin.coinType);
            }
            
            // Adiciona uma pequena força para espalhar as moedas
            Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                rb.AddForce(randomDirection * coinDropData.dropForce, ForceMode2D.Impulse);
            }
        }
        
        Debug.Log($"💰 Dropou {coinsAmount}x moeda(s) de {selectedCoin.coinType}!");
    }
}