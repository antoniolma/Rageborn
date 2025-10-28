using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("Room Prefab")]
    public GameObject roomPrefab; // Só 1 prefab de sala para testar
    
    void Start()
    {
        // Instancia a sala na posição (0, 0)
        if (roomPrefab != null)
        {
            Instantiate(roomPrefab, Vector2.zero, Quaternion.identity);
            Debug.Log("✅ Sala instanciada!");
        }
        else
        {
            Debug.LogError("❌ Room Prefab não configurado no Inspector!");
        }
        
        // Posiciona o player na sala
        SpawnPlayer();
    }
    
    void SpawnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector2 spawnOffset = new Vector2(5, 5); // Ajuste conforme necessário
            player.transform.position = Vector2.zero;
            Debug.Log("✅ Player posicionado na sala!");
        }
        else
        {
            Debug.LogWarning("⚠️ Player não encontrado! Certifique-se que tem a tag 'Player'");
        }
    }
}