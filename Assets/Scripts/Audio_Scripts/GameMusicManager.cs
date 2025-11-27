using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameMusicManager : MonoBehaviour
{
    [Header("🎵 Músicas do Menu")]
    [SerializeField] private AudioClip[] menuMusicTracks;
    
    [Header("🎮 Músicas do Jogo")]
    [SerializeField] private AudioClip[] gameMusicTracks;
    
    [Header("💀 Músicas do Boss")]
    [SerializeField] private AudioClip[] bossMusicTracks;
    
    [Header("⚙️ Configurações")]
    [SerializeField] private float volume = 0.15f;
    
    private static GameMusicManager instance;
    private AudioSource audioSource;
    private AudioClip[] currentPlaylist;
    private int currentTrackIndex = 0;
    private bool isBossArena = false;
    
    void Awake()
    {
        Debug.Log($"🎵 GameMusicManager.Awake() chamado! instance={instance?.name ?? "NULL"}, this={this.name}");
        
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"⚠️ Destruindo GameMusicManager duplicado em '{gameObject.name}'!");
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        Debug.Log($"✅ GameMusicManager registrado como singleton!");
        
        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.loop = false; // Não loop individual, vamos controlar manualmente
        audioSource.playOnAwake = false;
        audioSource.volume = volume;
        audioSource.spatialBlend = 0f; // 🔊 IMPORTANTE: 0 = 2D (ouve em qualquer lugar), 1 = 3D (só perto do objeto)
        audioSource.priority = 0; // Máxima prioridade
        audioSource.reverbZoneMix = 0f; // 🔊 Desabilita reverb zones
        audioSource.bypassEffects = true; // 🔊 Ignora todos os efeitos de áudio
        audioSource.bypassListenerEffects = true; // 🔊 Ignora efeitos do listener
        audioSource.bypassReverbZones = true; // 🔊 Ignora zonas de reverb completamente
        
        Debug.Log($"🔊 AudioSource configurado: spatialBlend={audioSource.spatialBlend}, priority={audioSource.priority}, bypassEffects={audioSource.bypassEffects}");
        
        // Registra callback para verificar AudioListener ao trocar de cena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDestroy()
    {
        Debug.LogWarning($"❌ GameMusicManager sendo DESTRUÍDO! GameObject: {gameObject.name}, cena: {SceneManager.GetActiveScene().name}");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureAudioListener();
        CheckSceneAndPlayMusic(scene.name);
    }
    
    void Start()
    {
        // Garante AudioListener na cena inicial
        EnsureAudioListener();
        
        // Carrega o volume salvo
        float volumeSalvo = PlayerPrefs.GetFloat("VolumeMusica", 0.15f);
        SetVolume(volumeSalvo);
        Debug.Log($"🎵 Volume de música carregado: {volumeSalvo:F2}");
        
        // Começa tocando música do menu
        string currentScene = SceneManager.GetActiveScene().name;
        CheckSceneAndPlayMusic(currentScene);
    }
    
    void Update()
    {
        // Debug do estado atual
        if (Time.frameCount % 120 == 0) // A cada 2 segundos
        {
            if (audioSource != null && audioSource.clip != null)
            {
                Debug.Log($"🎵 Status: isPlaying={audioSource.isPlaying}, time={audioSource.time:F2}/{audioSource.clip.length:F2}s, clip={audioSource.clip.name}, volume={audioSource.volume}, enabled={audioSource.enabled}, GameObject.active={gameObject.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning($"⚠️ AudioSource ou clip é NULL! audioSource={audioSource != null}, clip={audioSource?.clip != null}");
            }
        }
        
        // Verifica se a música atual terminou para tocar a próxima
        if (!audioSource.isPlaying && currentPlaylist != null && currentPlaylist.Length > 0)
        {
            Debug.Log("🎵 Música terminou, tocando próxima...");
            PlayNextTrack();
        }
    }
    
    /// <summary>
    /// Verifica a cena e decide qual playlist tocar
    /// </summary>
    void CheckSceneAndPlayMusic(string sceneName)
    {
        Debug.Log($"🎵 GameMusicManager: Cena carregada = '{sceneName}'");
        
        bool wasInBossArena = isBossArena;
        isBossArena = (sceneName == "BossArena");
        
        Debug.Log($"🎵 isBossArena = {isBossArena}");
        
        // Determina qual playlist usar
        AudioClip[] newPlaylist = null;
        string playlistType = "";
        
        Debug.Log($"🎵 Verificando playlists - Boss:{bossMusicTracks?.Length ?? 0}, Game:{gameMusicTracks?.Length ?? 0}, Menu:{menuMusicTracks?.Length ?? 0}");
        
        if (isBossArena && bossMusicTracks != null && bossMusicTracks.Length > 0)
        {
            newPlaylist = bossMusicTracks;
            playlistType = "BOSS";
            Debug.Log($"🎵 Selecionou BOSS playlist ({bossMusicTracks.Length} músicas)");
        }
        else if (IsGameScene(sceneName) && gameMusicTracks != null && gameMusicTracks.Length > 0)
        {
            newPlaylist = gameMusicTracks;
            playlistType = "GAME";
            Debug.Log($"🎵 Selecionou GAME playlist ({gameMusicTracks.Length} músicas)");
        }
        else if (IsMenuScene(sceneName) && menuMusicTracks != null && menuMusicTracks.Length > 0)
        {
            newPlaylist = menuMusicTracks;
            playlistType = "MENU";
            Debug.Log($"🎵 Selecionou MENU playlist ({menuMusicTracks.Length} músicas)");
        }
        else
        {
            Debug.LogWarning($"⚠️ Nenhuma playlist encontrada para cena '{sceneName}'");
        }
        
        // Compara se é a mesma playlist (mesmo array de referência)
        bool isSamePlaylist = (newPlaylist == currentPlaylist);
        Debug.Log($"🎵 Playlist atual vs nova: {(currentPlaylist == menuMusicTracks ? "MENU" : currentPlaylist == gameMusicTracks ? "GAME" : currentPlaylist == bossMusicTracks ? "BOSS" : "NULL")} vs {playlistType} | São iguais? {isSamePlaylist}");
        
        // Só troca a playlist se mudou de tipo
        if (!isSamePlaylist && newPlaylist != null)
        {
            Debug.Log($"🎵 Trocando playlist de {(currentPlaylist == menuMusicTracks ? "MENU" : currentPlaylist == gameMusicTracks ? "GAME" : currentPlaylist == bossMusicTracks ? "BOSS" : "NULL")} para {playlistType}!");
            SwitchPlaylist(newPlaylist);
        }
        else
        {
            Debug.Log($"🎵 Playlist já é a mesma ({playlistType}), continuando música atual");
        }
    }
    
    /// <summary>
    /// Troca a playlist imediatamente (sem fade)
    /// </summary>
    void SwitchPlaylist(AudioClip[] newPlaylist)
    {
        if (newPlaylist == null || newPlaylist.Length == 0)
        {
            Debug.LogWarning("⚠️ GameMusicManager: Playlist vazia ou nula!");
            return;
        }
        
        Debug.Log($"🎵 SwitchPlaylist chamado com {newPlaylist.Length} músicas");
        
        // Verifica se há clips nulos
        int nullCount = 0;
        for (int i = 0; i < newPlaylist.Length; i++)
        {
            if (newPlaylist[i] == null)
            {
                Debug.LogError($"❌ CLIP NULL no índice {i} da playlist!");
                nullCount++;
            }
            else
            {
                Debug.Log($"   ✅ Índice {i}: {newPlaylist[i].name}");
            }
        }
        
        if (nullCount > 0)
        {
            Debug.LogError($"❌ {nullCount} de {newPlaylist.Length} clips estão NULL! Verifique o Inspector do GameMusicManager!");
            return;
        }
        
        Debug.Log($"✅ Todos os {newPlaylist.Length} clips estão OK!");
        
        currentPlaylist = newPlaylist;
        currentTrackIndex = Random.Range(0, currentPlaylist.Length); // Começa em música aleatória
        
        Debug.Log($"🎵 Índice aleatório escolhido: {currentTrackIndex}");
        
        // Para a música atual imediatamente
        audioSource.Stop();
        
        // Verifica se o clip existe
        if (currentPlaylist[currentTrackIndex] == null)
        {
            Debug.LogError($"❌ Música no índice {currentTrackIndex} é NULL!");
            return;
        }
        
        // Toca a nova música
        audioSource.clip = currentPlaylist[currentTrackIndex];
        audioSource.volume = volume;
        audioSource.spatialBlend = 0f; // Força 2D
        audioSource.priority = 0; // Máxima prioridade
        audioSource.mute = false; // Garante que não está mudo
        audioSource.enabled = true; // Garante que está ativado
        audioSource.Play();
        
        Debug.Log($"🎵 Mudou para: {audioSource.clip.name} | Volume: {audioSource.volume} | isPlaying: {audioSource.isPlaying} | mute: {audioSource.mute} | AudioSource em: {audioSource.gameObject.name} | spatialBlend: {audioSource.spatialBlend}");
    }
    
    /// <summary>
    /// Toca a próxima música da playlist
    /// </summary>
    void PlayNextTrack()
    {
        if (currentPlaylist == null || currentPlaylist.Length == 0) return;
        
        // Escolhe aleatoriamente uma música diferente da atual (se possível)
        int newIndex = currentTrackIndex;
        
        if (currentPlaylist.Length > 1)
        {
            // Garante que não repete a mesma música
            while (newIndex == currentTrackIndex)
            {
                newIndex = Random.Range(0, currentPlaylist.Length);
            }
        }
        else
        {
            // Só tem 1 música, repete ela
            newIndex = 0;
        }
        
        currentTrackIndex = newIndex;
        audioSource.clip = currentPlaylist[currentTrackIndex];
        audioSource.Play();
        
        Debug.Log($"🎵 Tocando: {audioSource.clip.name}");
    }
    
    /// <summary>
    /// Verifica se é uma cena de menu
    /// </summary>
    bool IsMenuScene(string sceneName)
    {
        return sceneName.Contains("Menu") || sceneName.Contains("MainMenu");
    }
    
    /// <summary>
    /// Verifica se é uma cena de jogo
    /// </summary>
    bool IsGameScene(string sceneName)
    {
        // Arenas de jogo hardcoded
        bool isGame = sceneName == "Arena_Inferno" || 
                      sceneName == "Arena_dungeon" || 
                      sceneName == "Shop_Dungeon";
        
        Debug.Log($"🎵 IsGameScene('{sceneName}') = {isGame}");
        return isGame;
    }
    
    /// <summary>
    /// Garante que existe um AudioListener ativo na cena
    /// </summary>
    void EnsureAudioListener()
    {
        AudioListener[] allListeners = FindObjectsOfType<AudioListener>();
        
        Debug.Log($"🔊 Verificando AudioListeners na cena '{SceneManager.GetActiveScene().name}' - Encontrados: {allListeners.Length}");
        
        foreach (var listener in allListeners)
        {
            Debug.Log($"   - AudioListener em: {listener.gameObject.name} (enabled={listener.enabled})");
        }
        
        if (allListeners.Length == 0)
        {
            // Não encontrou AudioListener, verifica se a Main Camera existe
            Camera mainCamera = Camera.main;
            
            if (mainCamera != null)
            {
                // Adiciona AudioListener na Main Camera
                AudioListener newListener = mainCamera.gameObject.AddComponent<AudioListener>();
                Debug.Log($"✅ AudioListener adicionado à Main Camera na cena: {SceneManager.GetActiveScene().name}");
            }
            else
            {
                // Cria um GameObject dedicado para o AudioListener
                GameObject audioListenerObj = new GameObject("AudioListener");
                audioListenerObj.AddComponent<AudioListener>();
                Debug.LogWarning($"⚠️ Main Camera não encontrada! AudioListener criado em GameObject separado na cena: {SceneManager.GetActiveScene().name}");
            }
        }
        else if (allListeners.Length > 1)
        {
            Debug.LogWarning($"⚠️ MÚLTIPLOS AudioListeners encontrados ({allListeners.Length})! Desativando extras...");
            
            // Mantém apenas o da Main Camera ativo
            Camera mainCamera = Camera.main;
            AudioListener cameraListener = mainCamera?.GetComponent<AudioListener>();
            
            foreach (var listener in allListeners)
            {
                if (listener != cameraListener)
                {
                    Debug.LogWarning($"   ❌ Desativando AudioListener em: {listener.gameObject.name}");
                    listener.enabled = false;
                }
                else
                {
                    Debug.Log($"   ✅ Mantendo AudioListener em: {listener.gameObject.name}");
                }
            }
        }
    }
    
    // ========================================
    // MÉTODOS PÚBLICOS
    // ========================================
    
    /// <summary>
    /// Ajusta o volume da música
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume;
    }
    
    /// <summary>
    /// Pausa a música
    /// </summary>
    public void PauseMusic()
    {
        audioSource.Pause();
    }
    
    /// <summary>
    /// Retoma a música
    /// </summary>
    public void ResumeMusic()
    {
        audioSource.UnPause();
    }
}