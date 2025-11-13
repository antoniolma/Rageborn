using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;
    
    [Header("Spawn Settings")]
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private float spawnDelay = 2f; // Delay entre spawns
    [SerializeField] private float timeBetweenWaves = 10f; // Tempo entre ondas
    
    [Header("Spawn Area")]
    [SerializeField] private float spawnRadius = 10f; // Raio de spawn ao redor do spawner
    [SerializeField] private Vector2 spawnAreaCenter; // Centro da área de spawn
    [SerializeField] private bool useSpawnerPosition = true; // Usar posição deste GameObject
    
    [Header("Wave Progression")]
    [SerializeField] private bool increaseEnemiesPerWave = true;
    [SerializeField] private int enemyIncreasePerWave = 2;
    [SerializeField] private int maxEnemiesPerWave = 20;
    
    [Header("Debug")]
    [SerializeField] private bool showSpawnArea = true;
    [SerializeField] private Color gizmoColor = Color.red;
    
    private int currentWave = 0;
    private int enemiesAlive = 0;
    private bool isSpawning = false;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    
    void Start()
    {
        if (useSpawnerPosition)
        {
            spawnAreaCenter = transform.position;
        }
        
        if (enemyPrefabs.Length == 0)
        {
            Debug.LogError("❌ Nenhum Enemy Prefab configurado no EnemySpawner!");
            return;
        }
        
        Debug.Log("✅ EnemySpawner inicializado!");
        StartCoroutine(SpawnWaves());
    }
    
    void Update()
    {
        // Remove inimigos mortos da lista
        spawnedEnemies.RemoveAll(enemy => enemy == null);
        enemiesAlive = spawnedEnemies.Count;
    }
    
    private IEnumerator SpawnWaves()
    {
        while (true)
        {
            currentWave++;
            int enemiesToSpawn = CalculateEnemiesForWave();
            
            Debug.Log($"🌊 Onda {currentWave} começou! Inimigos: {enemiesToSpawn}");
            
            yield return StartCoroutine(SpawnWave(enemiesToSpawn));
            
            Debug.Log($"✅ Onda {currentWave} completa! Aguardando próxima onda...");
            
            // Aguarda até todos os inimigos morrerem ou tempo acabar
            float waitTime = 0f;
            while (enemiesAlive > 0 && waitTime < timeBetweenWaves)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }
            
            // Tempo extra entre ondas
            yield return new WaitForSeconds(3f);
        }
    }
    
    private IEnumerator SpawnWave(int enemyCount)
    {
        isSpawning = true;
        
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnDelay);
        }
        
        isSpawning = false;
    }
    
    private void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0) return;
        
        // Escolhe um prefab aleatório
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        
        // Calcula posição aleatória dentro do raio
        Vector2 randomPosition = GetRandomSpawnPosition();
        
        // Spawna o inimigo
        GameObject enemy = Instantiate(enemyPrefab, randomPosition, Quaternion.identity);
        spawnedEnemies.Add(enemy);
        
        Debug.Log($"👾 Enemy spawned em {randomPosition}");
    }
    
    private Vector2 GetRandomSpawnPosition()
    {
        // Gera posição aleatória em um círculo
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        return spawnAreaCenter + randomOffset;
    }
    
    private int CalculateEnemiesForWave()
    {
        if (!increaseEnemiesPerWave)
        {
            return enemiesPerWave;
        }
        
        int enemies = enemiesPerWave + (enemyIncreasePerWave * (currentWave - 1));
        return Mathf.Min(enemies, maxEnemiesPerWave);
    }
    
    // Método público para spawnar um inimigo manualmente
    public void SpawnEnemyManual()
    {
        SpawnEnemy();
    }
    
    // Método para pausar/retomar spawning
    public void StopSpawning()
    {
        StopAllCoroutines();
        isSpawning = false;
        Debug.Log("⏸️ Spawning pausado!");
    }
    
    public void ResumeSpawning()
    {
        if (!isSpawning)
        {
            StartCoroutine(SpawnWaves());
            Debug.Log("▶️ Spawning retomado!");
        }
    }
    
    // Getters
    public int GetCurrentWave() => currentWave;
    public int GetEnemiesAlive() => enemiesAlive;
    public bool IsSpawning() => isSpawning;
    
    // Desenha a área de spawn no Editor
    void OnDrawGizmos()
    {
        if (!showSpawnArea) return;
        
        Gizmos.color = gizmoColor;
        Vector2 center = useSpawnerPosition ? (Vector2)transform.position : spawnAreaCenter;
        Gizmos.DrawWireSphere(center, spawnRadius);
    }
}