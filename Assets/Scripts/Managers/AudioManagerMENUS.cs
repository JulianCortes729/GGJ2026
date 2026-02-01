using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManagerMENUS : MonoBehaviour
{
    public static AudioManagerMENUS Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource ambientSource;
    public AudioSource sfxSource;

    [Header("Scene Music")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public AudioClip ambientSound;
    // ... (El resto de tus variables de SFX siguen igual) ...
    [Header("SFX Clips")]
    public AudioClip onButtonClickClip;
    public AudioClip bellStart;

    [Header("Player SFX")]
    public AudioClip jumpClip;
    public AudioClip landClip;
    public AudioClip punchSwingClip;
    public AudioClip spinAttackClip;
    public AudioClip ragdollImpactClip;

    [Header("Mask SFX")]
    public AudioClip maskPickupClip1;
    public AudioClip maskPickupClip2;

    public AudioClip maskPickupClip3;

    [Header("UI SFX")]
    public AudioClip uiHoverClip;

    // ... (Variables de volumen siguen igual) ...
    [Range(0, 1)] public float musicVolume = 1f;
    [Range(0, 1)] public float sfxVolume = 1f;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string MuteMusicKey = "MuteMusic";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadConfig();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        //PAUSA
        PauseEvents.OnPauseGame += PauseGameAudio;
        PauseEvents.OnResumeGame += ResumeGameAudio;
        PauseEvents.OnRestartGame += RestoreAudioForGameplay;
        PauseEvents.OnReturnToMenu += StopAllAudio;

        //DERROTA
        DefeatEvents.OnDefeat += PauseGameAudio;
        DefeatEvents.OnRetryFromDefeat += RestoreAudioForGameplay;
        DefeatEvents.OnReturnToMenuFromDefeat += StopAllAudio;

        //VICTORIA
        //Al ganar, pausamos el audio del juego 
        VictoryEvents.OnVictory += PauseGameAudio;

        VictoryEvents.OnReturnToMenuFromVictory += StopAllAudio;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //PAUSA
        PauseEvents.OnPauseGame -= PauseGameAudio;
        PauseEvents.OnResumeGame -= ResumeGameAudio;
        PauseEvents.OnRestartGame -= RestoreAudioForGameplay;
        PauseEvents.OnReturnToMenu -= StopAllAudio;

        //DERROTA
        DefeatEvents.OnDefeat -= PauseGameAudio;
        DefeatEvents.OnRetryFromDefeat -= RestoreAudioForGameplay;
        DefeatEvents.OnReturnToMenuFromDefeat -= StopAllAudio;

        //VICTORIA 
        VictoryEvents.OnVictory -= PauseGameAudio;
        VictoryEvents.OnReturnToMenuFromVictory -= StopAllAudio;
    }


    private void OnSceneLoaded(Scene escena, LoadSceneMode modo)
    {
        // === ZONA DE AUTODESTRUCCIÓN ===
        // Si detectamos que estamos en el Menú Principal (Índice 0)
        if (escena.buildIndex == 1)
        {
            // 1. Limpiamos la referencia estática para que nadie intente acceder a un muerto
            if (Instance == this)
            {
                Instance = null;
            }

            // 2. Nos destruimos a nosotros mismos
            Destroy(gameObject);

            // 3. ¡IMPORTANTE! Return para que NO se ejecute nada más de este código
            return;
        }

        // ============================================
        // Si llegamos aquí, es que NO estamos en el menú, así que cargamos el audio normal.

        ResumeGameAudio();

        if (escena.buildIndex == 2)
        {
            ChangeMusic(gameMusic);
            PlayAmbient(ambientSound);

            if (bellStart != null)
            {
                StartCoroutine(PlaySFXWithDelay(bellStart, 1f, 1f, 0.2f));
            }
        }
    }

    public void PauseGameAudio()
    {
        if (musicSource != null && musicSource.isPlaying) musicSource.Pause();
        if (ambientSource != null && ambientSource.isPlaying) ambientSource.Pause();
    }

    public void ResumeGameAudio()
    {
        if (musicSource != null) musicSource.UnPause();
        if (ambientSource != null) ambientSource.UnPause();
    }

    private void RestoreAudioForGameplay()
    {
        // Al reiniciar, nos aseguramos que todo est� "despausado" y listo
        ResumeGameAudio();
       
    }

    private void StopAllAudio()
    {
        
        if (musicSource != null) musicSource.Stop();
        if (ambientSource != null) ambientSource.Stop();
        if (sfxSource != null) sfxSource.Stop();
    }


    private void ChangeMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    private void PlayAmbient(AudioClip clip)
    {
        if (clip == null || ambientSource == null) return;
        if (ambientSource.clip == clip && ambientSource.isPlaying) return;

        ambientSource.clip = clip;
        ambientSource.loop = true;
        ambientSource.volume = musicVolume;
        ambientSource.Play();
    }

    private void LoadConfig()
    {
        if (PlayerPrefs.HasKey("MusicVolume")) musicVolume = PlayerPrefs.GetFloat("MusicVolume");
        if (PlayerPrefs.HasKey("SFXVolume")) sfxVolume = PlayerPrefs.GetFloat("SFXVolume");

       

        if (musicSource != null) musicSource.volume = musicVolume;
    }
    public static IEnumerator PlaySFXWithDelay(AudioClip clip, float volumen = 1f, float pitch = 1f, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        PlaySFX(clip, volumen, pitch);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public static void PlaySFX(AudioClip clip, float volumen = 1f, float pitch = 1f)
    {
        if (clip == null || Instance == null || Instance.sfxSource == null) return;

        Instance.sfxSource.pitch = pitch;
        Instance.sfxSource.PlayOneShot(clip, volumen * Instance.sfxVolume);
    }

    public void SetVolumenSFX(float valor)
    {
        sfxVolume = valor;
        PlayerPrefs.SetFloat(SFXVolumeKey, valor);
    }

    public void MuteMusica(bool mute)
    {
        if (musicSource != null)
            musicSource.mute = mute;

        PlayerPrefs.SetInt(MuteMusicKey, mute ? 1 : 0);
    }
}