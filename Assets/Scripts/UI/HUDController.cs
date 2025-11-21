using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUDController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Slider healthSlider; 
    [SerializeField] private float healthLerpSpeed = 8f;

    [Header("Coins")]
    [SerializeField] private TMPro.TMP_Text coinsText;

    // internals
    private float displayedHealthFraction = 1f;
    private float targetHealthFraction = 1f;

    private PlayerController player;

    void Awake()
    {
        if (healthSlider == null) Debug.LogWarning("[HUD] healthSlider não atribuído.");
        if (coinsText == null) Debug.LogWarning("[HUD] coinsText não atribuído.");
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
        
        if (player != null)
        {
            // inicializa health
            healthSlider.maxValue = player.GetMaxHealth();
            healthSlider.value = player.GetCurrentHealth();
        }

        // ✅ Atualiza texto inicial de moedas
        UpdateCoinsDisplay();
    }

    void Update()
    {
        // atualiza health
        if (player != null)
            healthSlider.value = player.GetCurrentHealth();

        // ✅ Atualiza moedas todo frame (ou você pode usar eventos se preferir)
        UpdateCoinsDisplay();
    }

    private void UpdateTargetHealthFromPlayer()
    {
        if (player == null) return;
        int cur = player.GetCurrentHealth();
        int max = player.GetMaxHealth();
        if (max <= 0) return;
        targetHealthFraction = Mathf.Clamp01((float)cur / (float)max);
    }

    // ✅ NOVO - Atualiza display de moedas usando CurrencyManager
    private void UpdateCoinsDisplay()
    {
        if (coinsText != null && CurrencyManager.Instance != null)
        {
            coinsText.text = CurrencyManager.Instance.GetRunCoins().ToString();
        }
    }

    // API pública (útil para testes / chamadas diretas)
    public void ForceUpdateCoins(int coins)
    {
        if (coinsText != null) 
            coinsText.text = coins.ToString();
    }
}