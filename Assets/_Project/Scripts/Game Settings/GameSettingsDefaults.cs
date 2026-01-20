using UnityEngine;

[CreateAssetMenu(menuName = "Game/Game Settings Defaults")]
public class GameSettingsDefaults : ScriptableObject
{
    public int defaultSeed;
    public float defaultDifficulty = 1f;
    public bool cheatsActivated = false;
}
