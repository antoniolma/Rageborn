
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [Header("Room Scenes")]
    [SerializeField] private List<string> normalRoomScenes = new List<string>();
    [SerializeField] private string shopRoomScene = "ShopRoom";
    [SerializeField] private string bossRoomScene = ""; // ✅ Pode ficar vazio

    [Header("Room Flow Settings")]
    [SerializeField] private int roomsBeforeShop = 2;

    [Header("Player Persistence")]
    [SerializeField] private Vector2 playerSpawnOffset = new Vector2(5, 5);

    private List<string> roomSequence = new List<string>();
    private int currentRoomIndex = 0;
    private int totalRoomsCleared = 0;

    // Dados do player para persistir entre scenes
    private int playerHealth;
    private int playerMaxHealth;
    private int runCoins;

    // ✅ Flag para controlar se é a primeira room
    private bool isFirstLoad = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        GenerateRoomSequence();
        // ✅ NÃO carrega automaticamente - espera o botão iniciar
    }

    void GenerateRoomSequence()
    {
        roomSequence.Clear();

        // Valida se temos rooms configuradas
        if (normalRoomScenes.Count == 0)
        {
            Debug.LogWarning("⚠️ Nenhuma room scene configurada no RoomManager!");
            return;
        }

        // Cria lista com todas as rooms disponíveis
        List<string> availableRooms = new List<string>(normalRoomScenes);

        // Embaralha
        ShuffleList(availableRooms);

        // Monta a sequência: Room → Room → Shop → Room → Room → Shop... (→ Boss opcional)
        for (int i = 0; i < availableRooms.Count; i++)
        {
            roomSequence.Add(availableRooms[i]);

            // A cada X rooms, adiciona um shop (se existir)
            if ((i + 1) % roomsBeforeShop == 0 && i < availableRooms.Count - 1)
            {
                if (!string.IsNullOrEmpty(shopRoomScene))
                {
                    roomSequence.Add(shopRoomScene);
                }
            }
        }

        // Adiciona o boss no final (se existir)
        if (!string.IsNullOrEmpty(bossRoomScene))
        {
            roomSequence.Add(bossRoomScene);
        }

        Debug.Log($"📋 Sequência de rooms gerada: {roomSequence.Count} rooms no total");
        for (int i = 0; i < roomSequence.Count; i++)
        {
            Debug.Log($"  {i + 1}. {roomSequence[i]}");
        }
    }

    // ✅ NOVO - Método público para iniciar o jogo (chamado pelo botão)
    public void StartGame()
    {
        if (roomSequence.Count > 0)
        {
            isFirstLoad = true;
            LoadRoom(0);
        }
        else
        {
            Debug.LogError("❌ Sequência de rooms está vazia!");
        }
    }

    void LoadRoom(int index)
    {
        if (index < 0 || index >= roomSequence.Count)
        {
            Debug.LogError($"❌ Índice de room inválido: {index}");
            return;
        }

        currentRoomIndex = index;
        string sceneName = roomSequence[index];

        // ✅ Só salva dados do player se NÃO for a primeira vez
        if (!isFirstLoad)
        {
            SavePlayerData();
        }

        Debug.Log($"🚪 Carregando room {currentRoomIndex + 1}/{roomSequence.Count}: {sceneName}");

        // Carrega a nova scene
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        // Carrega a scene de forma assíncrona
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Small wait to let scene objects Awake/Start
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.05f);

        // Quando a scene carregar, posiciona player
        // ✅ Na primeira vez, instancia / reposiciona o player
        if (isFirstLoad)
        {
            SpawnPlayer();
            isFirstLoad = false;
        }
        else
        {
            RestorePlayerData();
            // Guarantee that player exists before positioning
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                StartCoroutine(EnsurePositionAfterFrames(player, 1));
            }
            else
            {
                Debug.LogWarning("⚠️ Player não encontrado após carregar a cena (LoadSceneAsync).");
            }
        }
    }

    // ✅ NOVO - Instancia o player na primeira vez
    void SpawnPlayer()
    {
        // Procura por um player já existente (persistente)
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");

        if (existingPlayer != null)
        {
            Debug.Log("✅ Player persistente encontrado. Irei posicioná-lo no spawn da cena.");
            StartCoroutine(EnsurePositionAfterFrames(existingPlayer, 1));
        }
        else
        {
            // Se não houver player persistente, procura por um Player na cena (prefab)
            GameObject scenePlayer = GameObject.FindGameObjectWithTag("Player");
            if (scenePlayer != null)
            {
                Debug.Log("✅ Player local da cena encontrado. Posicionando-o no spawn.");
                StartCoroutine(EnsurePositionAfterFrames(scenePlayer, 1));
            }
            else
            {
                Debug.LogWarning("⚠️ Nenhum GameObject com tag 'Player' encontrado na cena ou persistente!");
            }
        }
    }

    // Helper coroutine para esperar um ou mais frames e então posicionar (evita problemas de timing)
    IEnumerator EnsurePositionAfterFrames(GameObject player, int framesToWait = 1)
    {
        for (int i = 0; i < framesToWait; i++)
            yield return new WaitForEndOfFrame();
        PositionPlayer(player);
    }

    // Mantém compatibilidade com chamadas antigas
    void PositionPlayer(GameObject player = null)
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("⚠️ Nenhum player encontrado para posicionar!");
            return;
        }

        // Primeiro tenta encontrar o componente PlayerSpawnPoint na cena (mais robusto que a tag)
        PlayerSpawnPoint spawnPointComp = FindObjectOfType<PlayerSpawnPoint>();
        if (spawnPointComp != null)
        {
            player.transform.position = spawnPointComp.transform.position;
            Debug.Log($"✅ Player posicionado no PlayerSpawnPoint: {spawnPointComp.transform.position}");
            return;
        }

        // Fallback para procurar por tag "PlayerSpawn" (compatibilidade)
        GameObject spawnObject = GameObject.FindGameObjectWithTag("PlayerSpawn");
        if (spawnObject != null)
        {
            player.transform.position = spawnObject.transform.position;
            Debug.Log($"✅ Player posicionado no spawn por tag: {spawnObject.transform.position}");
            return;
        }

        // último recurso: usa offset padrão
        player.transform.position = (Vector2)Vector2.zero + playerSpawnOffset;
        Debug.LogWarning("⚠️ Spawn point não encontrado (nem PlayerSpawnPoint nem Tag). Usando posição padrão.");
    }

    void SavePlayerData()
    {
        // Salva vida do player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerHealth = playerController.GetCurrentHealth();
                playerMaxHealth = playerController.GetMaxHealth();
            }
        }

        // Salva moedas
        if (CurrencyManager.Instance != null)
        {
            runCoins = CurrencyManager.Instance.GetRunCoins();
        }
    }

    void RestorePlayerData()
    {
        // Restaura vida do player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null && playerHealth > 0)
            {
                Debug.Log($"❤️ Player health restaurado: {playerHealth}/{playerMaxHealth}");
            }
        }
    }

    public void LoadNextRoom()
    {
        totalRoomsCleared++;

        int nextIndex = currentRoomIndex + 1;

        if (nextIndex < roomSequence.Count)
        {
            LoadRoom(nextIndex);
        }
        else
        {
            Debug.Log("🎉 Todas as rooms foram completadas!");
            OnRunCompleted();
        }
    }

    public void TransitionToShop()
    {
        Debug.Log("🛒 Transicionando para o shop...");
        LoadNextRoom();
    }

    public void ExitShopAndLoadNextRoom()
    {
        Debug.Log("🚪 Saindo do shop...");
        LoadNextRoom();
    }

    void OnRunCompleted()
    {
        Debug.Log("🏆 Run completada! Voltando ao menu...");
        // Aqui você pode carregar a scene de vitória ou menu
        // SceneManager.LoadScene("VictoryScreen");
        // SceneManager.LoadScene("MainMenu");
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

    // Métodos públicos para acessar informações
    public bool IsCurrentRoomShop()
    {
        if (currentRoomIndex < 0 || currentRoomIndex >= roomSequence.Count) return false;
        return roomSequence[currentRoomIndex] == shopRoomScene;
    }

    public bool IsCurrentRoomBoss()
    {
        if (currentRoomIndex < 0 || currentRoomIndex >= roomSequence.Count) return false;
        return roomSequence[currentRoomIndex] == bossRoomScene;
    }

    public int GetCurrentRoomNumber()
    {
        return currentRoomIndex + 1;
    }

    public int GetTotalRooms()
    {
        return roomSequence.Count;
    }

    public string GetCurrentRoomName()
    {
        if (currentRoomIndex < 0 || currentRoomIndex >= roomSequence.Count) return "Unknown";
        return roomSequence[currentRoomIndex];
    }
}