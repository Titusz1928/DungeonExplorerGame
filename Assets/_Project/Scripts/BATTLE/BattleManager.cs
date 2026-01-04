using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("UI References")]
    [SerializeField] private List<Image> enemySlots; // The Image objects
    [SerializeField] private TMP_Text logText;
    [SerializeField] private Button endTurnButton;

    private List<EnemyController> activeCombatants = new List<EnemyController>();
    private int targetedEnemyIndex = 0;
    private bool isProcessingTurn = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        foreach (var slot in enemySlots) slot.gameObject.SetActive(false);
    }

    public void StartBattle(EnemyController mainTarget, List<EnemyController> helpers)
    {
        isProcessingTurn = false; // Reset turn state
        endTurnButton.interactable = true; // Ensure button is clickable
        logText.text = "You were ambushed!";

        activeCombatants.Clear();
        activeCombatants.Add(mainTarget);
        activeCombatants.AddRange(helpers);

        // Reset targeting to the first enemy
        targetedEnemyIndex = 0;

        for (int i = 0; i < enemySlots.Count; i++)
        {
            if (i < activeCombatants.Count)
            {
                EnemyController enemy = activeCombatants[i];


                enemySlots[i].sprite = enemy.data.battlesprite;
                enemySlots[i].gameObject.SetActive(true);

                // Setup the click event for this specific slot
                int index = i; // Local copy for the listener
                Button btn = enemySlots[i].GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnEnemyClicked(index));
                }
            }
            else
            {
                enemySlots[i].gameObject.SetActive(false);
            }
        }

        UpdateTargetVisuals();
        Debug.Log($"Battle started. Target: {activeCombatants[targetedEnemyIndex].data.enemyName}");
    }

    public void OnEnemyClicked(int index)
    {
        if (index >= activeCombatants.Count) return;

        targetedEnemyIndex = index;
        Debug.Log($"New Target Selected: {activeCombatants[targetedEnemyIndex].data.enemyName}");

        UpdateTargetVisuals();
    }

    private void UpdateTargetVisuals()
    {
        for (int i = 0; i < enemySlots.Count; i++)
        {
            // Find the child named "border"
            Transform border = enemySlots[i].transform.Find("Border");
            if (border != null)
            {
                // Only show border if this slot is the targeted index AND the slot is active
                border.gameObject.SetActive(i == targetedEnemyIndex && enemySlots[i].gameObject.activeSelf);
            }
        }
    }

    // Helper to get the currently selected enemy for the attack logic later
    public EnemyController GetTargetedEnemy()
    {
        return activeCombatants[targetedEnemyIndex];
    }


    public void OnEndTurnPressed()
    {
        if (isProcessingTurn) return;

        // DEBUG LOGS
        Debug.Log($"[Battle] End Turn Pressed. List Count: {activeCombatants.Count}");
        for (int i = 0; i < activeCombatants.Count; i++)
        {
            Debug.Log($"[Battle] Slot {i}: {(activeCombatants[i] != null ? activeCombatants[i].name : "NULL")}");
        }

        StartCoroutine(ExecuteBattleTurn());
    }

    private IEnumerator ExecuteBattleTurn()
    {
        // 1. Safety Check: Make sure we actually have enemies
        if (activeCombatants.Count == 0 || activeCombatants.All(e => e == null))
        {
            Debug.LogError("Battle attempted with no active combatants!");
            yield break;
        }

        isProcessingTurn = true;
        endTurnButton.interactable = false;

        // --- 1. PLAYER'S TURN ---
        EnemyController target = GetTargetedEnemy();

        // Safety: Ensure target exists and is alive
        if (target != null && target.GetComponent<EnemyStats>() != null && target.GetComponent<EnemyStats>().currentHP > 0)
        {
            yield return StartCoroutine(ResolveAttack(null, target));

            UpdateEnemySprites();

            if (CheckBattleEnd())
            {
                // We MUST reset these before quitting the coroutine
                isProcessingTurn = false;
                yield break;
            }

            yield return new WaitForSeconds(1.0f);
        }

        // --- 2. ENEMIES' TURN ---
        var currentEnemies = new List<EnemyController>(activeCombatants);
        foreach (var enemy in currentEnemies)
        {
            if (enemy == null) continue;

            var eStats = enemy.GetComponent<EnemyStats>();
            if (eStats == null || eStats.currentHP <= 0) continue;

            yield return StartCoroutine(ResolveAttack(enemy, null));

            if (PlayerStateManager.Instance.isDead)
            {
                yield return StartCoroutine(HandlePlayerDefeat());
                isProcessingTurn = false; // Reset before exit
                yield break;
            }

            yield return new WaitForSeconds(1.0f);
        }

        // --- 3. END OF TURN ---
        logText.text = "End of round. Injuries are bleeding...";

        // Trigger Injury processing for the turn
        // Access the manager on the player and trigger the turn-tick
        InjuryManager im = PlayerStateManager.Instance.GetComponent<InjuryManager>();
        if (im != null)
        {
            im.OnTurnEnded();
        }

        yield return new WaitForSeconds(1.5f);

        logText.text = "Your Turn";
        isProcessingTurn = false;
        endTurnButton.interactable = true;
    }

    private bool CheckBattleEnd()
    {
        // Check if any enemy has HP > 0
        bool anyAlive = activeCombatants.Any(e => e != null && e.GetComponent<EnemyStats>().currentHP > 0);

        if (!anyAlive)
        {
            StartCoroutine(EndBattleSequence());
            return true;
        }
        return false;
    }

    private IEnumerator EndBattleSequence()
    {
        logText.text = "<color=green>Victory! All enemies defeated.</color>";
        yield return new WaitForSeconds(2.0f);

        UIManager.Instance.ExitBattleState();
        Debug.Log("Battle Ended.");
    }

    private void UpdateEnemySprites()
    {
        for (int i = 0; i < activeCombatants.Count; i++)
        {
            if (activeCombatants[i] == null || activeCombatants[i].GetComponent<EnemyStats>().currentHP <= 0)
            {
                // Fade out or just disable
                enemySlots[i].gameObject.SetActive(false);

                // If the dead enemy was our target, switch target to the next alive enemy
                if (targetedEnemyIndex == i)
                {
                    AutoSelectNextTarget();
                }
            }
        }
        UpdateTargetVisuals();
    }

    private void AutoSelectNextTarget()
    {
        for (int i = 0; i < activeCombatants.Count; i++)
        {
            if (activeCombatants[i] != null && activeCombatants[i].GetComponent<EnemyStats>().currentHP > 0)
            {
                targetedEnemyIndex = i;
                return;
            }
        }
    }

    private IEnumerator HandlePlayerDefeat()
    {
        logText.text = "<color=red>You have been defeated...</color>";

        // 1. Get the prefab from PlayerStateManager
        GameObject gameOverPrefab = PlayerStateManager.Instance.gameOverWindow;

        if (gameOverPrefab != null)
        {
            // 2. Instantiate it as a child of THIS object (the BattleCanvas)
            // This ensures it inherits the canvas scaling and positioning
            GameObject windowInstance = Instantiate(gameOverPrefab, this.transform);

            // Optional: Ensure it spans the whole screen or centers correctly
            RectTransform rt = windowInstance.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localPosition = Vector3.zero; // Center it
            }

            Debug.Log("Game Over window spawned under BattleCanvas.");
        }
        else
        {
            Debug.LogError("No Game Over prefab found in PlayerStateManager!");
        }

        // Wait for the player to process the defeat text
        yield return new WaitForSeconds(2.0f);

        UIManager.Instance.ExitBattleState();
        SceneManagerEX.Instance.LoadScene("MainMenu");
    }

    private IEnumerator ResolveAttack(EnemyController attacker, EnemyController defender)
    {
        bool playerAttacking = attacker == null;

        // 1. Calculate Hit Chance
        float hitChance = 0.5f;
        if (playerAttacking)
        {
            int skillLevel = PlayerSkillManager.Instance.GetLevel(PlayerSkill.WeaponHandling);
            hitChance += (skillLevel * 0.02f);
            hitChance = Mathf.Clamp(hitChance, 0f, 0.95f);
        }

        // 2. Perform Accuracy Roll
        bool didHit = Random.value <= hitChance;

        string attackerName = playerAttacking ? "You" : attacker.data.enemyName;
        string targetName = playerAttacking ? defender.data.enemyName : "you";

        if (playerAttacking)
            logText.text = $"You attempt to strike the {targetName}...";
        else
            logText.text = $"The {attackerName} lunges at you...";

        yield return new WaitForSeconds(1f);

        if (!didHit)
        {
            logText.text = playerAttacking ? "You swung wide and missed!" : $"You dodged the {attackerName}'s attack!";
            yield break;
        }

        // 3. Prepare Damage Data (Includes Natural Damage & Strength)
        WeaponItemSO weapon = GetEquippedWeapon(attacker);
        DamageType dType;
        double baseDamage;

        if (playerAttacking)
        {
            // Player Damage Logic
            baseDamage = (weapon != null) ? weapon.damageAmount : 5.0;
            dType = (weapon != null) ? weapon.damageType : DamageType.Blunt;
            // Optional: Add Player Strength here if you have a PlayerStats script
        }
        else
        {
            // Enemy Damage Logic
            EnemyStats stats = attacker.GetComponent<EnemyStats>();
            if (weapon != null)
            {
                baseDamage = weapon.damageAmount;
                dType = weapon.damageType;
            }
            else
            {
                // Use Natural Damage from EnemySO if no weapon is held
                baseDamage = 5.0; // Default base for natural attacks
                dType = attacker.data.naturalDamageType;
            }

            // Apply Enemy Strength Modifier
            baseDamage += stats.strength;
        }

        // Add Randomness (90% to 110% variance)
        double rawDamage = baseDamage * Random.Range(0.9f, 1.1f);

        // 4. Identify Target Body Part
        string partName;
        ArmorSlot slot;
        if (!playerAttacking)
        {
            slot = GetRandomPlayerSlot();
            partName = slot.ToString().ToLower();
        }
        else
        {
            var randomPart = defender.data.anatomy[Random.Range(0, defender.data.anatomy.Count)];
            partName = randomPart.partName;
            slot = randomPart.associatedSlot;
        }

        // 5. Calculate Protection & Difference
        double protection = playerAttacking
            ? defender.GetComponent<EnemyArmorManager>().GetProtection(partName, dType)
            : EquipmentManager.Instance.GetTotalDefenseForSlot(slot).GetTypedDefense(dType);

        double damageDifference = rawDamage - protection;
        double finalDamage = 0;

        // 6. Tiered Injury & Damage Logic
        // Blunt weapons need a higher damage margin to cause an injury
        float injuryThreshold = (dType == DamageType.Blunt) ? 8.0f : 3.0f;

        if (damageDifference > injuryThreshold)
        {
            // TIER A: SEVERE HIT (Damage > Protection + Threshold)
            finalDamage = damageDifference;
            float severity = 20f + (float)(damageDifference * 2.0); // e.g., 20 overkill = 2.0 severity

            if (!playerAttacking)
            {
                PlayerStateManager.Instance.GetComponent<InjuryManager>().AddInjury(slot, GetInjuryType(dType), severity);
                logText.text = $"<color=red>CRITICAL!</color> The {attackerName} dealt {finalDamage:F1} damage and caused a {GetInjuryType(dType)}!";
            }
            else
            {
                // Player deals critical to enemy (even if enemies don't have an injury system yet, we show the log)
                logText.text = $"<color=orange>Brutal Hit!</color> You struck the {targetName}'s {partName} for {finalDamage:F1} damage!";
            }
        }
        else if (damageDifference > 0)
        {
            // TIER B: SUCCESSFUL HIT, NO INJURY (Damage > Protection, but below threshold)
            finalDamage = damageDifference;
            logText.text = playerAttacking
                ? $"You hit the {targetName}'s {partName} for {finalDamage:F1} damage."
                : $"The strike landed! You took {finalDamage:F1} damage, but your armor prevented a wound.";
        }
        else if (damageDifference > -5.0)
        {
            // TIER C: BRUISE / BLUNT FORCE (Damage slightly below protection)
            finalDamage = Random.Range(1f, 3f); // Minimal chip damage
            logText.text = playerAttacking
                ? $"Your blow was mostly deflected by the {targetName}'s armor."
                : $"The {attackerName} hit your {partName}! Your armor held, but you're bruised.";
        }
        else
        {
            // TIER D: TOTAL BLOCK
            finalDamage = 0;
            logText.text = playerAttacking
                ? $"The {targetName}'s armor is too thick!"
                : $"Your armor completely absorbed the impact.";
        }

        // 7. Apply Durability & Final HP Changes
        if (!playerAttacking)
        {
            ApplyArmorWear(slot, finalDamage > 0, dType);
            PlayerStateManager.Instance.inflictDamage((float)finalDamage);
        }
        else
        {
            if (EquipmentManager.Instance.mainHandWeapon != null)
            {
                var weaponInstance = EquipmentManager.Instance.mainHandWeapon;
                weaponInstance.currentDurability = System.Math.Max(0, weaponInstance.currentDurability - 1);
            }
            defender.GetComponent<EnemyStats>().TakeDamage(finalDamage);
            PlayerSkillManager.Instance.AddXP(PlayerSkill.WeaponHandling, 15f);
        }
    }

    // --- HELPERS ---

    private WeaponItemSO GetEquippedWeapon(EnemyController enemy)
    {
        if (enemy == null) return (WeaponItemSO)EquipmentManager.Instance.mainHandWeapon?.itemSO;

        // Find first weapon in enemy inventory
        var inv = enemy.GetComponent<EnemyArmorManager>().rawInventory;
        var weapon = inv.Find(i => i.itemSO is WeaponItemSO);
        return (WeaponItemSO)weapon?.itemSO;
    }

    private ArmorSlot GetRandomPlayerSlot()
    {
        // Filter out 'None' and 'Everything' flags to get only specific parts
        var validSlots = System.Enum.GetValues(typeof(ArmorSlot))
            .Cast<ArmorSlot>()
            .Where(s => s != ArmorSlot.None && IsPowerOfTwo((int)s))
            .ToList();

        return validSlots[Random.Range(0, validSlots.Count)];
    }

    private void ApplyArmorWear(ArmorSlot slot, bool isSuccessfulHit, DamageType dType)
    {
        // We only care about player armor for now as requested
        if (!EquipmentManager.Instance.equippedArmor.ContainsKey(slot)) return;

        var armorLayers = EquipmentManager.Instance.equippedArmor[slot];

        foreach (var kvp in armorLayers)
        {
            ItemInstance item = kvp.Value;
            ArmorItemSO so = item.itemSO as ArmorItemSO;
            if (so == null || !so.isBreakable) continue;

            double durabilityLoss = 0;

            if (isSuccessfulHit)
            {
                // RULE: 10% reduction + guaranteed hole for Pierce/Slash
                if (dType == DamageType.Pierce || dType == DamageType.Slash)
                {
                    durabilityLoss += so.maxDurability * 0.10;
                    item.holes++;
                    Debug.Log($"[Durability] {so.itemName} got a hole from a {dType} strike!");
                }

                // RULE: Extra point of loss per hole
                durabilityLoss += (1 + item.holes);

                // RULE: Chance for a new hole based on wear (60% dur = 40% hole chance)
                double durPercent = item.currentDurability / so.maxDurability;
                if (UnityEngine.Random.value > durPercent)
                {
                    item.holes++;
                    Debug.Log($"[Durability] {so.itemName} cracked! New hole formed.");
                }
            }
            else
            {
                // RULE: Unsuccessful strike (Blocked) -> 50% chance to lose 1 dur
                if (UnityEngine.Random.value <= 0.5f)
                {
                    durabilityLoss += 1;
                }
            }

            item.currentDurability = System.Math.Max(0, item.currentDurability - durabilityLoss);
        }
    }

    // Helper to check if it's a single flag (1, 2, 4, 8...) and not a combination
    private bool IsPowerOfTwo(int x) => (x != 0) && ((x & (x - 1)) == 0);

    private InjuryType GetInjuryType(DamageType dType) => dType switch
    {
        DamageType.Slash => InjuryType.Cut,
        DamageType.Pierce => InjuryType.Stab,
        _ => InjuryType.Fracture
    };
}