using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUDController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Slider healthSlider; 
    [SerializeField] private float healthLerpSpeed = 8f;

    [Header("Coins")]
    [SerializeField] private TMPro.TMP_Text coinsText; // ou TMPro.TMP_Text se usar TextMeshPro

    // internals
    private float displayedHealthFraction = 1f;
    private float targetHealthFraction = 1f;

    private PlayerController player; // pega referências do Player (PlayerController). :contentReference[oaicite:1]{index=1}

    void Awake()
    {
        if (healthSlider == null) Debug.LogWarning("[HUD] healthSlider não atribuído.");
        if (coinsText == null) Debug.LogWarning("[HUD] coinsText não atribuído.");
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        // inicializa
        healthSlider.maxValue = player.GetMaxHealth();
        healthSlider.value = player.GetCurrentHealth();

        // subscreve eventos de moeda
        PlayerCurrency.OnCoinsChanged += OnCoinsChanged;
        // atualiza texto inicial
        coinsText.text = PlayerCurrency.Instance != null ? PlayerCurrency.Instance.GetCoins().ToString() : "0";
    }

    void OnDestroy()
    {
        PlayerCurrency.OnCoinsChanged -= OnCoinsChanged;
    }

    void Update()
    {
        // atualiza target health se houver player
        if (player != null)
            healthSlider.value = player.GetCurrentHealth();

    }

    private void UpdateTargetHealthFromPlayer()
    {
        if (player == null) return;
        int cur = player.GetCurrentHealth();
        int max = player.GetMaxHealth();
        if (max <= 0) return;
        targetHealthFraction = Mathf.Clamp01((float)cur / (float)max);
    }

    private void OnCoinsChanged(int newValue)
    {
        if (coinsText != null)
            coinsText.text = newValue.ToString();
    }

    // API pública (útil para testes / chamadas diretas)
    public void ForceUpdateCoins(int coins)
    {
        if (coinsText != null) coinsText.text = coins.ToString();
    }
}
