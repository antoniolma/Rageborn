using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Gerencia menu principal e opções do Rageborn
public class MainMenu : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject painelPrincipal;
    [SerializeField] private GameObject painelOpcoes;
    
    [Header("Opções - Volume")]
    [SerializeField] private Slider sliderVolumeMaster;
    [SerializeField] private Slider sliderVolumeMusica;
    [SerializeField] private Slider sliderVolumeSFX;
    
    [Header("Opções - Resolução")]
    [SerializeField] private Toggle toggleFullscreen;
    
    [Header("Áudio (Opcional)")]
    [SerializeField] private AudioSource musicaMenu;

    private void Start()
    {
        MostrarPainelPrincipal();
        CarregarOpcoes();
    }

    #region Navegação Principal
    
    // Botão "Jogar"
    public void IniciarJogo()
    {
        SceneManager.LoadScene("GameScene");
    }

    // Botão "Opções"
    public void AbrirOpcoes()
    {
        painelPrincipal.SetActive(false);
        painelOpcoes.SetActive(true);
    }

    // Botão "Voltar"
    public void VoltarMenu()
    {
        MostrarPainelPrincipal();
    }

    // Botão "Sair"
    public void SairJogo()
    {
        Debug.Log("Saindo do Rageborn...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    #endregion

    #region Sistema de Opções - Volume

    // Slider de volume master
    public void AjustarVolumeMaster(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("VolumeMaster", volume);
        PlayerPrefs.Save();
    }

    // Slider de música
    public void AjustarVolumeMusica(float volume)
    {
        if (musicaMenu != null)
        {
            musicaMenu.volume = volume;
        }
        
        PlayerPrefs.SetFloat("VolumeMusica", volume);
        PlayerPrefs.Save();
    }

    // Slider de SFX
    public void AjustarVolumeSFX(float volume)
    {
        PlayerPrefs.SetFloat("VolumeSFX", volume);
        PlayerPrefs.Save();
    }

    #endregion

    #region Sistema de Opções - Vídeo

    // Toggle de fullscreen
    public void AlternarFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Botão "Resetar Padrões"
    public void ResetarOpcoes()
    {
        AudioListener.volume = 1f;
        
        if (sliderVolumeMaster != null)
            sliderVolumeMaster.value = 1f;
        
        if (sliderVolumeMusica != null)
            sliderVolumeMusica.value = 0.7f;
        
        if (sliderVolumeSFX != null)
            sliderVolumeSFX.value = 0.8f;
        
        if (toggleFullscreen != null)
            toggleFullscreen.isOn = true;
        
        Screen.fullScreen = true;
        
        PlayerPrefs.DeleteKey("VolumeMaster");
        PlayerPrefs.DeleteKey("VolumeMusica");
        PlayerPrefs.DeleteKey("VolumeSFX");
        PlayerPrefs.DeleteKey("Fullscreen");
        PlayerPrefs.Save();
        
        Debug.Log("Opções resetadas!");
    }

    #endregion

    #region Carregar/Salvar Opções

    // Carrega as configs salvas
    private void CarregarOpcoes()
    {
        float volumeMaster = PlayerPrefs.GetFloat("VolumeMaster", 1f);
        AudioListener.volume = volumeMaster;
        if (sliderVolumeMaster != null)
            sliderVolumeMaster.value = volumeMaster;
        
        float volumeMusica = PlayerPrefs.GetFloat("VolumeMusica", 0.7f);
        if (musicaMenu != null)
            musicaMenu.volume = volumeMusica;
        if (sliderVolumeMusica != null)
            sliderVolumeMusica.value = volumeMusica;
        
        float volumeSFX = PlayerPrefs.GetFloat("VolumeSFX", 0.8f);
        if (sliderVolumeSFX != null)
            sliderVolumeSFX.value = volumeSFX;
        
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = isFullscreen;
        if (toggleFullscreen != null)
            toggleFullscreen.isOn = isFullscreen;
    }

    #endregion

    #region Helpers

    private void MostrarPainelPrincipal()
    {
        painelPrincipal.SetActive(true);
        if (painelOpcoes != null)
            painelOpcoes.SetActive(false);
    }

    #endregion
}