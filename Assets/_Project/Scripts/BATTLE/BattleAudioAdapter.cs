using UnityEngine;
using TitusGames.Framework;

public class BattleAudioAdapter : IBattleAudioService
{
    private readonly AudioManager _frameworkAudioManager;

    public BattleAudioAdapter(AudioManager frameworkAudioManager)
    {
        _frameworkAudioManager = frameworkAudioManager;
    }

    public void PlayStartBattleSFX()
    {
        _frameworkAudioManager?.PlaySFX("battle_start");
    }

    public void PlayBossBattleMusic()
    {
        _frameworkAudioManager?.PlayMusic("boss_battle_theme");
    }

    public void PlayBattleMusic()
    {
        _frameworkAudioManager?.PlayMusic("battle_theme");
    }

    public void PlayExplorationMusic()
    {
        _frameworkAudioManager?.PlayMusic("exploration_theme");
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && _frameworkAudioManager != null)
        {
            _frameworkAudioManager.PlaySFX(clip.name);
        }
    }

    public void PlaySFX(string clipName)
    {
        _frameworkAudioManager?.PlaySFX(clipName);
    }
}