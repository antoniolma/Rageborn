using UnityEngine;

/// <summary>
/// Script para a hitbox da espada do Anukus.
/// Coloque este script em um GameObject filho do Anukus com um Collider2D (Trigger).
/// </summary>
public class AnukusSwordHitbox : MonoBehaviour
{
    private Anukus_Boss parentBoss;
    
    private void Start()
    {
        // Encontra o Anukus_Boss no pai
        parentBoss = GetComponentInParent<Anukus_Boss>();
        
        if (parentBoss == null)
        {
            Debug.LogError("❌ AnukusSwordHitbox não encontrou Anukus_Boss no pai!");
        }
        
        // Verifica se tem um collider configurado como trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("❌ AnukusSwordHitbox precisa de um Collider2D!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning("⚠️ AnukusSwordHitbox: Collider2D deve ser Trigger! Configurando automaticamente...");
            col.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Repassa a colisão para o boss pai
        if (parentBoss != null)
        {
            parentBoss.OnSwordHit(collision);
        }
    }
}
