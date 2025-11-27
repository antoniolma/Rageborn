using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VictoryManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Button returnToMainMenuButton; // opcional: pode deixar nulo e o script procura no painel

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
            Debug.Log($"VictoryManager: Instance criada ({gameObject.name}).");
        }
        else
        {
            Debug.Log($"VictoryManager: Instância duplicada detectada em {gameObject.name}. Destruindo esta instância.");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        Debug.Log("VictoryManager: Start iniciado.");

        // Se o painel foi passado, escondemos inicialmente
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("VictoryManager: victoryPanel NÃO atribuído no Inspector. Tentarei encontrar automaticamente.");
        }

        // Se o botão não foi setado no Inspector, tenta achar dentro do panel
        if (returnToMainMenuButton == null && victoryPanel != null)
        {
            returnToMainMenuButton = victoryPanel.GetComponentInChildren<Button>(true);
            if (returnToMainMenuButton != null)
                Debug.Log($"VictoryManager: botão encontrado automaticamente: {returnToMainMenuButton.name}");
            else
                Debug.LogWarning("VictoryManager: nenhum Button encontrado dentro do victoryPanel (GetComponentInChildren).");
        }

        // Se ainda não tem botão, tenta achar por nome na cena (fallback)
        if (returnToMainMenuButton == null)
        {
            var anyBtn = FindObjectOfType<Button>();
            if (anyBtn != null)
            {
                returnToMainMenuButton = anyBtn;
                Debug.LogWarning($"VictoryManager: nenhum botão específico setado — usando o primeiro Button encontrado na cena: {anyBtn.name}");
            }
        }

        // Se achou o botão, garante listener por código e adiciona fallback component
        if (returnToMainMenuButton != null)
        {
            // limpa listeners antigos (evita referências a prefabs/inativas)
            returnToMainMenuButton.onClick.RemoveAllListeners();
            returnToMainMenuButton.onClick.AddListener(() =>
            {
                Debug.Log("VictoryManager: listener (onClick) do botão acionado.");
                ReturnToMainMenu();
            });

            // adiciona componente auxiliar para reforçar detecção de clique (se já não existir)
            var aux = returnToMainMenuButton.gameObject.GetComponent<AlwaysWorkingReturnButton>();
            if (aux == null)
            {
                aux = returnToMainMenuButton.gameObject.AddComponent<AlwaysWorkingReturnButton>();
                aux.fallbackMainMenuSceneName = mainMenuSceneName;
                Debug.Log("VictoryManager: AlwaysWorkingReturnButton adicionado ao botão como fallback.");
            }
        }
        else
        {
            Debug.LogWarning("VictoryManager: Não foi possível configurar um Button automático. Use ButtonDebugger ou verifique a cena.");
        }

        // Small sanity log
        Debug.Log($"VictoryManager: configuração completa. bossesNeededToWin={bossesNeededToWin}");
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

    // Ativa vitória: mostra painel e pausa jogo, mas não depende do botão funcionar
    public void TriggerVictory()
    {
        if (hasWon) return;

        hasWon = true;
        Debug.Log("VictoryManager: 🏆 Vitória ativada!");

        Time.timeScale = 0f;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            // Garante que painel esteja interativo (caso CanvasGroup bloqueie)
            var cg = victoryPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
    }

    // Chamado pelo botão
    public void ReturnToMainMenu()
    {
        Debug.Log("VictoryManager: ReturnToMainMenu chamado, tentando carregar cena: " + mainMenuSceneName);

        // Garante que o jogo volte a rodar
        Time.timeScale = 1f;

        // Verifica se a cena está na BuildSettings (checagem leve)
#if UNITY_EDITOR
        // No Editor essa checagem é apenas um aviso (não complica a execução)
#endif
        try
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("VictoryManager: erro ao carregar cena: " + ex.Message);
        }
    }

    public void ResetCount()
    {
        bossesKilled = 0;
        hasWon = false;
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    public int GetKills() => bossesKilled;
}

/// <summary>
/// AlwaysWorkingReturnButton:
/// - Implementa IPointerClickHandler para pegar cliques via EventSystem.
/// - Faz fallback de detecção via RectangleContainsScreenPoint no Update() (funciona mesmo com Time.timeScale = 0).
/// - Chama VictoryManager.Instance ou carrega cena fallback.
/// </summary>
public class AlwaysWorkingReturnButton : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public string fallbackMainMenuSceneName = "MainMenu";
    private RectTransform rect;
    private Canvas rootCanvas;
    private bool isHandlingClick = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        rootCanvas = rect != null ? rect.GetComponentInParent<Canvas>() : null;
        Debug.Log("AlwaysWorkingReturnButton: Awake - pronto para detectar cliques no botão " + gameObject.name);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("AlwaysWorkingReturnButton: OnPointerClick recebido.");
        SafeReturnToMainMenu();
    }

    void Update()
    {
        // Backup click detection via RectTransform (ignora Time.timeScale)
        if (isHandlingClick) return;

        if (rect != null && Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            Camera cam = null;
            if (rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = rootCanvas.worldCamera != null ? rootCanvas.worldCamera : Camera.main;

            if (RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos, cam))
            {
                Debug.Log("AlwaysWorkingReturnButton: clique detectado via Update(RectTransform).");
                isHandlingClick = true;
                SafeReturnToMainMenu();
            }
        }

        if (isHandlingClick && Input.GetMouseButtonUp(0))
            isHandlingClick = false;
    }

    private void SafeReturnToMainMenu()
    {
        // Tenta singleton
        if (VictoryManager.Instance != null)
        {
            Debug.Log("AlwaysWorkingReturnButton: chamando VictoryManager.Instance.ReturnToMainMenu()");
            VictoryManager.Instance.ReturnToMainMenu();
            return;
        }

        // Tenta achar qualquer VictoryManager
        var vm = FindObjectOfType<VictoryManager>();
        if (vm != null)
        {
            Debug.Log("AlwaysWorkingReturnButton: encontrou VictoryManager na cena e chamando ReturnToMainMenu()");
            vm.ReturnToMainMenu();
            return;
        }

        // Fallback direto para carregar cena
        Debug.LogWarning("AlwaysWorkingReturnButton: VictoryManager não encontrado. Carregando cena fallback: " + fallbackMainMenuSceneName);
        Time.timeScale = 1f;
        try
        {
            SceneManager.LoadScene(fallbackMainMenuSceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("AlwaysWorkingReturnButton: falha ao carregar cena de fallback: " + ex.Message);
        }
    }
}
