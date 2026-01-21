using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Wandering,
    Investigating,
    Guarding,
    Searching,
    Chasing
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    private NavMeshAgent agent;

    public EnemySO data;

    [Header("Persistence")]
    public string instanceID;

    public bool IsBoss => data != null && data.isBoss;


    private EnemyState state;
    private Rigidbody2D rb;

    private Vector2 currentTarget;
    private Vector2 guardCenter;

    Vector2 facingDirection;

    public event System.Action<EnemyController> OnEnemyDeath;
    private bool reportedDeath = false;

    private float stateTimer;

    [Header("Behavior Timing")]
    private float decisionDelayMean;
    private float decisionTimer;
    private bool isWaiting;

    [Header("Smart AI Settings")]
    [SerializeField] private float searchDuration = 4f; // How long to look for player after losing them
    [SerializeField] private float searchRadius = 4f;   // How far to look around the last known spot

    private Vector2 lastKnownPosition; // Where the enemy THINKS the player is
    private bool canSeePlayer = false; // Internal flag for the current frame
    private float searchTimer;

    [Header("Corpse")]
    [SerializeField] private GameObject corpsePrefab;

    private EnemyArmorManager armorManager;
    private EnemyInjuryManager injuryManager;

    [Header("Debug")]
    [SerializeField] private TMPro.TextMeshPro stateText;

    [SerializeField] private float chaseSpeedMultiplier = 1.25f;

    public EnemyState getState()
    {
        return state;
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        rb = GetComponent<Rigidbody2D>();

        // Generate a unique ID if this is a brand new enemy
        if (string.IsNullOrEmpty(instanceID))
        {
            instanceID = System.Guid.NewGuid().ToString();
        }

        injuryManager = GetComponent<EnemyInjuryManager>();
    }

    void Start()
    {
        guardCenter = transform.position;

        EnemyStats stats = GetComponent<EnemyStats>();
        if (stats != null)
        {
            stats.Initialize(this);
        }
        else
        {
            Debug.LogWarning($"{name} is missing EnemyStats component!");
        }

        armorManager = GetComponent<EnemyArmorManager>();
        if (armorManager == null) armorManager = gameObject.AddComponent<EnemyArmorManager>();
        armorManager.Initialize(this);

        if (stateText != null)
            stateText.text = state.ToString();

        ApplyVisuals();

        SetState(
            data.isGuarding
                ? EnemyState.Guarding
                : EnemyState.Wandering
        );

        PickNewTarget();

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            agent.Warp(hit.position); // Warping is the best way to move an agent to the mesh
        }
    }

    void OnEnable()
    {
        NoiseManager.OnNoiseEmitted += OnNoiseHeard;
    }

    void OnDisable()
    {
        NoiseManager.OnNoiseEmitted -= OnNoiseHeard;
    }

    void ApplyVisuals()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && data.worldsprite != null)
        {
            sr.sprite = data.worldsprite;
        }
    }


    // --------------------------------------------------
    // UPDATE LOOP
    // --------------------------------------------------

    void Update()
    {
        // CHECK IF THERE IS A BATTLE OR NOT
        if (UIManager.Instance.IsInBattle)
            return;

        // Cheat/Settings check
        if (!GameSettings.Instance.IsVisible() && (state == EnemyState.Chasing || state == EnemyState.Investigating || state == EnemyState.Searching))
        {
            SetState(data.isGuarding ? EnemyState.Guarding : EnemyState.Wandering);
            PickNewTarget();
        }

        CheckVision();

        // We only want the "Idle/Decision" timer to stop the AI during calm states.
        // If they are Chasing or Searching, they should be moving constantly.
        if (isWaiting && state != EnemyState.Chasing && state != EnemyState.Searching)
        {
            decisionTimer -= Time.deltaTime;
            if (decisionTimer <= 0f)
            {
                isWaiting = false;
                PickNewTarget();
            }
            return;
        }

        // --- ADD THE NEW CASES HERE ---
        switch (state)
        {
            case EnemyState.Wandering:
                UpdateWandering();
                break;

            case EnemyState.Guarding:
                UpdateGuarding();
                break;

            case EnemyState.Investigating:
                UpdateInvestigating();
                break;

            case EnemyState.Chasing:
                UpdateChasing(); // New logic: Move to LKP
                break;

            case EnemyState.Searching:
                UpdateSearching(); // New logic: Look around LKP
                break;
        }
    }

    //!!!!!!
    //FOR FUTURE ENEMY INJURY SPEED PENALTIES
    //public float GetEffectiveSpeed()
    //{
    //    float speedModifier = 1.0f;

    //    if (injuryManager != null)
    //    {
    //        foreach (var injury in injuryManager.activeInjuries)
    //        {
    //            // Check for leg/feet injuries
    //            if (injury.bodyPart == ArmorSlot.Legs || injury.bodyPart == ArmorSlot.Feet)
    //            {
    //                // Severity 100 on legs reduces enemy speed by 40%
    //                float penalty = (injury.severity / 100f) * 0.4f;
    //                speedModifier -= penalty;
    //            }
    //        }
    //    }

    //    // Ensure enemies don't stop moving entirely (min 25% speed)
    //    speedModifier = Mathf.Max(0.25f, speedModifier);

    //    return data.moveSpeed * speedModifier;
    //}

    void OnNoiseHeard(NoiseEvent noise)
    {
        if (!GameSettings.Instance.IsVisible()) return;

        // Ignore own noise if needed later
        // if (noise.Source == gameObject) return;

        float distance = Vector2.Distance(transform.position, noise.Position);

        // Hearing range influenced by noise strength
        float effectiveHearingRange = data.hearRange + noise.BaseStrength;

        if (distance > effectiveHearingRange)
            return;

        // Optional: scale reaction strength
        float perceivedStrength = noise.BaseStrength * (1f - distance / effectiveHearingRange);

        // Minimum threshold to react
        if (perceivedStrength < 0.5f)
            return;

        // Guard behavior restriction
        if (state == EnemyState.Guarding && !data.isGuarding)
            return;

        // Ignore if already chasing later
        if (state == EnemyState.Chasing)
            return;

        // React!
        Investigate(noise.Position);

        Debug.DrawLine(
            transform.position,
            noise.Position,
            Color.yellow,
            0.5f
        );
    }

    public void SetState(EnemyState newState)
    {
        // If player is invisible, block entry into aggressive/alert states
        if (!GameSettings.Instance.IsVisible())
        {
            if (newState == EnemyState.Chasing ||
                newState == EnemyState.Investigating ||
                newState == EnemyState.Searching)
            {
                return;
            }
        }

        if (state == newState)
            return;


        // --- PLAY SOUND ON DETECTION ---
        if (newState == EnemyState.Chasing)
        {
            PlayDetectionSound();
        }

        state = newState;

        // Update debug text
        if (stateText != null)
        {
            stateText.text = state.ToString();
        }

        // Optional: color by state (debug)

        stateText.color = state switch
        {
            EnemyState.Wandering => Color.gray,
            EnemyState.Guarding => Color.cyan,
            EnemyState.Investigating => Color.yellow,
            EnemyState.Searching => new Color(1f, 0.5f, 0f),
            EnemyState.Chasing => Color.red,
            _ => Color.white
        };

    }

    private void PlayDetectionSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOverSFX();
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (data == null) return;

        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);

        Vector3 left = Quaternion.Euler(0, 0, -data.visionAngle / 2f) * facingDirection;
        Vector3 right = Quaternion.Euler(0, 0, data.visionAngle / 2f) * facingDirection;

        Gizmos.DrawRay(transform.position, left * data.visionRange);
        Gizmos.DrawRay(transform.position, right * data.visionRange);
        Gizmos.DrawWireSphere(transform.position, data.visionRange);
    }
#endif

    void FixedUpdate()
    {
        // Stop physics movement if in battle
        if (UIManager.Instance.IsInBattle)
        {
            rb.linearVelocity = Vector2.zero; // Stop physics sliding

            // --- ADD THIS BLOCK ---
            // We must explicitly stop the agent here because we are about to 'return'
            // preventing MoveTowardsTarget() from running.
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero; // Force immediate stop
            }
            // ----------------------

            return;
        }

        MoveTowardsTarget();
    }

    // --------------------------------------------------
    // STATE UPDATES
    // --------------------------------------------------

    void StartWaiting()
    {
        float wait = RandomGaussian(decisionDelayMean, decisionDelayMean * 0.3f);
        decisionTimer = Mathf.Max(0.2f, wait);
        isWaiting = true;

        rb.linearVelocity = Vector2.zero;
    }

    void UpdateWandering()
    {
        if (ReachedTarget())
        {
            StartWaiting();
        }
    }

    void UpdateGuarding()
    {
        if (ReachedTarget())
        {
            StartWaiting();
        }
    }

    void UpdateChasing()
    {
        GameObject player = PlayerStateManager.Instance.gameObject;
        currentTarget = player.transform.position;
    }

    void UpdateSearching()
    {
        // 1. If we see the player again, immediately resume chasing
        if (canSeePlayer)
        {
            SetState(EnemyState.Chasing);
            return;
        }

        // 2. Countdown to giving up
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0)
        {
            // Gave up, go back to guard or wander
            SetState(data.isGuarding ? EnemyState.Guarding : EnemyState.Wandering);
            PickNewTarget();
            return;
        }

        // 3. Move around the search area
        if (ReachedTarget())
        {
            // Wait a tiny bit then pick another spot near the Last Known Position
            if (!isWaiting)
            {
                StartWaiting(); // Reuse your existing wait logic
                // Override the generic waiting behavior to pick a search target next
                Invoke(nameof(PickNewSearchTarget), decisionTimer);
            }
        }
    }

    void PickNewSearchTarget()
    {
        // Pick a random point close to where we last saw the player
        Vector2 randomPoint = lastKnownPosition + Random.insideUnitCircle * searchRadius;
        currentTarget = randomPoint;
    }

    void UpdateInvestigating()
    {
        if (ReachedTarget())
        {
            SetState(
                data.isGuarding
                    ? EnemyState.Guarding
                    : EnemyState.Wandering
            );

            PickNewTarget();
        }
    }

    void CheckVision()
    {
        if (!GameSettings.Instance.IsVisible()) return;

        GameObject player = PlayerStateManager.Instance.gameObject;
        Vector2 toPlayer = (Vector2)player.transform.position - rb.position;

        // 1️⃣ Distance
        if (toPlayer.magnitude > data.visionRange)
            return;

        // 2️⃣ Angle
        float angle = Vector2.Angle(facingDirection, toPlayer);
        if (angle > data.visionAngle * 0.5f)
            return;

        // 3️⃣ Line of sight
        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            toPlayer.normalized,
            data.visionRange,
            data.visionBlockers
        );

        if (hit.collider != null)
            return; // Wall blocks vision

        // 👁️ PLAYER SEEN
        SetState(EnemyState.Chasing);
        currentTarget = player.transform.position;
    }

    // --------------------------------------------------
    // MOVEMENT
    // --------------------------------------------------

    void MoveTowardsTarget()
    {
        if (isWaiting || UIManager.Instance.IsInBattle)
        {
            if (agent.isOnNavMesh) agent.isStopped = true;
            return;
        }

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;

            // Apply Speed Increase if Chasing
            float currentSpeed = data.moveSpeed;
            if (state == EnemyState.Chasing)
            {
                currentSpeed *= chaseSpeedMultiplier;
            }

            agent.speed = currentSpeed;
            agent.SetDestination(currentTarget);

            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                facingDirection = agent.velocity.normalized;
            }
        }
    }

    bool ReachedTarget()
    {
        // SAFETY CHECK: If the agent isn't on a NavMesh yet, it can't have 'reached' a target
        if (!agent.isOnNavMesh)
        {
            return false;
        }

        if (!agent.pathPending)
        {
            // Now it is safe to check remainingDistance
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // --------------------------------------------------
    // TARGET SELECTION
    // --------------------------------------------------

    void PickNewTarget()
    {
        stateTimer = Random.Range(2f, 4f);

        if (data.isGuarding)
        {
            // This handles the "PickNewGuardTarget" logic automatically
            currentTarget = guardCenter + Random.insideUnitCircle * data.guardRadius;
        }
        else
        {
            // Standard wandering logic
            currentTarget = (Vector2)transform.position + Random.insideUnitCircle * 3f;
        }
    }

    void PickNewGuardTarget()
    {
        stateTimer = Random.Range(2f, 4f);
        currentTarget = guardCenter + Random.insideUnitCircle * data.guardRadius;
    }


    public void TickInjuries()
    {
        if (injuryManager != null) injuryManager.OnTurnEnded();
    }

    //void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (!collision.gameObject.CompareTag("Player"))
    //        return;

    //    // TEMP DEV BEHAVIOR
    //    Debug.Log("enemy dies");
    //    //Die();
    //}

    public void Die()
    {
        if (reportedDeath)
            return;

        reportedDeath = true;

        // DEBUG: Check if anyone is actually listening to this boss dying
        if (OnEnemyDeath == null)
        {
            Debug.LogWarning($"[BOSS DEBUG] {gameObject.name} died, but NO ONE was listening to OnEnemyDeath!");
        }
        else
        {
            Debug.Log($"[BOSS DEBUG] {gameObject.name} died. Notifying {OnEnemyDeath.GetInvocationList().Length} listeners.");
        }

        OnEnemyDeath?.Invoke(this);

        if (corpsePrefab != null && data.corpseContainer != null)
        {
            Vector3 pos = transform.position;
            Vector2Int cell = new Vector2Int(
                Mathf.FloorToInt(pos.x),
                Mathf.FloorToInt(pos.y)
            );

            // UNIQUE corpse ID (never reused)

            GameObject corpseObj = Instantiate(
                corpsePrefab,
                new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f),
                Quaternion.identity
            );

            WorldContainer container = corpseObj.GetComponent<WorldContainer>();
            if (container == null)
                container = corpseObj.AddComponent<WorldContainer>();

            container.SetInventory(armorManager.rawInventory, data.corpseContainer);

            // Sprite setup
            SpriteRenderer sr = corpseObj.GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = corpseObj.AddComponent<SpriteRenderer>();

            if (data.corpseContainer.containerIcon != null)
                sr.sprite = data.corpseContainer.containerIcon;

            container.sr = sr;

            //create corspe
            // Generate a completely unique ID for this corpse
            string corpseId = $"corpse_{System.Guid.NewGuid()}";

            // Initialize container with unique ID
            container.Initialize(cell, corpseId);
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // If destroyed without Die() being called (despawn, chunk unload, etc)
        if (!reportedDeath && EnemySpawnManager.Instance != null)
        {
            EnemySpawnManager.Instance.NotifyEnemyRemoved(this);
        }
    }

    // --------------------------------------------------
    // EXTERNAL HOOK (Noise later)
    // --------------------------------------------------

    public void Investigate(Vector2 position)
    {
        SetState(EnemyState.Investigating);
        currentTarget = position;
    }

    float RandomGaussian(float mean, float stdDev)
    {
        float u1 = 1f - Random.value;
        float u2 = 1f - Random.value;
        float randStdNormal =
            Mathf.Sqrt(-2f * Mathf.Log(u1)) *
            Mathf.Sin(2f * Mathf.PI * u2);

        return mean + stdDev * randStdNormal;
    }

    public EnemyState GetState() => state;
    public Vector2 GetGuardCenter() => guardCenter;

    public void SetGuardCenter(Vector2 newCenter) => guardCenter = newCenter;
}