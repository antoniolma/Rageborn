using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float defaultDuration = 0.2f;
    [SerializeField] private float defaultIntensity = 0.3f;
    
    [Header("Shake Curve (Optional)")]
    [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.Linear(0, 1, 1, 0);
    
    private bool isShaking = false;
    
    // Método principal - chame este para fazer shake
    public void Shake(float duration, float intensity)
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine(duration, intensity));
        }
    }
    
    // Sobrecarga - usa valores padrão
    public void Shake()
    {
        Shake(defaultDuration, defaultIntensity);
    }
    
    IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        isShaking = true;
        
        // ✅ CORREÇÃO: Salva posição ANTES do shake, não no Start()
        Vector3 originalPosition = transform.localPosition;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Usa curva de animação para suavizar o shake
            float curveValue = shakeCurve.Evaluate(progress);
            
            // Calcula offset aleatório
            float x = Random.Range(-1f, 1f) * intensity * curveValue;
            float y = Random.Range(-1f, 1f) * intensity * curveValue;
            
            // Aplica shake
            transform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            yield return null;
        }
        
        // Retorna à posição original
        transform.localPosition = originalPosition;
        isShaking = false;
    }
    
    // Método para shake súbito (hit único)
    public void ShakeOnce(float intensity)
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeOnceCoroutine(intensity));
        }
    }
    
    IEnumerator ShakeOnceCoroutine(float intensity)
    {
        isShaking = true;
        
        // ✅ CORREÇÃO: Salva posição ANTES do shake
        Vector3 originalPosition = transform.localPosition;
        
        // Shake súbito
        float x = Random.Range(-1f, 1f) * intensity;
        float y = Random.Range(-1f, 1f) * intensity;
        transform.localPosition = originalPosition + new Vector3(x, y, 0);
        
        yield return new WaitForSeconds(0.05f);
        
        // Retorna suavemente
        float elapsed = 0f;
        float returnDuration = 0.1f;
        Vector3 shakePosition = transform.localPosition;
        
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / returnDuration;
            
            transform.localPosition = Vector3.Lerp(shakePosition, originalPosition, progress);
            yield return null;
        }
        
        transform.localPosition = originalPosition;
        isShaking = false;
    }
}