using UnityEngine;

public class MenuMusicStarter : MonoBehaviour
{
    void Start()
    {
        Debug.Log("⭐ Script iniciou!");
        
        AudioSource audio = GetComponent<AudioSource>();
        
        if (audio == null)
        {
            Debug.LogError("❌ NÃO TEM AUDIO SOURCE!");
            return;
        }
        
        Debug.Log("✅ Audio Source encontrado!");
        Debug.Log("🎵 Clip: " + audio.clip.name);
        Debug.Log("🔊 Volume: " + audio.volume);
        Debug.Log("▶️ Play On Awake: " + audio.playOnAwake);
        
        audio.Play();
        
        Debug.Log("🎵 Chamou Play()!");
        Debug.Log("🎵 Está tocando? " + audio.isPlaying);
    }
}