using UnityEngine;

public class ShopSpawnPoint : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private GameObject pedestalVisual;
    [SerializeField] private Transform itemSpawnPosition;
    
    private GameObject spawnedItem;
    
    public void SpawnItem(ShopItemData itemData)
    {
        if (itemData == null || itemData.itemPrefab == null)
        {
            Debug.LogWarning("Item data ou prefab está null!");
            return;
        }
        
        Vector3 spawnPos = itemSpawnPosition != null ? itemSpawnPosition.position : transform.position;
        spawnedItem = Instantiate(itemData.itemPrefab, spawnPos, Quaternion.identity, transform);
        
        // Configura o item com os dados
        InteractableItem itemScript = spawnedItem.GetComponent<InteractableItem>();
        if (itemScript != null)
        {
            itemScript.InitializeItem(itemData);
        }
    }
    
    public void ClearItem()
    {
        if (spawnedItem != null)
        {
            Destroy(spawnedItem);
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 pos = itemSpawnPosition != null ? itemSpawnPosition.position : transform.position;
        Gizmos.DrawWireCube(pos, Vector3.one * 0.5f);
        Gizmos.DrawLine(transform.position, pos);
    }
}