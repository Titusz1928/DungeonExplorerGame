using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy Data")]
public class EnemySO : ScriptableObject
{
    public string enemyName;
    public Sprite sprite;
    public int maxHealth;
    public float speed;
    public float hearRange;
    public float visionRange;
    public float aggression;
    public bool isGuarding;
    public float guardRadius;
}
