using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject painelPausa;
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject painelConfirmacao;
    
    [Header("Opções - Volume")]
    [SerializeField] private Slider sliderVolumeMaster;
    [SerializeField] private Slider sliderVolumeMusica;
    [SerializeField] private Slider sliderVolumeSFX;
    
    [Header("Opções - Resolução")]
    [SerializeField] private Toggle toggleFullscreen;
    
    private bool jogoPausado = false;
    
    void Start()
    {
        // Certifica que tudo está fechado no início
        if (painelPausa != null)
            painelPausa.SetActive(false);
        
        if (painelOpcoes != null)
            painelOpcoes.SetActive(false);
        
        if (painelConfirmacao != null)
            painelConfirmacao.SetActive(false);
        
        // Carrega as opções salvas
        CarregarOpcoes();
    }
    
    void Update()
    {
        // Detecta a tecla ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (jogoPausado)
            {
                Continuar();
            }
            else
            {
                Pausar();
            }
        }
    }
    
    #region Controle de Pausa
    
    public void Pausar()
    {
        jogoPausado = true;
        Time.timeScale = 0f; // Pausa o jogo
        
        if (painelPausa != null)
            painelPausa.SetActive(true);
        
        Debug.Log("⏸️ Jogo pausado");
    }
    
    public void Continuar()
    {
        jogoPausado = false;
        Time.timeScale = 1f; // Despausa o jogo
        
        // Fecha todos os painéis
        if (painelPausa != null)
            painelPausa.SetActive(false);
        
        if (painelOpcoes != null)
            painelOpcoes.SetActive(false);
        
        if (painelConfirmacao != null)
            painelConfirmacao.SetActive(false);
        
        Debug.Log("▶️ Jogo continuado");
    }
    
    #endregion
    
    #region Navegação entre Painéis
    
    // Botão "Opções"
    public void AbrirOpcoes()
    {
        if (painelPausa != null)
            painelPausa.SetActive(false);
        
        if (painelOpcoes != null)
            painelOpcoes.SetActive(true);
    }
    
    // Botão "Voltar" (das opções)
    public void VoltarParaPausa()
    {
        if (painelOpcoes != null)
            painelOpcoes.SetActive(false);
        
        if (painelPausa != null)
            painelPausa.SetActive(true);
    }
    
    // Botão "Menu Principal" - abre confirmação
    public void AbrirConfirmacaoMenuPrincipal()
    {
        if (painelPausa != null)
            painelPausa.SetActive(false);
        
        if (painelConfirmacao != null)
            painelConfirmacao.SetActive(true);
    }
    
    // Botão "Sim" da confirmação
    public void ConfirmarMenuPrincipal()
    {
        Time.timeScale = 1f; // Despausa antes de trocar de cena
        Debug.Log("🏠 Voltando ao menu principal...");
        SceneManager.LoadScene("MainMenu");
    }
    
    // Botão "Não" da confirmação
    public void CancelarMenuPrincipal()
    {
        if (painelConfirmacao != null)
            painelConfirmacao.SetActive(false);
        
        if (painelPausa != null)
            painelPausa.SetActive(true);
    }
    
    #endregion
    
    #region Sistema de Opções - Volume
    
    public void AjustarVolumeMaster(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("VolumeMaster", volume);
        PlayerPrefs.Save();
    }
    
    public void AjustarVolumeMusica(float volume)
    {
        // Ajusta o volume do GameMusicManager
        GameMusicManager musicManager = FindObjectOfType<GameMusicManager>();
        if (musicManager != null)
        {
            musicManager.SetVolume(volume);
        }
        
        PlayerPrefs.SetFloat("VolumeMusica", volume);
        PlayerPrefs.Save();
        
        Debug.Log($"🎵 Volume da música ajustado para: {volume:F2}");
    }
    
    public void AjustarVolumeSFX(float volume)
    {
        PlayerPrefs.SetFloat("VolumeSFX", volume);
        PlayerPrefs.Save();
    }
    
    #endregion
    
    #region Sistema de Opções - Vídeo
    
    public void AlternarFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public void ResetarOpcoes()
    {
        AudioListener.volume = 1f;
        
        if (sliderVolumeMaster != null)
            sliderVolumeMaster.value = 1f;
        
        if (sliderVolumeMusica != null)
            sliderVolumeMusica.value = 0.15f;
        
        // Reseta o volume do GameMusicManager
        GameMusicManager musicManager = FindObjectOfType<GameMusicManager>();
        if (musicManager != null)
        {
            musicManager.SetVolume(0.15f);
        }
        
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
        
        Debug.Log("✅ Opções resetadas!");
    }
    
    #endregion
    
    #region Carregar Opções
    
    private void CarregarOpcoes()
    {
        float volumeMaster = PlayerPrefs.GetFloat("VolumeMaster", 1f);
        AudioListener.volume = volumeMaster;
        if (sliderVolumeMaster != null)
            sliderVolumeMaster.value = volumeMaster;
        
        float volumeMusica = PlayerPrefs.GetFloat("VolumeMusica", 0.15f);
        if (sliderVolumeMusica != null)
            sliderVolumeMusica.value = volumeMusica;
        
        // Aplica o volume salvo ao GameMusicManager
        GameMusicManager musicManager = FindObjectOfType<GameMusicManager>();
        if (musicManager != null)
        {
            musicManager.SetVolume(volumeMusica);
        }
        
        float volumeSFX = PlayerPrefs.GetFloat("VolumeSFX", 0.8f);
        if (sliderVolumeSFX != null)
            sliderVolumeSFX.value = volumeSFX;
        
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = isFullscreen;
        if (toggleFullscreen != null)
            toggleFullscreen.isOn = isFullscreen;
    }
    
    #endregion
}