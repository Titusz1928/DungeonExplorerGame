using TitusGames.Framework;
using UnityEngine;

public class BattleAudioAdapter : MonoBehaviour, IBattleAudioService
{
    [Header("Audio Clips & Tracks")]
    [SerializeField] private AudioClip startBattleSFX;
    [SerializeField] private string battleMusicTrack = "battlemusic";
    [SerializeField] private string bossMusicTrack = "bigbattlemusic";
    [SerializeField] private string explorationMusicTrack = "GameTheme";

    private void OnEnable()
    {
        ServiceLocator.Current.Register<IBattleAudioService>(this);
    }

    private void OnDisable()
    {
        ServiceLocator.Current.Unregister<IBattleAudioService>();
    }

    public void PlayStartBattleSFX()
    {
        if (ServiceLocator.Current.TryGet<IAudioService>(out var audio))
        {
            audio.PlaySFX(startBattleSFX);
        }
    }

    public void PlayBattleMusic()
    {
        if (ServiceLocator.Current.TryGet<IAudioService>(out var audio))
        {
            audio.PlayMusic(battleMusicTrack, fade: true);
        }
    }

    public void PlayBossBattleMusic()
    {
        if (ServiceLocator.Current.TryGet<IAudioService>(out var audio))
        {
            audio.PlayMusic(bossMusicTrack, fade: true);
        }
    }

    public void PlayExplorationMusic()
    {
        if (ServiceLocator.Current.TryGet<IAudioService>(out var audio))
        {
            audio.PlayMusic(explorationMusicTrack, fade: true);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (ServiceLocator.Current.TryGet<IAudioService>(out var audio))
        {
            audio.PlaySFX(clip);
        }
    }

    public void PlaySFX(string clipName)
    {
        if (ServiceLocator.Current.TryGet<IAudioService>(out var audio))
        {
            audio.PlaySFX(clipName);
        }
    }
}