using UnityEngine;

public interface IGameAudioService
{
    void PlayLevelUpSFX();
    void PlayDetectionSFX();
    void PlayVictorySFX();
    void PlayGameOverSFX();

    void PlaySFX(AudioClip clip, float volumeMultiplier = 1f);
    void PlaySFX(string clipName, float volumeMultiplier = 1f);
}
