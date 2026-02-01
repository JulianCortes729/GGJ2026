using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; 

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource ambientSource;
    public AudioSource sfxSource;

    [Header("Scene Music")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public AudioClip ambientSound;

    [Header("SFX Clips")]
    public AudioClip onButtonClickClip;
    public AudioClip bellStart;
    public AudioClip bellEnd;

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

    [Header("Volume")]
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
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene escena, LoadSceneMode modo)
    {

        if (escena.buildIndex == 1)
        {
            ChangeMusic(menuMusic);
            ambientSource.Stop();
        }
        else if (escena.buildIndex == 2)
        {
            ChangeMusic(gameMusic);
            PlaySFX(bellStart);
            PlayAmbient(ambientSound);
        }
    }


    private void ChangeMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null)
            return;

        if (musicSource.clip == clip) return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    private void PlayAmbient(AudioClip clip)
    {
        if (clip == null || musicSource == null)
            return;

        if (musicSource.clip == clip) return;

        ambientSource.clip = clip;
        ambientSource.loop = true;
        ambientSource.volume = musicVolume;
        ambientSource.Play();
    }

    public static void PlaySFX(AudioClip clip, float volumen = 1f, float pitch = 1f)
    {
        if (clip == null || Instance == null || Instance.sfxSource == null) return;

        Instance.sfxSource.pitch = pitch;
        Instance.sfxSource.PlayOneShot(clip, volumen * Instance.sfxVolume);
    }

    public static IEnumerator PlaySFXWithDelay(AudioClip clip, float volumen = 1f, float pitch = 1f, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        PlaySFX(clip, volumen, pitch);
    }


    public void SetVolumenMusica(float valor)
    {
        musicVolume = valor;
        if (musicSource != null)
            musicSource.volume = musicVolume;

        PlayerPrefs.SetFloat(MusicVolumeKey, valor);
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

    private void LoadConfig()
    {
        if (PlayerPrefs.HasKey(MusicVolumeKey))
            musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey);
        if (PlayerPrefs.HasKey(SFXVolumeKey))
            sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey);

        bool mute = PlayerPrefs.GetInt(MuteMusicKey, 0) == 1;

        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
            musicSource.mute = mute;
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }
}
