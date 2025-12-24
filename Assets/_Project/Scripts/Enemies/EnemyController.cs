using System.Security.Cryptography;
using UnityEngine;

public enum EnemyState
{
    Wandering,
    Investigating,
    Guarding,
    Searching,
    Chasing
}

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    public EnemySO data;

    [Header("Persistence")]
    public string instanceID;


    [Header("Runtime Stats")]
    public int uniqueEnemyID;
    public int maxHP;
    public int currentHP;
    public float strength; // derived from HP

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

    [Header("Corpse")]
    [SerializeField] private GameObject corpsePrefab;

    [Header("Debug")]
    [SerializeField] private TMPro.TextMeshPro stateText;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Generate a unique ID if this is a brand new enemy
        if (string.IsNullOrEmpty(instanceID))
        {
            instanceID = System.Guid.NewGuid().ToString();
        }
    }

    void Start()
    {
        guardCenter = transform.position;

        if (stateText != null)
            stateText.text = state.ToString();

        GenerateStats();
        ApplyVisuals();

        SetState(
            data.isGuarding
                ? EnemyState.Guarding
                : EnemyState.Wandering
        );

        PickNewTarget();
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
        if (sr != null && data.sprite != null)
        {
            sr.sprite = data.sprite;
        }
    }

    // --------------------------------------------------
    // STAT GENERATION
    // --------------------------------------------------

    void GenerateStats()
    {
        // HP variance
        float hpRoll = Random.Range(0.9f, 1.1f);
        maxHP = Mathf.RoundToInt(data.maxHealth * hpRoll);
        currentHP = maxHP;

        strength = (float)maxHP / data.maxHealth;

        // Decision delay variance (90–110%)
        float delayRoll = Random.Range(0.9f, 1.1f);
        decisionDelayMean = data.decisionDelay * delayRoll;

        Debug.Log(
            $"{name} | HP: {currentHP} | Strength: {strength:F2} | ThinkDelay: {decisionDelayMean:F2}"
        );
    }

    // --------------------------------------------------
    // UPDATE LOOP
    // --------------------------------------------------

    void Update()
    {
        CheckVision();

        if (isWaiting)
        {
            decisionTimer -= Time.deltaTime;
            if (decisionTimer <= 0f)
            {
                isWaiting = false;
                PickNewTarget();
            }
            return;
        }

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
        }
    }

    void OnNoiseHeard(NoiseEvent noise)
    {
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
        if (state == newState)
            return;

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
        if (isWaiting)
            return;

        Vector2 pos = rb.position;
        Vector2 toTarget = currentTarget - pos;

        if (toTarget.sqrMagnitude < 0.01f)
            return;

        facingDirection = toTarget.normalized; // 👁️ LOOK DIRECTION

        rb.MovePosition(pos + facingDirection * data.moveSpeed * Time.fixedDeltaTime);
    }

    bool ReachedTarget()
    {
        return Vector2.Distance(rb.position, currentTarget) < 0.1f;
    }

    // --------------------------------------------------
    // TARGET SELECTION
    // --------------------------------------------------

    void PickNewTarget()
    {
        stateTimer = Random.Range(2f, 4f);
        currentTarget = (Vector2)transform.position + Random.insideUnitCircle * 3f;
    }

    void PickNewGuardTarget()
    {
        stateTimer = Random.Range(2f, 4f);
        currentTarget = guardCenter + Random.insideUnitCircle * data.guardRadius;
    }

    // --------------------------------------------------
    // DAMAGE / DEATH (stub for later)
    // --------------------------------------------------

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        // TEMP DEV BEHAVIOR
        Debug.Log("enemy dies");
        Die();
    }

    void Die()
    {
        if (reportedDeath)
            return;

        reportedDeath = true;

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

            container.containerData = data.corpseContainer;

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
