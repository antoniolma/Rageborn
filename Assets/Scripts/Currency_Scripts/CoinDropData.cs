using UnityEngine;

[CreateAssetMenu(fileName = "Coin Drop Data", menuName = "Rageborn/Coin Drop Data")]
public class CoinDropData : ScriptableObject
{
    [System.Serializable]
    public class CoinDropChance
    {
        public CoinType coinType;
        public GameObject coinPrefab;
        [Range(0f, 100f)]
        public float dropChance; // Porcentagem de chance
        [Range(0, 10)]
        public int minAmount = 1;
        [Range(0, 10)]
        public int maxAmount = 3;
    }
    
    [Header("Coin Drop Settings")]
    public CoinDropChance[] coinDropChances;
    
    [Header("Drop Force")]
    public float dropForce = 2f;
    public float dropRadius = 0.5f;
    
    // Método para pegar um tipo de moeda aleatório baseado nas chances
    public CoinDropChance GetRandomCoinDrop()
    {
        float totalWeight = 0f;
        foreach (var coin in coinDropChances)
        {
            totalWeight += coin.dropChance;
        }
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var coin in coinDropChances)
        {
            currentWeight += coin.dropChance;
            if (randomValue <= currentWeight)
            {
                return coin;
            }
        }
        
        return coinDropChances[0]; // Fallback para cobre
    }
}