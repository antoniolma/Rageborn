using UnityEngine;

// Script simples para marcar onde inimigos podem spawnar
public class EnemySpawnPoint : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Color gizmoColor = Color.red;
    [SerializeField] private float gizmoRadius = 0.5f;
    [SerializeField] private bool showLabel = true;
    
    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        
        if (showLabel)
        {
            // Desenha linha para cima
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1.5f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, gizmoRadius);
    }
}