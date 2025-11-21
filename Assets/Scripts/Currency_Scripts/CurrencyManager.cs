using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;
    
    [Header("Currency")]
    [SerializeField] private int startingRunCoins = 100;
    private int runCoins = 0; // Moedas temporárias da run
    private int metaCoins = 0; // Moedas permanentes (para metaprogression)
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI runCoinsText;
    [SerializeField] private TextMeshProUGUI metaCoinsText;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadMetaCoins();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        runCoins = startingRunCoins; 
        UpdateUI();
    }
    
    public void AddRunCoins(int amount)
    {
        runCoins += amount;
        UpdateUI();
        
        string coinColor = amount == 1 ? "Cobre" : amount == 5 ? "Prata" : "Ouro";
        Debug.Log($"💰 Coletou {amount} moedas ({coinColor})! Total: {runCoins}");
    }
    
    public void AddMetaCoins(int amount)
    {
        metaCoins += amount;
        SaveMetaCoins();
        UpdateUI();
    }
    
    public bool SpendRunCoins(int amount)
    {
        if (runCoins >= amount)
        {
            runCoins -= amount;
            UpdateUI();
            return true;
        }
        Debug.Log("❌ Moedas insuficientes!");
        return false;
    }
    
    public bool SpendMetaCoins(int amount)
    {
        if (metaCoins >= amount)
        {
            metaCoins -= amount;
            SaveMetaCoins();
            UpdateUI();
            return true;
        }
        return false;
    }
    
    public void ResetRunCoins()
    {
        runCoins = 0;
        UpdateUI();
    }
    
    public int GetRunCoins() => runCoins;
    public int GetMetaCoins() => metaCoins;
    
    void UpdateUI()
    {
        if (runCoinsText != null)
            runCoinsText.text = runCoins.ToString();
        if (metaCoinsText != null)
            metaCoinsText.text = metaCoins.ToString();
    }
    
    void SaveMetaCoins()
    {
        PlayerPrefs.SetInt("MetaCoins", metaCoins);
        PlayerPrefs.Save();
    }
    
    void LoadMetaCoins()
    {
        metaCoins = PlayerPrefs.GetInt("MetaCoins", 0);
    }
}