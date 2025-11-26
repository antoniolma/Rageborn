using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject victoryPanel;
    
    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    private bool hasWon = false;
    
    public static VictoryManager Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
    }
    
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
}