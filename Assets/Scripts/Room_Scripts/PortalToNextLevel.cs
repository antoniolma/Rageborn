using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Se true, usa o RoomManager para carregar próxima room. Se false, carrega targetSceneName manualmente.")]
    public bool useRoomManager = true;
    
    [Tooltip("Nome exato da cena a ser carregada (case-sensitive). Só usado se useRoomManager = false")]
    public string targetSceneName = "Arena_Inferno";
    
    [Tooltip("Se true, teleporta automaticamente ao entrar no trigger sem pressionar tecla.")]
    public bool autoActivate = false;
    
    [Tooltip("Tecla para ativar o portal quando dentro do trigger.")]
    public KeyCode activateKey = KeyCode.E;
    
    [Tooltip("Tempo de espera (seg) depois do carregamento para spawnar ou animações.")]
    public float postLoadDelay = 0.1f;

    [Header("UI")]
    public GameObject interactionCanvas; // Referência ao Canvas filho

    // runtime
    private bool playerInRange = false;
    private GameObject playerObject;
    private bool isLoading = false;

    void Start()
    {
        // Se não atribuiu manualmente, tenta achar o canvas filho
        if (interactionCanvas == null)
        {
            interactionCanvas = transform.Find("InteractionCanvas")?.gameObject;
        }
        
        // Começa escondido
        if (interactionCanvas != null)
            interactionCanvas.SetActive(false);
    }

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading) return;

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerObject = other.gameObject;
            ShowPrompt(true);

            if (autoActivate)
                StartCoroutine(ActivatePortal());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            ShowPrompt(false);
            playerObject = null;
        }
    }

    void Update()
    {
        if (isLoading) return;

        if (playerInRange && !autoActivate && Input.GetKeyDown(activateKey))
        {
            StartCoroutine(ActivatePortal());
        }
    }

    IEnumerator ActivatePortal()
    {
        if (isLoading) yield break;
        isLoading = true;
        ShowPrompt(false);

        var controller = playerObject != null ? playerObject.GetComponent<MonoBehaviour>() : null;
        if (controller != null)
            controller.enabled = false;

        // ✅ NOVO - Usa RoomManager se disponível
        if (useRoomManager && RoomManager.Instance != null)
        {
            Debug.Log($"[Portal] Carregando próxima room via RoomManager...");
            
            yield return new WaitForSeconds(postLoadDelay);
            
            // Chama o RoomManager para carregar a próxima room
            RoomManager.Instance.LoadNextRoom();
            
            // O RoomManager cuida do resto, então só reabilitamos o controller
            if (controller != null)
                controller.enabled = true;
                
            isLoading = false;
            yield break;
        }

        // ✅ FALLBACK - Carrega scene manualmente se RoomManager não estiver disponível
        string sceneName = targetSceneName?.Trim().Replace("\"", "").Replace("'", "");
        Debug.Log($"[Portal] Carregando cena '{sceneName}' manualmente...");

        AsyncOperation asyncLoad = null;
        try
        {
            asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Portal] Exceção ao chamar LoadSceneAsync('{sceneName}'): {ex.Message}");
        }

        if (asyncLoad == null)
        {
            Debug.LogError($"[Portal] Falha ao iniciar carregamento de cena '{sceneName}'. Verifique Build Settings.");
            if (controller != null) controller.enabled = true;
            isLoading = false;
            yield break;
        }

        while (!asyncLoad.isDone)
            yield return null;

        yield return new WaitForSeconds(postLoadDelay);

        if (controller != null)
            controller.enabled = true;

        isLoading = false;
    }

    private void ShowPrompt(bool show)
    {
        if (interactionCanvas != null)
            interactionCanvas.SetActive(show);
    }
}