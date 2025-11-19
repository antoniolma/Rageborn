using UnityEngine;

// Script simples para marcar onde o player spawna em cada scene
public class PlayerSpawnPoint : MonoBehaviour
{
    [Header("Visual Helper")]
    [SerializeField] private Color gizmoColor = Color.green;
    [SerializeField] private float gizmoRadius = 1f;
    
    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, gizmoRadius);
    }
}