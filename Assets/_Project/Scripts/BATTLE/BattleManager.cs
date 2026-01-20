using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerActionType { Attack, Treatment, EquipmentChange }

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("UI References")]
    [SerializeField] private List<Image> enemySlots; // The Image objects
    [SerializeField] private TMP_Text logText;
    [SerializeField] private Button endTurnButton;

    [Header("Log Speed Settings")]
    public bool isSkipLogEnabled = false;
    private float NormalWait = 1.0f;
    private float SkipWait = 0.1f;

    private List<EnemyController> activeCombatants = new List<EnemyController>();
    public int targetedEnemyIndex = 0;
    private bool isProcessingTurn = false;

    [Header("Targeting Data")]
    private string currentTargetPartName;
    private ArmorSlot currentTargetSlot;

    private PlayerActionType pendingAction = PlayerActionType.Attack;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        foreach (var slot in enemySlots) slot.gameObject.SetActive(false);
    }

    public void StartBattle(EnemyController mainTarget, List<EnemyController> helpers)
    {
        activeCombatants.Clear();
        activeCombatants.Add(mainTarget);
        activeCombatants.AddRange(helpers);

        // 2. Determine which music to play
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayStartBattleSFX();

            // Check if ANY combatant in the list is a boss
            bool bossPresent = activeCombatants.Any(e => e.IsBoss);

            if (bossPresent)
            {
                Debug.Log("Playing Boss music");
                AudioManager.Instance.PlayBossBattleMusic();
                logText.text = "Your target appears!";
            }
            else
            {
                logText.text = "You were ambushed!";
                AudioManager.Instance.PlayBattleMusic();
            }
        }

        isProcessingTurn = false; // Reset turn state
        endTurnButton.interactable = true; // Ensure button is clickable


        // Reset targeting to the first enemy
        targetedEnemyIndex = 0;

        // 2. NEW: Roll the initial target part for the first enemy
        if (activeCombatants.Count > 0)
        {
            RollTargetPart(activeCombatants[targetedEnemyIndex]);
        }

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
        BattleUIManager.Instance.RefreshAll();

        Debug.Log($"Battle started. Target: {activeCombatants[targetedEnemyIndex].data.enemyName}");
    }

    public void OnEnemyClicked(int index)
    {
        if (index >= activeCombatants.Count) return;

        // NEW: Close any open anatomy overlays before changing targets
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.HideAnatomyOverlay();
        }

        targetedEnemyIndex = index;
        Debug.Log($"New Target Selected: {activeCombatants[targetedEnemyIndex].data.enemyName}");

        RollTargetPart(activeCombatants[targetedEnemyIndex]);

        UpdateTargetVisuals();
    }

    private void UpdateTargetVisuals()
    {
        for (int i = 0; i < enemySlots.Count; i++)
        {
            // SAFETY 1: Check if the slot itself exists in the list
            if (enemySlots[i] == null)
            {
                Debug.LogWarning($"BattleManager: Slot at index {i} is null!");
                continue;
            }

            Transform border = enemySlots[i].transform.Find("Border");
            Transform bodypartselector = enemySlots[i].transform.Find("TargetBodypartSelector");

            bool isThisTarget = (i == targetedEnemyIndex && enemySlots[i].gameObject.activeSelf);

            // Update Border
            if (border != null) border.gameObject.SetActive(isThisTarget);

            // Update Selector
            if (bodypartselector != null)
            {
                bodypartselector.gameObject.SetActive(isThisTarget);

                if (isThisTarget)
                {
                    TMPro.TextMeshProUGUI partText = bodypartselector.GetComponentInChildren<TMPro.TextMeshProUGUI>();

                    if (partText != null)
                    {
                        // SAFETY 2: Ensure we actually have a part name before converting to Upper
                        if (!string.IsNullOrEmpty(currentTargetPartName))
                        {
                            partText.text = currentTargetPartName.ToUpper();
                        }
                        else
                        {
                            partText.text = "???"; // Fallback
                            Debug.LogWarning("BattleManager: currentTargetPartName is null or empty!");
                        }
                    }
                }
            }
        }
    }

    // Helper to get the currently selected enemy for the attack logic later
    public EnemyController GetTargetedEnemy()
    {
        return activeCombatants[targetedEnemyIndex];
    }

    public void SetPendingAction(PlayerActionType action)
    {
        pendingAction = action;

        // Optional: Log it so the player knows what they just did
        logText.text = action switch
        {
            PlayerActionType.Treatment => "Prepared to use medical items...",
            PlayerActionType.EquipmentChange => "Preparing to swap gear...",
            _ => "Targeting enemy..."
        };
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
        BattleUIManager.Instance.SwitchToLogTab();

        if (activeCombatants.Count == 0 || activeCombatants.All(e => e == null))
        {
            UIManager.Instance.ExitBattleState();
            yield break;
        }

        isProcessingTurn = true;
        endTurnButton.interactable = false;

        // Initial refresh at start of turn
        BattleUIManager.Instance.RefreshAll();

        // --- 1. PLAYER'S TURN ---
        switch (pendingAction)
        {
            case PlayerActionType.Treatment:
                logText.text = "You focus on treating your wounds...";
                yield return GetWait(1.2f);
                // Note: The logic (healing/bandaging) already happened in the UI script
                break;

            case PlayerActionType.EquipmentChange:
                logText.text = "You spend the moment adjusting your equipment...";
                yield return GetWait(1.2f);
                // Note: The equip/unequip already happened in EquipmentRowPrefab
                break;

            case PlayerActionType.Attack:
            default:
                EnemyController target = GetTargetedEnemy();
                if (target != null && target.GetComponent<EnemyStats>().currentHP > 0)
                {
                    yield return StartCoroutine(ResolveAttack(null, target));
                    UpdateEnemySprites();

                    if (IsBattleWon())
                    {
                        yield return StartCoroutine(EndBattleSequence());
                        isProcessingTurn = false;
                        yield break;
                    }
                    yield return GetWait();
                }
                else
                {
                    logText.text = "No target selected!";
                    yield return GetWait();
                }
                break;
        }

        // Reset the action to Attack for the next turn
        pendingAction = PlayerActionType.Attack;

        // --- 2. ENEMIES' TURN ---
        var currentEnemies = new List<EnemyController>(activeCombatants);
        foreach (var enemy in currentEnemies)
        {
            if (enemy == null || enemy.GetComponent<EnemyStats>().currentHP <= 0) continue;

            yield return StartCoroutine(ResolveAttack(enemy, null));

            if (PlayerStateManager.Instance.isDead)
            {
                yield return StartCoroutine(HandlePlayerDefeat());
                isProcessingTurn = false;
                yield break;
            }
            yield return GetWait();
        }

        // --- 3. END OF TURN (BLEEDING) ---
        logText.text = "End of round. Injuries are bleeding...";
        yield return GetWait();

        // Process Player Bleed
        InjuryManager pim = PlayerStateManager.Instance.GetComponent<InjuryManager>();
        if (pim != null) pim.OnTurnEnded();

        // Process Enemy Bleed
        foreach (var enemy in activeCombatants)
        {
            if (enemy != null && enemy.GetComponent<EnemyStats>().currentHP > 0)
            {
                var eim = enemy.GetComponent<EnemyInjuryManager>();
                if (eim != null) eim.OnTurnEnded();
            }
        }

        yield return GetWait();
        UpdateEnemySprites();
        BattleUIManager.Instance.RefreshAll();

        // FINAL CHECK: Did anyone bleed to death?
        if (PlayerStateManager.Instance.isDead)
        {
            yield return StartCoroutine(HandlePlayerDefeat());
            isProcessingTurn = false;
            yield break;
        }

        if (IsBattleWon())
        {
            yield return StartCoroutine(EndBattleSequence());
            isProcessingTurn = false;
            yield break;
        }

        // --- 4. START NEW ROUND ---
        logText.text = "Your Turn";
        isProcessingTurn = false;
        endTurnButton.interactable = true;
    }


    private bool IsBattleWon()
    {
        return !activeCombatants.Any(e => e != null && e.GetComponent<EnemyStats>().currentHP > 0);
    }

    private IEnumerator EndBattleSequence()
    {
        // Ensure button is off so player can't click during sequence
        endTurnButton.interactable = false;

        logText.text = "<color=green>Victory! All enemies defeated.</color>";
        yield return new WaitForSeconds(2.5f);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayExplorationMusic();
        }

        UIManager.Instance.ExitBattleState();

        // Optional: Give XP or Loot here
        Debug.Log("Battle Ended Successfully.");
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

    // Inside BattleManager.cs
    public void ManualTargetPart(ArmorSlot slot, string partName)
    {
        currentTargetSlot = slot;
        currentTargetPartName = partName;

        // Refresh the text label to show the manual selection
        UpdateTargetVisuals();
        Debug.Log($"Manually targeted: {partName}");
    }

    public void RollTargetPart(EnemyController defender)
    {
        if (defender == null) return;

        // Pick random part from the enemy's unique anatomy
        var randomPart = defender.data.anatomy[Random.Range(0, defender.data.anatomy.Count)];

        currentTargetPartName = randomPart.partName;
        currentTargetSlot = randomPart.associatedSlot;
    }

    private IEnumerator HandlePlayerDefeat()
    {
        logText.text = "<color=red>You have been defeated...</color>";

        // 1. Get the prefab
        GameObject gameOverPrefab = PlayerStateManager.Instance.gameOverWindow;

        if (gameOverPrefab != null)
        {
            // 2. Open via WindowManager
            // Because UIManager.EnterBattleState already registered 'battleWindowRoot',
            // this will spawn in the correct canvas automatically.
            WindowManager.Instance.OpenWindow(gameOverPrefab);

            Debug.Log("Game Over window opened via WindowManager in Battle Root.");
        }
        else
        {
            Debug.LogError("No Game Over prefab found!");
        }

        // 3. Pause for the player to see the log message
        yield return new WaitForSeconds(2.5f);

        // Note: Usually, the Game Over window buttons (like "Exit to Menu") 
        // will handle the SceneManager call, but if you want to force it:
        //UIManager.Instance.ExitBattleState();
        SceneManagerEX.Instance.LoadScene("MainMenu");
    }

    private IEnumerator ResolveAttack(EnemyController attacker, EnemyController defender)
    {
        bool playerAttacking = attacker == null;
        WeaponItemSO weapon = GetEquippedWeapon(attacker);

        // 1. Calculate Hit Chance
        float hitChance = 0.5f;
        if (playerAttacking)
        {
            int skillLevel = PlayerSkillManager.Instance.GetLevel(PlayerSkill.WeaponHandling);
            hitChance += (skillLevel * 0.02f);
            hitChance = Mathf.Clamp(hitChance, 0f, 0.95f);
        }

        hitChance -= GetInjuryAccuracyPenalty(attacker);

        // 2. Perform Accuracy Roll
        bool didHit = Random.value <= hitChance;

        // NEW: Check for Shield Block (Defender)
        bool wasBlocked = false;
        if (didHit && HasShieldEquipped(defender))
        {
            if (Random.value <= 0.15f)
            { // 15% Block Chance
                wasBlocked = true;
            }
        }

        string attackerName = playerAttacking ? "You" : attacker.data.enemyName;
        string targetName = playerAttacking ? defender.data.enemyName : "you";

        if (playerAttacking)
        {
            AudioClip attacksound = weapon.attackSound;
            AudioManager.Instance.PlaySFX(attacksound);
            logText.text = $"You attempt to strike the {targetName}...";
        }
        else
        {
            AudioClip attacksound = attacker.data.attackSound;
            AudioManager.Instance.PlaySFX(attacksound);
            logText.text = $"The {attackerName} lunges at you...";
        }

        yield return GetWait();

        if (!didHit)
        {
            logText.text = playerAttacking ? "You swung wide and missed!" : $"You dodged the {attackerName}'s attack!";
            yield break;
        }

        if (wasBlocked)
        {
            logText.text = playerAttacking ? $"The {targetName} <color=blue>BLOCKED</color> your strike with their shield!" : $"You <color=blue>BLOCKED</color> the attack!";

            AudioClip injury = Resources.Load<AudioClip>("Audio/SFX/characters/enemies/attack/block");
            AudioManager.Instance.PlaySFX(injury);

            // Optional: Damage shield durability here
            yield break;
        }

        // 3. Prepare Damage Data
        DamageType dType;
        double baseDamage;

        if (playerAttacking)
        {
            baseDamage = (weapon != null) ? weapon.damageAmount : 5.0;
            dType = (weapon != null) ? weapon.damageType : DamageType.Blunt;
        }
        else
        {
            EnemyStats stats = attacker.GetComponent<EnemyStats>();
            if (weapon != null)
            {
                baseDamage = weapon.damageAmount;
                dType = weapon.damageType;
            }
            else
            {
                baseDamage = 5.0;
                dType = attacker.data.naturalDamageType;
            }
            baseDamage += stats.strength;
        }

        double rawDamage = baseDamage * GetInjuryDamageMultiplier(attacker);
        rawDamage *= Random.Range(0.9f, 1.1f);

        // 4. Identify Target Body Part
        string partName;
        ArmorSlot slot;

        if (!playerAttacking)
        {
            // ENEMIES: Still roll randomly when they hit the player
            slot = GetRandomPlayerSlot();
            partName = slot.ToString().ToLower();
        }
        else
        {
            // PLAYER: Use the data we already "Locked In" via the UI
            slot = currentTargetSlot;
            partName = currentTargetPartName;
        }

        // 5. Calculate Protection
        double protection = playerAttacking
            ? defender.GetComponent<EnemyArmorManager>().GetProtection(partName, dType)
            : EquipmentManager.Instance.GetTotalDefenseForSlot(slot).GetTypedDefense(dType);

        double damageDifference = rawDamage - protection;
        double finalDamage = 0;
        float injuryThreshold = (dType == DamageType.Blunt) ? 8.0f : 3.0f;

        // 6. Tiered Injury & Damage Logic
        if (damageDifference > injuryThreshold)
        {
            // TIER A: SEVERE HIT
            finalDamage = damageDifference;
            float severity = 20f + (float)(damageDifference * 2.0);

            if (!playerAttacking)
            {
                PlayerStateManager.Instance.GetComponent<InjuryManager>().AddInjury(slot, GetInjuryType(dType), severity);

                // Check if the player's hit part is high-danger
                string bloodText = "";
                if (slot == ArmorSlot.Neck || slot == ArmorSlot.Head || slot == ArmorSlot.Torso)
                {
                    bloodText = " <color=red>The wound is bleeding heavily!</color>";
                }

                logText.text = $"<color=red>CRITICAL!</color> The {attackerName} hit your {partName} and caused a {GetInjuryType(dType)}!{bloodText}";

                AudioClip injury = Resources.Load<AudioClip>("Audio/SFX/characters/enemies/attack/injury");
                AudioManager.Instance.PlaySFX(injury);
            }
            else
            {
                // 1. Add the injury to the enemy
                defender.GetComponent<EnemyInjuryManager>().AddInjury(slot, GetInjuryType(dType), severity);

                // 2. Check for profound bleeding text
                string bleedNote = "";
                var partData = defender.data.anatomy.Find(p => p.associatedSlot == slot);

                // If the multiplier is high (e.g., > 1.2), add the flavor text
                if (partData != null && partData.bleedMultiplier > 1.2f)
                {
                    bleedNote = " <color=#FF3333>It's bleeding profusely!</color>";
                }

                logText.text = $"<color=orange>Brutal Hit!</color> You struck the {targetName}'s {partName} and caused a {GetInjuryType(dType)}!{bleedNote}";

                AudioClip injury = Resources.Load<AudioClip>("Audio/SFX/characters/enemies/attack/injury");
                AudioManager.Instance.PlaySFX(injury);
            }
        }
        else if (damageDifference > 0)
        {
            // TIER B: SUCCESSFUL HIT
            finalDamage = damageDifference;
            logText.text = playerAttacking
                ? $"You hit the {targetName}'s {partName}"
                : $"The strike landedbut your armor prevented a wound.";
        }
        else if (damageDifference > -5.0)
        {
            // TIER C: BRUISE
            finalDamage = Random.Range(1f, 3f);
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
                : $"Your armor completely absorbed the {attackerName}'s impact.";
        }

        // 7. Apply Armor Wear and Final HP Changes
        if (!playerAttacking)
        {
            // Target is Player
            ApplyArmorWear(null, slot, finalDamage > 0, dType);
            PlayerStateManager.Instance.inflictDamage((float)finalDamage);
        }
        else
        {
            // Target is Enemy
            ApplyArmorWear(defender, slot, finalDamage > 0, dType);
            defender.GetComponent<EnemyStats>().TakeDamage(finalDamage);

            // Weapon Durability (Player only)
            if (EquipmentManager.Instance.mainHandWeapon != null)
            {
                var weaponInstance = EquipmentManager.Instance.mainHandWeapon;
                weaponInstance.currentDurability = System.Math.Max(0, weaponInstance.currentDurability - 1);
            }

            PlayerSkillManager.Instance.AddXP(PlayerSkill.WeaponHandling, 5f, true);
        }

        BattleUIManager.Instance.RefreshAll();
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

    // Helper to check for shields on either Player or Enemy
    private bool HasShieldEquipped(EnemyController entity)
    {
        if (entity == null) // Checking Player
        {
            // Check if player has an item in the off-hand that is a shield
            var offhand = EquipmentManager.Instance.offHandShield; // Adjust to your variable name
            return offhand != null && offhand.itemSO is ShieldItemSO shield;
        }
        else // Checking Enemy
        {
            var inv = entity.GetComponent<EnemyArmorManager>().rawInventory;
            var shield = inv.Find(i => i.itemSO is ShieldItemSO);
            return (ShieldItemSO)shield?.itemSO;
        }
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

    private void ApplyArmorWear(EnemyController enemy, ArmorSlot slot, bool isSuccessfulHit, DamageType dType)
    {
        // Get the correct armor dictionary based on whether target is player or enemy
        Dictionary<ArmorLayer, ItemInstance> armorLayers;

        if (enemy == null) // Player
        {
            if (!EquipmentManager.Instance.equippedArmor.ContainsKey(slot)) return;
            armorLayers = EquipmentManager.Instance.equippedArmor[slot];
        }
        else // Enemy
        {
            EnemyArmorManager eArmor = enemy.GetComponent<EnemyArmorManager>();
            if (!eArmor.equippedArmor.ContainsKey(slot)) return;
            armorLayers = eArmor.equippedArmor[slot];
        }

        foreach (var kvp in armorLayers)
        {
            ItemInstance item = kvp.Value;
            ArmorItemSO so = item.itemSO as ArmorItemSO;
            if (so == null || !so.isBreakable) continue;

            double durabilityLoss = 0;

            if (isSuccessfulHit)
            {
                if (dType == DamageType.Pierce || dType == DamageType.Slash)
                {
                    durabilityLoss += so.maxDurability * 0.10;
                    item.holes++;
                }
                durabilityLoss += (1 + item.holes);

                if (UnityEngine.Random.value > (item.currentDurability / so.maxDurability))
                    item.holes++;
            }
            else if (UnityEngine.Random.value <= 0.5f)
            {
                durabilityLoss += 1;
            }

            item.currentDurability = System.Math.Max(0, item.currentDurability - durabilityLoss);
        }
    }

    private float GetInjuryAccuracyPenalty(EnemyController entity)
    {
        float penalty = 0f;
        List<Injury> activeInjuries;

        if (entity == null) // Player
        {
            var pim = PlayerStateManager.Instance.GetComponent<InjuryManager>();
            // Ignore injuries that are bandaged for accuracy penalties
            activeInjuries = pim.activeInjuries.FindAll(i => !i.isBandaged);
        }
        else // Enemy
        {
            activeInjuries = entity.GetComponent<EnemyInjuryManager>().activeInjuries;
        }

        foreach (var injury in activeInjuries)
        {
            // Arms, Hands, and Head hurt accuracy the most
            if (injury.bodyPart == ArmorSlot.Arms || injury.bodyPart == ArmorSlot.Hands)
                penalty += 0.10f;
            else if (injury.bodyPart == ArmorSlot.Head)
                penalty += 0.05f;
            else
                penalty += 0.02f; // Minor penalty for other body parts (pain/distraction)
        }

        return penalty;
    }

    private float GetInjuryDamageMultiplier(EnemyController entity)
    {
        float multiplier = 1.0f;
        List<Injury> activeInjuries;

        if (entity == null) // Player
        {
            var pim = PlayerStateManager.Instance.GetComponent<InjuryManager>();
            activeInjuries = pim.activeInjuries.FindAll(i => !i.isBandaged);
        }
        else
        {
            activeInjuries = entity.GetComponent<EnemyInjuryManager>().activeInjuries;
        }

        foreach (var injury in activeInjuries)
        {
            // Only Arms and Hands reduce physical damage output
            if (injury.bodyPart == ArmorSlot.Arms || injury.bodyPart == ArmorSlot.Hands)
            {
                multiplier -= 0.15f; // 15% less damage per arm/hand injury
            }
        }

        return Mathf.Max(0.5f, multiplier); // Cap damage reduction at 50%
    }

    // Add this inside BattleManager.cs
    public List<EnemyController> GetActiveCombatants()
    {
        return activeCombatants;
    }

    // Helper to check if it's a single flag (1, 2, 4, 8...) and not a combination
    private bool IsPowerOfTwo(int x) => (x != 0) && ((x & (x - 1)) == 0);

    private InjuryType GetInjuryType(DamageType dType) => dType switch
    {
        DamageType.Slash => InjuryType.Cut,
        DamageType.Pierce => InjuryType.Stab,
        _ => InjuryType.Fracture
    };

    // Helper property to get the current wait duration
    private WaitForSeconds GetWait(float customNormal = 1.0f)
    {
        return new WaitForSeconds(isSkipLogEnabled ? SkipWait : customNormal);
    }
}