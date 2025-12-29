using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Enemy Data")]
public class EnemySO : ScriptableObject
{
    [Header("Visual")]
    public string enemyName;
    public Sprite worldsprite;
    public Sprite battlesprite;
    public float spriteSizeX;
    public float spriteSizeY;

    [Header("Stats")]
    public int enemyID;
    public int maxHealth = 100;
    public float moveSpeed = 2f;

    [Header("Perception")]
    public float hearRange = 8f;
    [Range(0f, 1f)] public float aggression = 0.5f;

    [Header("Vision")]
    public float visionRange = 6f;
    [Range(0f, 360f)]
    public float visionAngle = 90f;
    public LayerMask visionBlockers;

    [Header("Behavior")]
    public bool isGuarding;
    public float guardRadius = 4f;

    [Header("Behavior Timing")]
    public float decisionDelay;

    [Header("Loot")]
    public ContainerSO corpseContainer; // your existing container system


}

