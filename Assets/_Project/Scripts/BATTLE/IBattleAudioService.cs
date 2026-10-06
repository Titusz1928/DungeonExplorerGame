using UnityEngine;

public interface IBattleAudioService
{
    void PlayStartBattleSFX();
    void PlayBossBattleMusic();
    void PlayBattleMusic();
    void PlayExplorationMusic();
    void PlaySFX(AudioClip clip);
    void PlaySFX(string clipName);
}