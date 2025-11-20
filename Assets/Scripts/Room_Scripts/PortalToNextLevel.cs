using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Nome exato da cena a ser carregada (case-sensitive).")]
    public string targetSceneName = "Arena_Inferno";
    [Tooltip("Se true, teleporta automaticamente ao entrar no trigger sem pressionar tecla.")]
    public bool autoActivate = false;
    [Tooltip("Tecla para ativar o portal quando dentro do trigger.")]
    public KeyCode activateKey = KeyCode.E;
    [Tooltip("Tempo de espera (seg) depois do carregamento para spawnar ou animações.")]
    public float postLoadDelay = 0.1f;

    [Header("UI")]
    public GameObject promptUIPrefab; // opcional: prefab do texto "Press E"

    // runtime
    private bool playerInRange = false;
    private GameObject promptInstance;
    private GameObject playerObject;
    private bool isLoading = false;

    void Reset()
    {
        // garante trigger no Collider2D
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

        // Optionally disable player control if a script exists
        var controller = playerObject != null ? playerObject.GetComponent<MonoBehaviour>() : null;
        if (controller != null)
            controller.enabled = false; // WARNING: this disables that single component only

        // Sanitize scene name
        string sceneName = targetSceneName?.Trim().Replace("\"", "").Replace("'", "");
        Debug.Log($"[Portal] Carregando cena '{sceneName}'...");

        // Try load async and handle null / missing scene
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

        // opcional: mostrar tela de transição (fade out) aqui
        while (!asyncLoad.isDone)
            yield return null;

        yield return new WaitForSeconds(postLoadDelay);

        // opcional: reativa controles (se o player foi re-instantied, talvez precise buscar novo objeto)
        if (controller != null)
            controller.enabled = true;

        isLoading = false;
    }

    private void ShowPrompt(bool show)
    {
        if (promptUIPrefab == null) return;

        if (show)
        {
            if (promptInstance == null)
            {
                // cria no Canvas principal (procura por Canvas)
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                    promptInstance = Instantiate(promptUIPrefab, canvas.transform);
                else
                    promptInstance = Instantiate(promptUIPrefab);
            }
            promptInstance.SetActive(true);
        }
        else
        {
            if (promptInstance != null)
                promptInstance.SetActive(false);
        }
    }
}
