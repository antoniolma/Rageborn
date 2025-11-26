using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    private bool isGameOver = false;
    
    public static GameOverManager Instance;
    
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
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    public void TriggerGameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        Debug.Log("💀 Game Over!");
        
        // Pausa o jogo
        Time.timeScale = 0f;
        
        // Mostra o painel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}