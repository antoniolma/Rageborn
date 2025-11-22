using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);
    
    private Camera cam;
    
    void Start()
    {
        // ✅ Força o tamanho ortográfico da câmera para 10
        cam = GetComponent<Camera>();
        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize = 10f;
            Debug.Log("📷 CameraFollow - Tamanho ortográfico definido para 10");
        }
        else if (cam != null && !cam.orthographic)
        {
            Debug.LogWarning("⚠️ CameraFollow - Câmera não está em modo ortográfico! Defina Projection como Orthographic.");
        }
        
        // Se não definir um target no Inspector, procura o jogador
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
