using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum NoiseActionType
{
    Walking,
    Sprinting,
    StealthKill
}


public enum NoisePreset
{
    DoorOpen,
    ChestOpen,
    DoorSlam,
    ObjectBreak
}


public class NoiseManager : MonoBehaviour
{
    private class DebugNoise
    {
        public NoiseEvent noise;
        public float timeRemaining;

        public DebugNoise(NoiseEvent noise)
        {
            this.noise = noise;
            timeRemaining = noise.Lifetime;
        }
    }

    public static NoiseManager Instance;

    public static event Action<NoiseEvent> OnNoiseEmitted;

    [Header("Movement Noise")]
    public float walkingNoise = 6f;
    public float sprintingNoise = 14f;
    public float stealthKillNoise = 3f;

    [Header("Fixed Noise Presets")]
    public float doorOpenNoise = 8f;
    public float chestOpenNoise = 5f;
    public float objectBreakNoise = 12f;


    [Header("Debug")]
    public bool debugDrawNoise = true;

    private List<DebugNoise> activeDebugNoises = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Add one persistent test noise for debugging
        if (debugDrawNoise)
        {
            Vector3 testPos = transform.position + Vector3.right * 2f;
            NoiseEvent testNoise = new NoiseEvent(testPos, 5f, 10f, gameObject);
            activeDebugNoises.Add(new DebugNoise(testNoise));
        }
    }


    private void Update()
    {
        if (!debugDrawNoise) return;

        for (int i = activeDebugNoises.Count - 1; i >= 0; i--)
        {
            activeDebugNoises[i].timeRemaining -= Time.deltaTime;
            if (activeDebugNoises[i].timeRemaining <= 0f)
            {
                activeDebugNoises.RemoveAt(i);
            }
        }
    }

    // ---------------------------------------------
    // 2D Editor Gizmos Drawing
    // ---------------------------------------------
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !debugDrawNoise || activeDebugNoises == null)
            return;

        foreach (var debugNoise in activeDebugNoises)
        {
            float radius = debugNoise.noise.BaseStrength;
            float t = debugNoise.timeRemaining / debugNoise.noise.Lifetime;

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f * t);

            Vector3 center = debugNoise.noise.Position;
            center.z = 0f;

            DrawCircle(center, radius, 32);
        }
    }

    // Approximate a circle with line segments
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
#endif


    // ---------------------------------------------------------
    // SKILL-INFLUENCED NOISE
    // ---------------------------------------------------------
    public void EmitActionNoise(NoiseActionType actionType, Vector3 position)
    {
        float baseStrength = GetBaseActionNoise(actionType);
        float finalStrength = ApplySneakModifiers(baseStrength);

        EmitNoise(finalStrength, position); // use passed position
    }

    // ---------------------------------------------------------
    // FIXED NOISE (doors, chests, etc.)
    // ---------------------------------------------------------
    public void EmitFixedNoise(NoisePreset preset, Vector3 position)
    {
        float strength = GetFixedNoise(preset);
        EmitNoise(strength, position);
    }

    // ---------------------------------------------------------
    // INTERNAL HELPERS
    // ---------------------------------------------------------
    void EmitNoise(float strength, Vector3 position)
    {
        NoiseEvent noise = new NoiseEvent(
            position,
            strength,
            1.5f,
            gameObject
        );

        //Debug.Log(           $"[NOISE] Strength: {strength:F2} | Pos: {position} | Source: {gameObject.name}");

        if (debugDrawNoise)
        {
            activeDebugNoises.Add(new DebugNoise(noise));
        }

        OnNoiseEmitted?.Invoke(noise);
    }

    float GetBaseActionNoise(NoiseActionType type)
    {
        switch (type)
        {
            case NoiseActionType.Walking:
                return walkingNoise;

            case NoiseActionType.Sprinting:
                return sprintingNoise;

            case NoiseActionType.StealthKill:
                return stealthKillNoise;

            default:
                return 0f;
        }
    }

    float GetFixedNoise(NoisePreset preset)
    {
        switch (preset)
        {
            case NoisePreset.DoorOpen:
                return doorOpenNoise;

            case NoisePreset.ChestOpen:
                return chestOpenNoise;

            case NoisePreset.ObjectBreak:
                return objectBreakNoise;

            default:
                return 0f;
        }
    }

    float ApplySneakModifiers(float baseNoise)
    {
        // Equipment influence (makes noise stronger)
        float equipmentSneak = EquipmentManager.Instance.GetTotalSneak();
        float equipmentModifier = 1f + (equipmentSneak * 0.005f); // e.g., +0.5% noise per equipment point

        // Stealth skill (reduces noise)
        int stealthLevel = PlayerSkillManager.Instance.GetLevel(PlayerSkill.Stealth);
        float stealthModifier = Mathf.Lerp(1f, 0.5f, stealthLevel / 10f); // reduces noise with skill

        return baseNoise * equipmentModifier * stealthModifier;
    }
}
