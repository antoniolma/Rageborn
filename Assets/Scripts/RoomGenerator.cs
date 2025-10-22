using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomGenerator : MonoBehaviour
{
    [Header("Room Templates")]
    [SerializeField] private int totalRoomTemplates = 5; // 5 tipos diferentes de sala
    
    [Header("Room Settings")]
    [SerializeField] private Vector2 roomSize = new Vector2(20f, 15f);
    [SerializeField] private float roomSpacing = 25f;
    
    [Header("Enemy Spawning")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int minEnemiesPerRoom = 2;
    [SerializeField] private int maxEnemiesPerRoom = 5;
    
    [Header("Boss Room")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private int bossRoomEnemies = 1; // Só o boss
    
    private List<RoomData> roomSequence = new List<RoomData>();
    private int currentRoomIndex = 0;
    
    void Start()
    {
        GenerateRoomSequence();
        SpawnAllRooms();
        SpawnPlayerInFirstRoom();
    }
    
    /// <summary>
    /// Gera a sequência: 5 salas aparecem 2x cada (aleatório) + 1 sala de boss
    /// </summary>
    void GenerateRoomSequence()
    {
        List<int> roomTypes = new List<int>();
        
        // Adiciona cada tipo de sala 2 vezes (1, 2, 3, 4, 5, 1, 2, 3, 4, 5)
        for (int i = 0; i < totalRoomTemplates; i++)
        {
            roomTypes.Add(i);
            roomTypes.Add(i); // Adiciona 2x
        }
        
        // Embaralha a ordem
        roomTypes = roomTypes.OrderBy(x => Random.value).ToList();
        
        // Cria as 10 salas normais
        for (int i = 0; i < roomTypes.Count; i++)
        {
            Vector2 position = new Vector2(i * roomSpacing, 0);
            RoomData room = new RoomData(
                position, 
                roomSize, 
                roomTypes[i], 
                RoomType.Normal
            );
            roomSequence.Add(room);
        }
        
        // Adiciona sala do boss no final
        Vector2 bossPosition = new Vector2(roomTypes.Count * roomSpacing, 0);
        RoomData bossRoom = new RoomData(
            bossPosition,
            roomSize * 1.5f, // Sala do boss é maior
            -1, // ID especial para boss
            RoomType.Boss
        );
        roomSequence.Add(bossRoom);
        
        // Log para debug
        Debug.Log("=== SEQUÊNCIA DE SALAS GERADA ===");
        for (int i = 0; i < roomSequence.Count; i++)
        {
            string roomInfo = roomSequence[i].type == RoomType.Boss 
                ? "BOSS ROOM" 
                : $"Sala Tipo {roomSequence[i].templateID + 1}";
            Debug.Log($"Posição {i}: {roomInfo}");
        }
    }
    
    void SpawnAllRooms()
    {
        for (int i = 0; i < roomSequence.Count; i++)
        {
            RoomData room = roomSequence[i];
            
            if (room.type == RoomType.Boss)
            {
                SpawnBossRoom(room);
            }
            else if (i > 0) // Não spawna inimigos na primeira sala (spawn do player)
            {
                SpawnEnemiesInRoom(room);
            }
        }
    }
    
    void SpawnEnemiesInRoom(RoomData room)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy Prefab não configurado!");
            return;
        }
        
        // Você pode variar inimigos baseado no templateID
        int enemyCount = GetEnemyCountForTemplate(room.templateID);
        
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2 randomPos = GetRandomPositionInRoom(room);
            GameObject enemy = Instantiate(enemyPrefab, randomPos, Quaternion.identity);
            enemy.name = $"Enemy_{room.templateID}_{i}";
        }
        
        Debug.Log($"Sala Tipo {room.templateID + 1}: {enemyCount} inimigos spawnados");
    }
    
    void SpawnBossRoom(RoomData room)
    {
        if (bossPrefab != null)
        {
            // Boss spawna no centro da sala
            Instantiate(bossPrefab, room.position, Quaternion.identity);
            Debug.Log("Boss spawnado na sala final!");
        }
        else
        {
            Debug.LogWarning("Boss Prefab não configurado! Usando inimigo normal.");
            if (enemyPrefab != null)
            {
                Instantiate(enemyPrefab, room.position, Quaternion.identity);
            }
        }
    }
    
    /// <summary>
    /// Você pode customizar quantos inimigos cada tipo de sala tem
    /// </summary>
    int GetEnemyCountForTemplate(int templateID)
    {
        // Exemplo: salas diferentes têm dificuldades diferentes
        switch (templateID)
        {
            case 0: return Random.Range(2, 4); // Sala tipo 1: mais fácil
            case 1: return Random.Range(3, 5); // Sala tipo 2: média
            case 2: return Random.Range(3, 6); // Sala tipo 3: média-difícil
            case 3: return Random.Range(4, 6); // Sala tipo 4: difícil
            case 4: return Random.Range(4, 7); // Sala tipo 5: mais difícil
            default: return Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);
        }
    }
    
    Vector2 GetRandomPositionInRoom(RoomData room)
    {
        return new Vector2(
            Random.Range(room.position.x - room.size.x / 2, room.position.x + room.size.x / 2),
            Random.Range(room.position.y - room.size.y / 2, room.position.y + room.size.y / 2)
        );
    }
    
    void SpawnPlayerInFirstRoom()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && roomSequence.Count > 0)
        {
            player.transform.position = roomSequence[0].position;
            Debug.Log("Player spawnado na primeira sala!");
        }
    }
    
    public RoomData GetCurrentRoom()
    {
        if (currentRoomIndex < roomSequence.Count)
            return roomSequence[currentRoomIndex];
        return null;
    }
    
    public RoomData GetRoomAtIndex(int index)
    {
        if (index >= 0 && index < roomSequence.Count)
            return roomSequence[index];
        return null;
    }
    
    public int GetTotalRooms() => roomSequence.Count;
    public bool IsLastRoom(int index) => index == roomSequence.Count - 1;
}

// Enum para tipos de sala
public enum RoomType
{
    Normal,
    Boss,
    Treasure, // Para futuras expansões
    Shop      // Para futuras expansões
}

// Classe de dados da sala
[System.Serializable]
public class RoomData
{
    public Vector2 position;
    public Vector2 size;
    public int templateID; // Qual dos 5 tipos de sala (0-4)
    public RoomType type;
    public bool cleared;
    
    public RoomData(Vector2 pos, Vector2 roomSize, int template, RoomType roomType)
    {
        position = pos;
        size = roomSize;
        templateID = template;
        type = roomType;
        cleared = false;
    }
}
