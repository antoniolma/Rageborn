using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Health Reference")]
    [SerializeField] private EnemyHealth enemyHealth;
    
    [Header("UI Elements")]
    [SerializeField] private Canvas healthBarCanvas;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image healthBarBackground;
    
    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private bool alwaysFaceCamera = true;
    
    [Header("Colors")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color mediumHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float mediumHealthThreshold = 0.6f;
    [SerializeField] private float lowHealthThreshold = 0.3f;
    
    [Header("Animation")]
    [SerializeField] private bool enableSmoothTransition = true;
    [SerializeField] private float transitionSpeed = 5f;
    
    private Camera mainCamera;
    private float targetFillAmount;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Encontra EnemyHealth se não foi atribuído
        if (enemyHealth == null)
        {
            enemyHealth = GetComponentInParent<EnemyHealth>();
        }
        
        if (enemyHealth == null)
        {
            Debug.LogError("❌ EnemyHealth não encontrado! HealthBar não funcionará.");
            enabled = false;
            return;
        }
        
        // Configura canvas inicial
        if (healthBarCanvas != null)
        {
            healthBarCanvas.worldCamera = mainCamera;
        }
        
        // Inicializa barra cheia
        UpdateHealthBar();
        
        // Esconde se estiver cheia e hideWhenFull ativo
        if (hideWhenFull && enemyHealth.GetHealthPercentage() >= 1f)
        {
            healthBarCanvas.gameObject.SetActive(false);
        }
    }
    
    void Update()
    {
        if (enemyHealth == null) return;
        
        // Atualiza posição da barra
        UpdatePosition();
        
        // Faz barra sempre olhar para câmera
        if (alwaysFaceCamera && mainCamera != null)
        {
            healthBarCanvas.transform.LookAt(
                healthBarCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up
            );
        }
        
        // Atualiza barra
        UpdateHealthBar();
    }
    
    void UpdatePosition()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.transform.position = transform.position + offset;
        }
    }
    
    void UpdateHealthBar()
    {
        float healthPercentage = enemyHealth.GetHealthPercentage();
        targetFillAmount = healthPercentage;
        
        // Mostra barra se não estiver cheia
        if (hideWhenFull && healthPercentage < 1f)
        {
            if (!healthBarCanvas.gameObject.activeSelf)
            {
                healthBarCanvas.gameObject.SetActive(true);
            }
        }
        
        // Atualiza fill amount
        if (enableSmoothTransition)
        {
            healthBarFill.fillAmount = Mathf.Lerp(
                healthBarFill.fillAmount,
                targetFillAmount,
                Time.deltaTime * transitionSpeed
            );
        }
        else
        {
            healthBarFill.fillAmount = targetFillAmount;
        }
        
        // Atualiza cor baseada na vida
        UpdateHealthColor(healthPercentage);
    }
    
    void UpdateHealthColor(float healthPercentage)
    {
        if (healthPercentage <= lowHealthThreshold)
        {
            healthBarFill.color = lowHealthColor;
        }
        else if (healthPercentage <= mediumHealthThreshold)
        {
            // Interpola entre yellow e red
            float t = (healthPercentage - lowHealthThreshold) / (mediumHealthThreshold - lowHealthThreshold);
            healthBarFill.color = Color.Lerp(lowHealthColor, mediumHealthColor, t);
        }
        else
        {
            // Interpola entre green e yellow
            float t = (healthPercentage - mediumHealthThreshold) / (1f - mediumHealthThreshold);
            healthBarFill.color = Color.Lerp(mediumHealthColor, fullHealthColor, t);
        }
    }
    
    // Método público para forçar atualização (útil para testes)
    public void ForceUpdate()
    {
        UpdateHealthBar();
    }
}