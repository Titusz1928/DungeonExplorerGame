using System.Collections;
using UnityEngine;

public class BossSpawnZone : EnemySpawnZone
{
    [Header("Boss Management")]
    public bool isPriority = true; // Added back for the Manager
    private GameObject currentBossInstance;
    private bool isBossDead = false;

    [Header("Demo Settings")]
    [SerializeField] private GameObject VictoryWindowPrefab;
    [SerializeField] private float windowDelay = 2.5f;

    public bool IsBossActive => currentBossInstance != null;

    // We remove OnEnable's InvokeRepeating because the 
    // EnemySpawnManager's TrySpawnEnemies loop will now trigger the spawn.

    private void Update()
    {
        // If the boss isn't dead, but we don't have an instance linked yet...
        if (!isBossDead && currentBossInstance == null)
        {
            FindAndLinkBoss();
        }
    }

    private void FindAndLinkBoss()
    {
        // Look for any EnemyController in the scene
        EnemyController[] allEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        foreach (var enemy in allEnemies)
        {
            // Check if this enemy is a boss and is close to this spawn zone
            if (enemy.IsBoss && Vector3.Distance(transform.position, enemy.transform.position) < radius + 5f)
            {
                currentBossInstance = enemy.gameObject;

                // Link the death event
                enemy.OnEnemyDeath -= HandleBossDeath; // Unsubscribe first to prevent double-linking
                enemy.OnEnemyDeath += HandleBossDeath;

                //Debug.Log($"[BOSS SYSTEM] Zone at {transform.position} successfully auto-linked to {enemy.name}");
                break;
            }
        }
    }

    public void OnSuccessfulSpawn(GameObject bossInstance)
    {
        if (isBossDead) return;

        currentBossInstance = bossInstance;

        // Mark as boss at the controller level
        EnemyController controller = currentBossInstance.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.OnEnemyDeath += HandleBossDeath;
        }

        //Debug.Log($"[BOSS SYSTEM] Boss spawned and linked to zone at {transform.position}");
    }

    private void HandleBossDeath(EnemyController boss)
    {
        // 1. Immediately prevent any respawns or logic loops
        isBossDead = true;

        // 2. Remove from save data immediately so it's gone even if the game crashes/closes
        WorldSaveData.Instance.RemoveObjectFromChunk(transform.position);

        // 3. Start the sequence
        //Debug.Log("[BOSS SYSTEM] Attemptimg to open victory window...");
        StartCoroutine(VictorySequenceRoutine());
    }

    private IEnumerator VictorySequenceRoutine()
    {
        //Debug.Log("[BOSS SYSTEM] Boss defeated. Waiting for battle cleanup...");

        // 1. Wait the initial delay (let death animations play)
        yield return new WaitForSeconds(windowDelay);

        // 2. Force the UI to switch back to General mode
        // This disables the BattleCanvas and enables the GeneralCanvas
        if (UIManager.Instance != null)
        {
            //Debug.Log("[BOSS SYSTEM] Switching UI to General State.");
            UIManager.Instance.ExitBattleState();
        }

        // 3. Small buffer to ensure WindowManager has updated its Root reference
        yield return new WaitForEndOfFrame();

        // 4. Open the Window 
        if (VictoryWindowPrefab != null)
        {
           // Debug.Log("[BOSS SYSTEM] Opening Victory Window on General Canvas.");
            WindowManager.Instance.OpenWindow(VictoryWindowPrefab);
        }

        // 5. Finally, destroy this zone
        //Debug.Log("[BOSS SYSTEM] Sequence complete. Removing Zone.");
        Destroy(gameObject);
    }


    public override bool CanSpawnInZone()
    {
        if (isBossDead) return false;

        if (currentBossInstance != null) return false;

        return true;
    }
}