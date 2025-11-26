using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject victoryPanel;

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Bosses")]
    [SerializeField, Tooltip("Quantos bosses precisam morrer para ativar vitória")] 
    private int bossesNeededToWin = 2;

    private int bossesKilled = 0;
    private bool hasWon = false;

    public static VictoryManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // se quiser que persista entre cenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("VictoryManager: victoryPanel não está atribuído no Inspector.");
        }
    }

    // Método público chamado pelos bosses quando morrem
    public void NotifyBossDeath()
    {
        if (hasWon) return;

        bossesKilled++;
        Debug.Log($"VictoryManager: boss notificado morto. Total mortos: {bossesKilled}/{bossesNeededToWin}");

        if (bossesKilled >= Mathf.Max(1, bossesNeededToWin))
        {
            TriggerVictory();
        }
    }

    // Mantive seu Trigger original (para chamadas manuais / debug)
    public void TriggerVictory()
    {
        if (hasWon) return;

        hasWon = true;
        Debug.Log("🏆 Você Venceu!");

        // Pausa o jogo
        Time.timeScale = 0f;

        // Mostra o painel
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Utilitários opcionais
    public void ResetCount()
    {
        bossesKilled = 0;
        hasWon = false;
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    public int GetKills() => bossesKilled;
}
