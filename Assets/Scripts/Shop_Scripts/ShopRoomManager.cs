using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShopRoomManager : MonoBehaviour
{
    [Header("Shop Items Pool")]
    [SerializeField] private List<ShopItemData> availableItems = new List<ShopItemData>();
    
    [Header("Spawn Settings")]
    [SerializeField] private ShopSpawnPoint[] spawnPoints;
    [SerializeField] private int minItems = 3;
    [SerializeField] private int maxItems = 5;
    [SerializeField] private bool allowDuplicates = false;
    
    [Header("Portal")]
    [SerializeField] private GameObject portalToNextLevel;
    
    void Start()
    {
        SpawnRandomItems();
        
        if (portalToNextLevel != null)
        {
            portalToNextLevel.SetActive(true);
        }
    }
    
    void SpawnRandomItems()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("❌ Nenhum spawn point configurado!");
            return;
        }
        
        if (availableItems.Count == 0)
        {
            Debug.LogError("❌ Nenhum item disponível na pool!");
            return;
        }
        
        // Determina quantos itens spawnar
        int itemsToSpawn = Random.Range(minItems, maxItems + 1);
        itemsToSpawn = Mathf.Min(itemsToSpawn, spawnPoints.Length);
        
        // Embaralha os spawn points
        List<ShopSpawnPoint> shuffledPoints = spawnPoints.ToList();
        ShuffleList(shuffledPoints);
        
        // Lista de itens já usados (se não permitir duplicatas)
        List<ShopItemData> usedItems = new List<ShopItemData>();
        
        // Spawna os itens
        for (int i = 0; i < itemsToSpawn; i++)
        {
            ShopItemData randomItem = GetRandomItem(usedItems);
            
            if (randomItem != null)
            {
                shuffledPoints[i].SpawnItem(randomItem);
                
                if (!allowDuplicates)
                    usedItems.Add(randomItem);
            }
        }
        
        Debug.Log($"🛒 Spawnados {itemsToSpawn} itens na loja!");
    }
    
    ShopItemData GetRandomItem(List<ShopItemData> excludeItems)
    {
        List<ShopItemData> availablePool = availableItems.Where(item => !excludeItems.Contains(item)).ToList();
        
        if (availablePool.Count == 0)
        {
            // Se não tiver mais itens únicos, permite repetir
            availablePool = availableItems;
        }
        
        if (availablePool.Count == 0)
        {
            Debug.LogWarning("⚠️ Nenhum item disponível na pool!");
            return null;
        }
        
        return availablePool[Random.Range(0, availablePool.Count)];
    }
    
    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    public void ClearAllItems()
    {
        foreach (ShopSpawnPoint point in spawnPoints)
        {
            point.ClearItem();
        }
    }
}