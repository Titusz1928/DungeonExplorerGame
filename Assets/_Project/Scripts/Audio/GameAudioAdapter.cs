using TitusGames.Framework;
using UnityEngine;

public class GameAudioAdapter : IGameAudioService
{
    private readonly TitusGames.Framework.AudioManager _audioManager;

    // Name of the audio clip located in Resources/Audio/SFX/ LevelUp
    private const string LEVEL_UP_SFX_NAME = "LevelUp";
    private const string DETECTION_SFX_NAME = "gameover";
    private const string VICTORY_SFX_NAME = "victory";
    private const string GAMEOVER_SFX_NAME = "gameover";

    public GameAudioAdapter(TitusGames.Framework.AudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public void PlayLevelUpSFX()
    {
        // Delegates to the framework's generic PlaySFX method
        _audioManager?.PlaySFX(LEVEL_UP_SFX_NAME);
    }

    public void PlayDetectionSFX()
    {
        // Delegates to the framework's generic PlaySFX method
        _audioManager?.PlaySFX(DETECTION_SFX_NAME);
    }

    public void PlayVictorySFX()
    {
        // Delegates to the framework's generic PlaySFX method
        _audioManager?.PlaySFX(VICTORY_SFX_NAME);
    }

    public void PlayGameOverSFX()
    {
        // Delegates to the framework's generic PlaySFX method
        _audioManager?.PlaySFX(GAMEOVER_SFX_NAME);
    }

    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;
        _audioManager?.PlaySFX(clip.name, volumeMultiplier);
    }

    public void PlaySFX(string clipName, float volumeMultiplier = 1f)
    {
        if (string.IsNullOrEmpty(clipName)) return;
        _audioManager?.PlaySFX(clipName, volumeMultiplier);
    }
}