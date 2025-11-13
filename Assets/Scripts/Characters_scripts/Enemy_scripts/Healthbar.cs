using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    
    [Header("Color Settings")]
    [SerializeField] private Color highHealthColor = Color.green;
    [SerializeField] private Color mediumHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;
    
    [Header("Animation")]
    [SerializeField] private float lerpSpeed = 5f;
    
    private float targetHealth;
    private float maxHealth;
    
    void Start()
    {
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }
        
        if (fillImage == null && healthSlider != null)
        {
            fillImage = healthSlider.fillRect.GetComponent<Image>();
        }
    }
    
    void Update()
    {
        // Anima suavemente a barra de vida
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealth, lerpSpeed * Time.deltaTime);
            UpdateHealthBarColor();
        }
    }
    
    public void SetMaxHealth(float max)
    {
        maxHealth = max;
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = max;
        }
        targetHealth = max;
    }
    
    public void SetHealth(float health)
    {
        targetHealth = Mathf.Clamp(health, 0, maxHealth);
    }
    
    private void UpdateHealthBarColor()
    {
        if (fillImage == null) return;
        
        float healthPercentage = healthSlider.value / healthSlider.maxValue;
        
        if (healthPercentage > 0.5f)
        {
            fillImage.color = Color.Lerp(mediumHealthColor, highHealthColor, (healthPercentage - 0.5f) * 2);
        }
        else
        {
            fillImage.color = Color.Lerp(lowHealthColor, mediumHealthColor, healthPercentage * 2);
        }
    }
}