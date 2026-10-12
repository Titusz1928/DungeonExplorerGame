using TitusGames.Framework;

public static class GameAudioExtensions
{
    private const string LEVEL_UP_SFX = "LevelUp";
    private const string DETECTION_SFX = "gameover";
    private const string VICTORY_SFX = "victory";
    private const string GAMEOVER_SFX = "gameover";

    public static void PlayLevelUpSFX(this IAudioService audio) => audio?.PlaySFX(LEVEL_UP_SFX);
    public static void PlayDetectionSFX(this IAudioService audio) => audio?.PlaySFX(DETECTION_SFX);
    public static void PlayVictorySFX(this IAudioService audio) => audio?.PlaySFX(VICTORY_SFX);
    public static void PlayGameOverSFX(this IAudioService audio) => audio?.PlaySFX(GAMEOVER_SFX);
}