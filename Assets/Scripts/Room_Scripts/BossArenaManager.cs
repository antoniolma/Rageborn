using UnityEngine;

public class BossArenaManager : MonoBehaviour
{
    [Header("Boss References")]
    [SerializeField] private GameObject anukusBoss;
    [SerializeField] private GameObject bobBoss;
    
    private bool anukusDead = false;
    private bool bobDead = false;
    private bool victoryTriggered = false;
    
    public static BossArenaManager Instance;
    
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
    
    void Update()
    {
        if (victoryTriggered) return;
        
        // Verifica se os bosses morreram
        if (!anukusDead && anukusBoss == null)
        {
            anukusDead = true;
            Debug.Log("⚔️ Anukus foi derrotado!");
            CheckVictory();
        }
        
        if (!bobDead && bobBoss == null)
        {
            bobDead = true;
            Debug.Log("⚡ Bob foi derrotado!");
            CheckVictory();
        }
    }
    
    void CheckVictory()
    {
        if (anukusDead && bobDead && !victoryTriggered)
        {
            victoryTriggered = true;
            Debug.Log("🏆 AMBOS OS BOSSES DERROTADOS! VITÓRIA!");
            
            // Pequeno delay antes de mostrar tela de vitória
            Invoke(nameof(TriggerVictory), 1.5f);
        }
    }
    
    void TriggerVictory()
    {
        if (VictoryManager.Instance != null)
        {
            VictoryManager.Instance.TriggerVictory();
        }
        else
        {
            Debug.LogWarning("⚠️ VictoryManager não encontrado!");
        }
    }
    
    // Método público caso queira registrar morte manualmente
    public void RegisterBossDeath(string bossName)
    {
        if (bossName == "Anukus")
        {
            anukusDead = true;
            Debug.Log("⚔️ Anukus foi derrotado! (Manual)");
        }
        else if (bossName == "Bob")
        {
            bobDead = true;
            Debug.Log("⚡ Bob foi derrotado! (Manual)");
        }
        
        CheckVictory();
    }
}