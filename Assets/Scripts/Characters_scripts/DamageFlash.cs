using UnityEngine;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private int numberOfFlashes = 2;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Material materialInstance;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            // Cria uma instância do material para não afetar outros objetos
            materialInstance = new Material(spriteRenderer.material);
            spriteRenderer.material = materialInstance;
            originalColor = spriteRenderer.color;
        }
    }
    
    public void Flash()
    {
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashCoroutine(flashColor));
        }
    }
    
    /// <summary>
    /// Flash com cor customizada (usado para diferentes tipos de arma)
    /// </summary>
    public void Flash(Color customColor)
    {
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashCoroutine(customColor));
        }
    }
    
    /// <summary>
    /// Flash baseado no tipo de arma
    /// </summary>
    public void FlashWithWeaponType(WeaponType weaponType)
    {
        Color weaponFlashColor = WeaponTypeHelper.GetDamageFlashColor(weaponType);
        Flash(weaponFlashColor);
    }
    
    private IEnumerator FlashCoroutine(Color colorToFlash)
    {
        for (int i = 0; i < numberOfFlashes; i++)
        {
            // Flash para a cor de dano
            spriteRenderer.color = colorToFlash;
            yield return new WaitForSeconds(flashDuration);
            
            // Volta para a cor original
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }
    }
    
    void OnDestroy()
    {
        // Limpa o material instanciado
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}