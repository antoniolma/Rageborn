using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Item", menuName = "Rageborn/Shop Item")]
public class ShopItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    [TextArea(2, 4)]
    public string description;
    public Sprite itemSprite;
    
    [Header("Price")]
    public int price;
    
    [Header("Item Effect")]
    public ShopItemType itemType;
    public int value; // Quantidade de cura, dano, velocidade, etc
    
    [Header("Prefab")]
    public GameObject itemPrefab;
}

public enum ShopItemType
{
    HealthPotion,
    DamageBoost,
    SpeedBoost,
    MaxHealthIncrease,
    AttackSpeedBoost,
    WeaponChange
}