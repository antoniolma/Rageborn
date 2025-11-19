using UnityEngine;
using TMPro;

public class ShopDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string promptText = "Entrar na Loja";
    
    [Header("UI")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI promptTextUI;
    
    private bool playerInRange = false;
    
    void Start()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
            
        if (promptTextUI != null)
            promptTextUI.text = $"[{interactKey}] {promptText}";
    }
    
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            EnterShop();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }
    
    void EnterShop()
    {
        Debug.Log("🚪 Entrando na loja...");
        
        RoomManager roomManager = FindObjectOfType<RoomManager>();
        if (roomManager != null)
        {
            roomManager.TransitionToShop();
        }
    }
}