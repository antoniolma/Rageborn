using UnityEngine;

public class PortalToNextLevel : InteractableItem
{
    [Header("Portal Settings")]
    [SerializeField] private Animator portalAnimator;
    [SerializeField] private ParticleSystem portalParticles;
    
    protected override void Start()
    {
        base.Start();
        price = 0; // Portal é sempre gratuito
        itemName = "Portal";
        description = "Avançar para o próximo nível";
        UpdatePromptText();
    }
    
    protected override void OnPurchaseSuccess()
    {
        base.OnPurchaseSuccess();
        
        Debug.Log("🌀 Avançando para o próximo nível...");
        
        // Efeito de transição
        if (portalAnimator != null)
        {
            portalAnimator.SetTrigger("Activate");
        }
        
        // Carrega o próximo nível
        RoomManager roomManager = FindObjectOfType<RoomManager>();
        if (roomManager != null)
        {
            roomManager.ExitShopAndLoadNextRoom();
        }
    }
}