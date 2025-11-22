using UnityEngine;

/// <summary>
/// Enum para os tipos de arma disponíveis no jogo
/// </summary>
public enum WeaponType
{
    Fire = 0,   // Fogo - Flash vermelho
    Ice = 1,    // Gelo - Flash azul
    Venom = 2   // Veneno - Flash verde
}

/// <summary>
/// Classe auxiliar para obter informações sobre tipos de arma
/// </summary>
public static class WeaponTypeHelper
{
    /// <summary>
    /// Retorna a cor do flash de dano baseado no tipo de arma
    /// </summary>
    public static Color GetDamageFlashColor(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.Fire:
                return new Color(1f, 0.2f, 0f); // Vermelho-laranja (fogo)
            
            case WeaponType.Ice:
                return new Color(0.2f, 0.6f, 1f); // Azul claro (gelo)
            
            case WeaponType.Venom:
                return new Color(0.2f, 1f, 0.2f); // Verde (veneno)
            
            default:
                return Color.red; // Fallback
        }
    }
    
    /// <summary>
    /// Retorna o nome do tipo de arma
    /// </summary>
    public static string GetWeaponName(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.Fire:
                return "Fogo";
            case WeaponType.Ice:
                return "Gelo";
            case WeaponType.Venom:
                return "Veneno";
            default:
                return "Desconhecido";
        }
    }
}
