using UnityEngine;

public class GenericShopItem : InteractableItem
{
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem purchaseEffect;
    
    protected override void OnPurchaseSuccess()
    {
        base.OnPurchaseSuccess();
        
        ApplyItemEffect();
        
        // Efeito visual de compra
        if (purchaseEffect != null)
        {
            Instantiate(purchaseEffect, transform.position, Quaternion.identity);
        }
        
        // Desativa o visual
        if (itemVisual != null)
        {
            itemVisual.SetActive(false);
        }
        else if (itemSpriteRenderer != null)
        {
            itemSpriteRenderer.enabled = false;
        }
        
        Destroy(gameObject, 0.5f);
    }
    
    void ApplyItemEffect()
    {
        switch (itemType)
        {
            case ShopItemType.HealthPotion:
                // ✅ CORRIGIDO - usa PlayerController ao invés de PlayerHealth
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.Heal(itemValue);
                    Debug.Log($"❤️ Curou {itemValue} de vida!");
                }
                break;
                
            case ShopItemType.DamageBoost:
                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.IncreaseDamage(itemValue);
                }
                break;
                
            case ShopItemType.SpeedBoost:
                if (PlayerStats.Instance != null)
                {
                    // ✅ Converte int para float
                    PlayerStats.Instance.IncreaseSpeed((float)itemValue);
                }
                break;
                
            case ShopItemType.MaxHealthIncrease:
                // ✅ CORRIGIDO - usa PlayerController
                PlayerController controller = player.GetComponent<PlayerController>();
                if (controller != null)
                {
                    controller.IncreaseMaxHealth(itemValue);
                }
                break;
                
            case ShopItemType.AttackSpeedBoost:
                if (PlayerStats.Instance != null)
                {
                    // ✅ Converte int para float
                    PlayerStats.Instance.IncreaseAttackSpeed((float)itemValue);
                }
                break;
        }
    }
}