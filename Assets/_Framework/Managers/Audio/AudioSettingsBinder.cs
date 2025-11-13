using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsBinder : MonoBehaviour
{
    [Header("UI References")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle musicToggle;
    public Toggle sfxToggle;

    private bool isInitialized = false;

    private void Start()
    {
        // Wait until AudioManager is ready
        StartCoroutine(InitializeWhenReady());
    }

    private System.Collections.IEnumerator InitializeWhenReady()
    {
        while (AudioManager.Instance == null)
            yield return null;

        var audioManager = AudioManager.Instance;

        // === Initialize UI values ===
        musicVolumeSlider.value = audioManager.GetMusicVolume();
        sfxVolumeSlider.value = audioManager.GetSFXVolume();

        // invert toggle state because toggle = MUTE
        musicToggle.isOn = !audioManager.IsMusicOn();
        sfxToggle.isOn = !audioManager.IsSFXOn();

        // Add listeners (invert the bool)
        musicVolumeSlider.onValueChanged.AddListener(v => audioManager.SetMusicVolume(v));
        sfxVolumeSlider.onValueChanged.AddListener(v => audioManager.SetSFXVolume(v));
        musicToggle.onValueChanged.AddListener(isMuted => audioManager.ToggleMusic(!isMuted));
        sfxToggle.onValueChanged.AddListener(isMuted => audioManager.ToggleSFX(!isMuted));

        isInitialized = true;
    }

    private void OnDisable()
    {
        // Avoid listener stacking when reopening the window
        if (!isInitialized || AudioManager.Instance == null)
            return;

        musicVolumeSlider.onValueChanged.RemoveAllListeners();
        sfxVolumeSlider.onValueChanged.RemoveAllListeners();
        musicToggle.onValueChanged.RemoveAllListeners();
        sfxToggle.onValueChanged.RemoveAllListeners();
    }
}
