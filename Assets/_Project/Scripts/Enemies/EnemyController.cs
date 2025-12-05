using UnityEngine;


public enum EnemyState
{
    Wandering,
    Investigating,
    Guarding,
    Searching,
    Chasing
}

public class EnemyController : MonoBehaviour
{
    /*public EnemySO data;
    private EnemyState state;

    private Vector2 currentTarget;
    private float stateTimer;

    void Start()
    {
        state = data.isGuarding ? EnemyState.Guarding : EnemyState.Wandering;
    }

    void Update()
    {
        switch (state)
        {
            case EnemyState.Wandering: UpdateWandering(); break;
            case EnemyState.Investigating: UpdateInvestigating(); break;
            case EnemyState.Guarding: UpdateGuarding(); break;
            case EnemyState.Searching: UpdateSearching(); break;
            case EnemyState.Chasing: UpdateChasing(); break;
        }

        CheckForPlayer();
    }

    void CheckForPlayer()
    {
        // Vision check, hearing check, etc.
    }*/
}