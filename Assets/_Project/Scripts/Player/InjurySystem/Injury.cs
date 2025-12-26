using UnityEngine;

public enum InjuryType { Cut, Stab, Fracture }

[System.Serializable]
public class Injury
{
    public ArmorSlot bodyPart;
    public InjuryType type;
    public float severity;      // 0 to 100 (100 is fresh/dangerous)
    public float healingRate;   // How much severity drops per "tick"

    public bool isBandaged;
    public float bandageLifetime; // Seconds or Turns remaining
    public bool bandageDirty;

    public Injury(ArmorSlot part, InjuryType t, float sev)
    {
        bodyPart = part;
        type = t;
        severity = sev;
        healingRate = 1.0f; // Default healing speed
        isBandaged = false;
    }

    // Calculate how much HP this specific injury should drain right now
    public float GetDamageAmount()
    {
        if (isBandaged && !bandageDirty) return 0;

        // Example formula: Severity 100 deals 2 damage, Severity 10 deals 0.2
        return severity * 0.02f;
    }
}