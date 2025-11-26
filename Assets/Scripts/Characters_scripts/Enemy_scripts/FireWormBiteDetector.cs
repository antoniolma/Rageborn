using UnityEngine;

/// <summary>
/// Script para detectar colisões da hitbox de mordida do FireWorm
/// Adicione este script no objeto filho que tem o Collider2D da boca
/// </summary>
public class FireWormBiteDetector : MonoBehaviour
{
    private FireWorm parentFireWorm;
    
    void Start()
    {
        // Busca o FireWorm pai
        parentFireWorm = GetComponentInParent<FireWorm>();
        
        if (parentFireWorm == null)
        {
            // Debug.LogError("⚠️ FireWormBiteDetector não encontrou FireWorm pai!");
        }
        else
        {
            // Debug.Log($"✅ FireWormBiteDetector configurado! Pai: {parentFireWorm.name}");
        }
        
        // Verifica se tem collider
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            // Debug.LogError("⚠️ FireWormBiteDetector não tem Collider2D!");
        }
        else
        {
            // Debug.Log($"✅ FireWormBiteDetector - Collider: {col.GetType().Name}, IsTrigger: {col.isTrigger}, Enabled: {col.enabled}");
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Debug.Log($"🔥👄 FireWormBiteDetector - OnTriggerEnter2D com: {collision.name}, Tag: {collision.tag}");
        
        if (parentFireWorm != null)
        {
            parentFireWorm.OnBiteHit(collision);
        }
        else
        {
            // Debug.LogError("⚠️ parentFireWorm é NULL!");
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Debug.Log($"⚠️ FireWormBiteDetector - OnCollisionEnter2D (NÃO DEVERIA USAR COLLISION!) com: {collision.gameObject.name}");
    }
}
