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
        if (player == null) return;
        
        switch (itemType)
        {
            case ShopItemType.HealthPotion:
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
                    Debug.Log($"⚔️ Dano aumentado em {itemValue}!");
                }
                break;
                
            case ShopItemType.SpeedBoost:
                if (PlayerStats.Instance != null)
                {
                    float speedIncrease = (float)itemValue / 10f;
                    PlayerStats.Instance.IncreaseSpeed(speedIncrease);
                    Debug.Log($"🏃 Velocidade aumentada em {speedIncrease}!");
                }
                break;
                
            case ShopItemType.MaxHealthIncrease:
                PlayerController controller = player.GetComponent<PlayerController>();
                if (controller != null)
                {
                    controller.IncreaseMaxHealth(itemValue);
                    Debug.Log($"❤️ Vida máxima aumentada em {itemValue}!");
                }
                break;
                
            case ShopItemType.AttackSpeedBoost:
                if (PlayerStats.Instance != null)
                {
                    PlayerStats.Instance.IncreaseAttackSpeed((float)itemValue);
                    Debug.Log($"⚡ Velocidade de ataque aumentada em {itemValue}!");
                }
                break;

            case ShopItemType.WeaponChange:
                PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    playerMovement.TypeSword = itemValue; // 0 = Fire, 1 = Ice, 2 = Venom
                    Debug.Log($"🗡️ Arma trocada para tipo {itemValue}!");
                }
                break;
        }
    }
}