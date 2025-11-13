using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music Clips")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameMusic;

    private AudioClip placeSFX;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private float musicVolume = 1f;
    private float sfxVolume = 1f;
    private bool isMusicOn = true;
    private bool isSFXOn = true;
    private bool hasStarted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup audio sources
        musicSource = GetComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        // === Load settings and music ===
        LoadSettings();
        LoadMusicFromResources();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void LoadMusicFromResources()
    {
        mainMenuMusic = Resources.Load<AudioClip>("Audio/Music/MainMenuTheme");
        gameMusic = Resources.Load<AudioClip>("Audio/Music/GameTheme");

        // Load SFX files
        placeSFX = Resources.Load<AudioClip>("Audio/SFX/placeSFX");

        if (mainMenuMusic == null)
            Debug.LogWarning("MainMenuTheme.mp3 not found in Resources/Audio/Music");

        if (gameMusic == null)
            Debug.LogWarning("GameTheme.mp3 not found in Resources/Audio/Music");

        if (placeSFX == null)
            Debug.LogWarning("placeSFX.mp3 not found in Resources/Audio/SFX");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenu":
                if (mainMenuMusic != null)
                {
                    if (!hasStarted)
                    {
                        musicSource.clip = mainMenuMusic;
                        musicSource.volume = musicVolume;
                        musicSource.mute = !isMusicOn;
                        musicSource.Play();
                        hasStarted = true;
                    }
                    else
                    {
                        StartCoroutine(FadeToNewTrack(mainMenuMusic));
                    }
                }
                break;

            case "Game":
                if (gameMusic != null)
                    StartCoroutine(FadeToNewTrack(gameMusic));
                break;
        }
    }

    public void PlayTestSFX()
    {
        PlaySFX(placeSFX);
    }


    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip != null && isSFXOn)
        {
            float finalVolume = Mathf.Clamp01(sfxVolume * volumeMultiplier);
            sfxSource.PlayOneShot(clip, finalVolume);
        }
    }

    private IEnumerator FadeToNewTrack(AudioClip newClip)
    {
        float fadeDuration = 1f;
        float startVolume = musicSource.volume;

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.mute = !isMusicOn;
        musicSource.Play();

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, musicVolume, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = musicVolume;
    }

    // --- Volume & Toggle Management ---
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
        SaveSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        SaveSettings();
    }

    public void ToggleMusic(bool isOn)
    {
        isMusicOn = isOn;
        musicSource.mute = !isOn;
        SaveSettings();
    }

    public void ToggleSFX(bool isOn)
    {
        isSFXOn = isOn;
        SaveSettings();
    }

    // --- Getters for UI Sync ---
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
    public bool IsMusicOn() => isMusicOn;
    public bool IsSFXOn() => isSFXOn;

    private void LoadSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        isMusicOn = PlayerPrefs.GetInt("IsMusicOn", 1) == 1;
        isSFXOn = PlayerPrefs.GetInt("IsSFXOn", 1) == 1;

        musicSource.volume = musicVolume;
        musicSource.mute = !isMusicOn;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("IsMusicOn", isMusicOn ? 1 : 0);
        PlayerPrefs.SetInt("IsSFXOn", isSFXOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
