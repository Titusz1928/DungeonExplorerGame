using UnityEngine;

public static class InjurySaveBuilder
{
    public static InjurySaveData Build()
    {
        InjurySaveData saveData = new InjurySaveData();
        var manager = PlayerStateManager.Instance.GetComponent<InjuryManager>();

        if (manager == null) return saveData;

        foreach (var injury in manager.activeInjuries)
        {
            saveData.injuries.Add(new InjurySaveEntry
            {
                bodyPart = injury.bodyPart,
                type = injury.type,
                severity = injury.severity,
                healingRate = injury.healingRate,
                bleedMultiplier = injury.bleedMultiplier,
                isBandaged = injury.isBandaged,
                bandageLifetime = injury.bandageLifetime,
                bandageDirty = injury.bandageDirty
            });
        }

        return saveData;
    }

    public static void Apply(GameObject playerObj, InjurySaveData save)
    {
        if (playerObj == null || save == null) return;

        var manager = playerObj.GetComponent<InjuryManager>();
        if (manager == null) return;

        manager.activeInjuries.Clear();

        foreach (var entry in save.injuries)
        {
            // Reconstruct the Injury object from save data
            Injury injury = new Injury(entry.bodyPart, entry.type, entry.severity, entry.bleedMultiplier)
            {
                healingRate = entry.healingRate,
                isBandaged = entry.isBandaged,
                bandageLifetime = entry.bandageLifetime,
                bandageDirty = entry.bandageDirty
            };

            manager.activeInjuries.Add(injury);
        }
    }
}