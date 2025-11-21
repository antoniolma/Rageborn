using UnityEngine;
using TMPro;

public class InteractableItem : MonoBehaviour
{
    [Header("Item Info")]
    protected string itemName;
    protected string description;
    protected int price;
    protected ShopItemType itemType;
    protected int itemValue;
    
    [Header("Interaction Settings")]
    [SerializeField] protected float interactionRange = 2f;
    [SerializeField] protected KeyCode interactKey = KeyCode.E;
    
    [Header("UI References")]
    [SerializeField] protected GameObject interactionPrompt;
    [SerializeField] protected TextMeshProUGUI promptText;
    [SerializeField] protected GameObject itemInfoCanvas;
    [SerializeField] protected TextMeshProUGUI itemNameText;
    [SerializeField] protected TextMeshProUGUI itemDescriptionText;
    
    [Header("Visual")]
    [SerializeField] protected GameObject itemVisual;
    
    protected Transform player;
    protected bool playerInRange = false;
    protected bool isPurchased = false;
    protected SpriteRenderer itemSpriteRenderer;
    
    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        itemSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
            
        if (itemInfoCanvas != null)
            itemInfoCanvas.SetActive(false);
            
        UpdatePromptText();
    }
    
    protected virtual void Update()
    {
        if (player == null || isPurchased) return;
        
        float distance = Vector2.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= interactionRange;
        
        // ✅ Atualiza UI quando o player entra ou sai do range
        if (playerInRange != wasInRange)
        {
            ShowInteractionPrompt(playerInRange);
            ShowItemInfo(playerInRange);
        }
        
        if (playerInRange && Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }
    
    public virtual void InitializeItem(ShopItemData data)
    {
        itemName = data.itemName;
        description = data.description;
        price = data.price;
        itemType = data.itemType;
        itemValue = data.value;
        
        // Atualiza o sprite do item
        if (itemSpriteRenderer == null)
        {
            itemSpriteRenderer = GetComponent<SpriteRenderer>();
            if (itemSpriteRenderer == null)
                itemSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        // Atualiza o sprite do item
        if (itemSpriteRenderer != null && data.itemSprite != null)
        {
            itemSpriteRenderer.sprite = data.itemSprite;
            Debug.Log($"✅ Sprite atribuído: {data.itemSprite.name}");
        }
        else
        {
            Debug.LogWarning($"❌ Sprite não atribuído! SpriteRenderer: {itemSpriteRenderer != null}, Sprite: {data.itemSprite != null}");
        }
        
        UpdatePromptText();
        UpdateItemInfo();
    }
    
    protected virtual void TryInteract()
    {
        if (price == 0) // Item gratuito
        {
            OnPurchaseSuccess();
        }
        else if (CurrencyManager.Instance.SpendRunCoins(price))
        {
            OnPurchaseSuccess();
        }
        else
        {
            OnPurchaseFailed();
        }
    }
    
    protected virtual void OnPurchaseSuccess()
    {
        isPurchased = true;
        ShowInteractionPrompt(false);
        ShowItemInfo(false);
    }
    
    protected virtual void OnPurchaseFailed()
    {
        Debug.Log("❌ Moedas insuficientes!");
    }
    
    // ✅ NOVO MÉTODO - Mostra/esconde o prompt de interação
    protected virtual void ShowInteractionPrompt(bool show)
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(show);
    }
    
    // ✅ NOVO MÉTODO - Mostra/esconde a caixinha de informações
    protected virtual void ShowItemInfo(bool show)
    {
        if (itemInfoCanvas != null)
            itemInfoCanvas.SetActive(show);
    }
    
    protected virtual void UpdatePromptText()
    {
        if (promptText != null)
        {
            if (price == 0)
                promptText.text = $"[{interactKey}]";
            else
                promptText.text = $"[{interactKey}] {price} moedas";
        }
    }
    
    protected virtual void UpdateItemInfo()
    {
        if (itemNameText != null)
            itemNameText.text = itemName;
            
        if (itemDescriptionText != null)
            itemDescriptionText.text = description;
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}