using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    
    [Header("Combat Stats")]
    [SerializeField] private int baseDamage = 10;
    private int currentDamageBonus = 0;
    
    [Header("Movement Stats")]
    [SerializeField] private float baseSpeed = 8f;
    private float currentSpeedBonus = 0f;
    
    [Header("Attack Speed Stats")]
    [SerializeField] private float baseAttackSpeed = 1f;
    private float currentAttackSpeedBonus = 0f;
    
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
    
    public int GetTotalDamage()
    {
        return baseDamage + currentDamageBonus;
    }
    
    public float GetTotalSpeed()
    {
        return baseSpeed + currentSpeedBonus;
    }
    
    public float GetTotalAttackSpeed()
    {
        return baseAttackSpeed + currentAttackSpeedBonus;
    }
    
    public void IncreaseDamage(int amount)
    {
        currentDamageBonus += amount;
        Debug.Log($"⚔️ Dano aumentado em {amount}! Dano total: {GetTotalDamage()}");
    }
    
    public void IncreaseSpeed(float amount)
    {
        currentSpeedBonus += amount;
        Debug.Log($"🏃 Velocidade aumentada em {amount}! Velocidade total: {GetTotalSpeed()}");
    }
    
    public void IncreaseAttackSpeed(float amount)
    {
        currentAttackSpeedBonus += amount;
        Debug.Log($"⚡ Velocidade de ataque aumentada em {amount}! Total: {GetTotalAttackSpeed()}");
    }
    
    public void ResetStats()
    {
        currentDamageBonus = 0;
        currentSpeedBonus = 0;
        currentAttackSpeedBonus = 0;
    }
}