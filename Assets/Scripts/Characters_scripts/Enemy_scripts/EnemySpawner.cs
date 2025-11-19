using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();
    
    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool useRandomSpawnInArea = false;
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-10, -10);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(10, 10);
    
    [Header("Wave Settings")]
    [SerializeField] private int startingWaves = 3;
    [SerializeField] private int baseEnemiesPerWave = 5;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float timeBetweenSpawns = 0.5f;
    
    [Header("Difficulty Scaling")]
    [SerializeField] private bool enableDifficultyScaling = true;
    [SerializeField] private float enemyIncreasePerRoom = 1.5f; // +50% inimigos por room
    [SerializeField] private float waveIncreasePerRoom = 0.5f; // +0.5 waves por room
    [SerializeField] private int maxEnemiesPerWave = 20;
    [SerializeField] private int maxWaves = 8;
    
    [Header("Shop Door")]
    [SerializeField] private GameObject shopDoor;
    [SerializeField] private string shopDoorName = "ShopDoorTrigger";
    
    [Header("UI (Optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI waveText;
    
    // Runtime variables
    private int currentWave = 0;
    private int totalWaves;
    private int enemiesPerWave;
    private int enemiesAlive = 0;
    private bool allWavesComplete = false;
    private bool isSpawning = false;
    
    // Dificuldade progressiva
    private int currentRoomNumber;
    
    void Start()
    {
        // Pega o número do room atual do RoomManager
        if (RoomManager.Instance != null)
        {
            currentRoomNumber = RoomManager.Instance.GetCurrentRoomNumber();
        }
        else
        {
            currentRoomNumber = 1; // Fallback
        }
        
        // Calcula dificuldade baseada no room
        CalculateDifficulty();
        
        // Encontra a porta da loja
        FindShopDoor();
        
        // Valida configuração
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogError("❌ Nenhum enemy prefab configurado no EnemySpawner!");
            return;
        }
        
        if (spawnPoints.Length == 0 && !useRandomSpawnInArea)
        {
            Debug.LogError("❌ Nenhum spawn point configurado e spawn aleatório desativado!");
            return;
        }
        
        // Inicia primeira wave após pequeno delay
        StartCoroutine(StartFirstWaveDelayed());
    }
    
    void CalculateDifficulty()
    {
        if (enableDifficultyScaling)
        {
            // Aumenta número de waves conforme progride
            float waveMultiplier = 1 + ((currentRoomNumber - 1) * waveIncreasePerRoom);
            totalWaves = Mathf.RoundToInt(startingWaves * waveMultiplier);
            totalWaves = Mathf.Min(totalWaves, maxWaves);
            
            // Aumenta número de inimigos por wave
            float enemyMultiplier = Mathf.Pow(enemyIncreasePerRoom, currentRoomNumber - 1);
            enemiesPerWave = Mathf.RoundToInt(baseEnemiesPerWave * enemyMultiplier);
            enemiesPerWave = Mathf.Min(enemiesPerWave, maxEnemiesPerWave);
            
            Debug.Log($"📊 Room {currentRoomNumber} - Dificuldade: {totalWaves} waves com {enemiesPerWave} inimigos cada");
        }
        else
        {
            totalWaves = startingWaves;
            enemiesPerWave = baseEnemiesPerWave;
        }
    }
    
    void FindShopDoor()
    {
        if (shopDoor == null)
        {
            shopDoor = GameObject.Find(shopDoorName);
            
            if (shopDoor == null)
            {
                Debug.LogWarning($"⚠️ '{shopDoorName}' não encontrada! Porta não será ativada.");
            }
        }
    }
    
    IEnumerator StartFirstWaveDelayed()
    {
        yield return new WaitForSeconds(2f);
        StartNextWave();
    }
    
    void Update()
    {
        // Verifica se pode iniciar próxima wave
        if (!isSpawning && enemiesAlive == 0 && currentWave < totalWaves && !allWavesComplete)
        {
            StartCoroutine(StartNextWaveWithDelay());
        }
        
        // Verifica se todas as waves terminaram
        if (enemiesAlive == 0 && currentWave >= totalWaves && !allWavesComplete)
        {
            OnAllWavesComplete();
        }
    }
    
    IEnumerator StartNextWaveWithDelay()
    {
        isSpawning = true;
        
        if (currentWave > 0)
        {
            Debug.Log($"⏳ Próxima wave em {timeBetweenWaves} segundos...");
            UpdateWaveUI($"Próxima wave em {timeBetweenWaves:F0}s");
            yield return new WaitForSeconds(timeBetweenWaves);
        }
        
        StartNextWave();
    }
    
    void StartNextWave()
    {
        currentWave++;
        Debug.Log($"📢 Wave {currentWave}/{totalWaves} começando! ({enemiesPerWave} inimigos)");
        
        UpdateWaveUI($"Wave {currentWave}/{totalWaves}");
        
        StartCoroutine(SpawnWave());
    }
    
    IEnumerator SpawnWave()
    {
        isSpawning = true;
        
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
        
        isSpawning = false;
    }
    
    void SpawnEnemy()
    {
        // Escolhe inimigo aleatório
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        
        // Escolhe posição de spawn
        Vector3 spawnPosition = GetSpawnPosition();
        
        // Spawna o inimigo
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Registra listener para quando morrer
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += OnEnemyDied;
        }
        else
        {
            // Fallback: procura outros componentes
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                // Você pode adicionar um evento OnDeath no seu script de Enemy
                Debug.LogWarning("⚠️ Enemy não tem EnemyHealth component! Contador de inimigos pode não funcionar.");
            }
        }
        
        enemiesAlive++;
    }
    
    Vector3 GetSpawnPosition()
    {
        if (useRandomSpawnInArea)
        {
            // Spawn aleatório em área definida
            float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            return new Vector3(x, y, 0);
        }
        else
        {
            // Spawn em pontos pré-definidos
            if (spawnPoints.Length > 0)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                return spawnPoint.position;
            }
            else
            {
                Debug.LogWarning("⚠️ Nenhum spawn point disponível! Usando posição (0,0)");
                return Vector3.zero;
            }
        }
    }
    
    // Callback quando inimigo morre
    public void OnEnemyDied()
    {
        enemiesAlive--;
        Debug.Log($"💀 Inimigo morreu! Restantes: {enemiesAlive}");
        
        // Atualiza UI
        UpdateWaveUI($"Wave {currentWave}/{totalWaves} - {enemiesAlive} restantes");
    }
    
    void OnAllWavesComplete()
    {
        allWavesComplete = true;
        Debug.Log("✅ Todas as waves completas! Room limpa!");
        
        UpdateWaveUI("Room Limpa!");
        
        // ATIVA A PORTA DA LOJA
        if (shopDoor != null)
        {
            shopDoor.SetActive(true);
            Debug.Log("🚪 Porta da loja ativada!");
        }
        else
        {
            Debug.LogWarning("⚠️ ShopDoor não encontrada!");
        }
    }
    
    void UpdateWaveUI(string text)
    {
        if (waveText != null)
        {
            waveText.text = text;
        }
    }
    
    // Gizmos para visualizar spawn area
    void OnDrawGizmosSelected()
    {
        if (useRandomSpawnInArea)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3(
                (spawnAreaMin.x + spawnAreaMax.x) / 2,
                (spawnAreaMin.y + spawnAreaMax.y) / 2,
                0
            );
            Vector3 size = new Vector3(
                spawnAreaMax.x - spawnAreaMin.x,
                spawnAreaMax.y - spawnAreaMin.y,
                0
            );
            Gizmos.DrawWireCube(center, size);
        }
        
        // Desenha spawn points
        if (spawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
        }
    }
}