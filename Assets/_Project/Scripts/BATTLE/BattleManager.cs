using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TitusGames.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerActionType { Attack, Treatment, EquipmentChange }

public class BattleManager : MonoBehaviour, IBattleService
{
    // --- DEPENDENCIES (Injected via GameBoot) ---
    private IBattleUIService _uiService;
    private IBattleAudioService _audioService;
    private IWindowService _windowService;
    private ISceneService _sceneService;
    private IPlayerSkillService _skillService;

    [Header("Log Speed Settings")]
    public bool isSkipLogEnabled = false;
    private float NormalWait = 1.0f;
    private float SkipWait = 0.1f;

    // --- STATE ---
    private List<EnemyController> activeCombatants = new List<EnemyController>();
    public int targetedEnemyIndex { get; private set; } = 0;
    private bool isProcessingTurn = false;

    private string currentTargetPartName;
    private ArmorSlot currentTargetSlot;
    private PlayerActionType pendingAction = PlayerActionType.Attack;

    // Required by IBattleService interface
    public bool IsSkipLogEnabled
    {
        get => isSkipLogEnabled;
        set => isSkipLogEnabled = value;
    }
    public int TargetedEnemyIndex => targetedEnemyIndex;

    public event System.Action<string> OnLogMessage;
    public event System.Action<bool> OnTurnProcessingStateChanged;
    public event System.Action OnVisualsRefreshRequested;
    public event System.Action OnBattleEnded;

    private void Awake()
    {
        // Auto-resolve all required services from ServiceLocator on load
        ServiceLocator.Current.TryGet(out _uiService);
        ServiceLocator.Current.TryGet(out _audioService);
        ServiceLocator.Current.TryGet(out _windowService);
        ServiceLocator.Current.TryGet(out _sceneService);
        ServiceLocator.Current.TryGet(out _skillService);
    }

    private void OnEnable()
    {
        ServiceLocator.Current.Register<IBattleService>(this);
    }

    private void OnDisable()
    {
        ServiceLocator.Current.Unregister<IBattleService>();
    }

    public void StartBattle(EnemyController mainTarget, List<EnemyController> helpers)
    {
        if (_uiService == null) ServiceLocator.Current.TryGet(out _uiService);
        if (_audioService == null) ServiceLocator.Current.TryGet(out _audioService);

        activeCombatants.Clear();
        activeCombatants.Add(mainTarget);
        activeCombatants.AddRange(helpers);

        // 1. Audio and Log Messages via Injected Services
        if (_audioService != null)
        {
            _audioService.PlayStartBattleSFX();

            // Check if ANY combatant in the list is a boss
            bool bossPresent = activeCombatants.Any(e => e.IsBoss);

            if (bossPresent)
            {
                Debug.Log("Playing Boss music");
                _audioService.PlayBossBattleMusic();
                _uiService?.SetLogText("Your target appears!");
            }
            else
            {
                _audioService.PlayBattleMusic();
                _uiService?.SetLogText("You were ambushed!");
            }
        }

        // 2. Reset Turn State
        isProcessingTurn = false; // Reset turn state
        _uiService?.SetEndTurnButtonInteractable(true);


        // 3. Target Setup
        targetedEnemyIndex = 0;
        if (activeCombatants.Count > 0)
        {
            RollTargetPart(activeCombatants[targetedEnemyIndex]);
        }

        // 4. UI Setup via Injected UI Service
        Debug.Log("[BattleManager] calling uiservice setupenemyslots method...");
        _uiService?.SetupEnemySlots(activeCombatants, targetedEnemyIndex, OnEnemyClicked);
        _uiService?.UpdateTargetVisuals(targetedEnemyIndex, currentTargetPartName);
        _uiService?.RefreshAll();

        Debug.Log($"Battle started. Target: {activeCombatants[targetedEnemyIndex].data.enemyName}");
    }

    public void OnEnemyClicked(int index)
    {
        if (index < 0 || index >= activeCombatants.Count) return;

        _uiService?.HideAnatomyOverlay();

        targetedEnemyIndex = index;
        Debug.Log($"New Target Selected: {activeCombatants[targetedEnemyIndex].data.enemyName}");

        RollTargetPart(activeCombatants[targetedEnemyIndex]);

        UpdateTargetVisuals();
    }

    private void UpdateTargetVisuals()
    {
        _uiService?.UpdateTargetVisuals(targetedEnemyIndex, currentTargetPartName);
    }

    // Helper to get the currently selected enemy for the attack logic later
    public EnemyController GetTargetedEnemy()
    {
        if (targetedEnemyIndex >= 0 && targetedEnemyIndex < activeCombatants.Count)
        {
            return activeCombatants[targetedEnemyIndex];
        }

        Debug.LogWarning("BattleManager: Targeted index out of range or activeCombatants is empty.");
        return null;
    }

    public void SetPendingAction(PlayerActionType action)
    {
        pendingAction = action;

        string logMessage = action switch
        {
            PlayerActionType.Treatment => "Prepared to use medical items...",
            PlayerActionType.EquipmentChange => "Preparing to swap gear...",
            _ => "Targeting enemy..."
        };

        _uiService?.SetLogText(logMessage);
    }


    public void EndTurn()
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
        _uiService?.SwitchToLogTab();

        if (activeCombatants.Count == 0 || activeCombatants.All(e => e == null))
        {
            ServiceLocator.Current.TryGet<IUIManagerService>(out var uimanagerService);
            uimanagerService.ExitBattleState();
            yield break;
        }

        isProcessingTurn = true;
        OnTurnProcessingStateChanged?.Invoke(isProcessingTurn);
        _uiService?.SetEndTurnButtonInteractable(false);

        // Initial refresh at start of turn
        _uiService?.RefreshAll();


        // --- 1. PLAYER'S TURN ---
        switch (pendingAction)
        {
            case PlayerActionType.Treatment:
                _uiService?.SetLogText("You focus on treating your wounds...");
                yield return GetWait(1.2f);
                break;

            case PlayerActionType.EquipmentChange:
                _uiService?.SetLogText("You spend the moment adjusting your equipment...");
                yield return GetWait(1.2f);
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
                        OnTurnProcessingStateChanged?.Invoke(isProcessingTurn);
                        yield break;
                    }
                    yield return GetWait();
                }
                else
                {
                    _uiService?.SetLogText("No target selected!");
                    yield return GetWait();
                }
                break;
        }

        // Reset the action to Attack for the next turn
        pendingAction = PlayerActionType.Attack;

        ServiceLocator.Current.TryGet<IPlayerStateService>(out var playerService);

        // --- 2. ENEMIES' TURN ---
        var currentEnemies = new List<EnemyController>(activeCombatants);
        foreach (var enemy in currentEnemies)
        {
            if (enemy == null || enemy.GetComponent<EnemyStats>().currentHP <= 0) continue;

            yield return StartCoroutine(ResolveAttack(enemy, null));

            if (playerService != null && playerService.IsDead)
            {
                yield return StartCoroutine(HandlePlayerDefeat());
                isProcessingTurn = false;
                OnTurnProcessingStateChanged?.Invoke(isProcessingTurn);
                yield break;
            }
            yield return GetWait();
        }

        // --- 3. END OF TURN (BLEEDING) ---
        _uiService?.SetLogText("End of round. Injuries are bleeding...");
        yield return GetWait();

        // Process Player Bleed
        InjuryManager pim = playerService.GameObject.GetComponent<InjuryManager>();
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
        _uiService?.RefreshAll();

        // FINAL CHECK: Did anyone bleed to death?
        if (playerService != null && playerService.IsDead)
        {
            yield return StartCoroutine(HandlePlayerDefeat());
            isProcessingTurn = false;
            OnTurnProcessingStateChanged?.Invoke(isProcessingTurn);
            yield break;
        }

        if (IsBattleWon())
        {
            yield return StartCoroutine(EndBattleSequence());
            isProcessingTurn = false;
            OnTurnProcessingStateChanged?.Invoke(isProcessingTurn);
            yield break;
        }

        // --- 4. START NEW ROUND ---
        _uiService?.SetLogText("Your Turn");
        isProcessingTurn = false;
        OnTurnProcessingStateChanged?.Invoke(isProcessingTurn);
        _uiService?.SetEndTurnButtonInteractable(true);
    }


    private bool IsBattleWon()
    {
        return !activeCombatants.Any(e => e != null && e.GetComponent<EnemyStats>().currentHP > 0);
    }

    private IEnumerator EndBattleSequence()
    {
        _uiService?.SetEndTurnButtonInteractable(false);
        _uiService?.SetLogText("<color=green>Victory! All enemies defeated.</color>");

        yield return GetWait(2.5f);

        _audioService?.PlayExplorationMusic();

        // Notify listeners (e.g. UIManager) that the battle has ended
        OnBattleEnded?.Invoke();

        Debug.Log("Battle Ended Successfully.");
    }

    private void UpdateEnemySprites()
    {
        for (int i = 0; i < activeCombatants.Count; i++)
        {
            var enemy = activeCombatants[i];
            bool isDeadOrNull = enemy == null || enemy.GetComponent<EnemyStats>().currentHP <= 0;

            if (isDeadOrNull && targetedEnemyIndex == i)
            {
                AutoSelectNextTarget();
            }
        }

        // Refresh slots and selection visuals through the UI service
        _uiService?.SetupEnemySlots(activeCombatants, targetedEnemyIndex, OnEnemyClicked);
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
        _uiService?.SetLogText("<color=red>You have been defeated...</color>");

        GameObject gameOverPrefab = null;

        if (ServiceLocator.Current.TryGet<IPlayerStateService>(out var playerService))
        {
            // Assuming gameOverWindow is exposed via IPlayerStateService or retrieved from component
            if (playerService is PlayerStateManager psm)
            {
                gameOverPrefab = psm.gameOverWindow;
            }
        }

        // Open via injected/resolved IWindowService
        if (gameOverPrefab != null && _windowService != null)
        {
            _windowService.OpenWindow(gameOverPrefab);
            Debug.Log("Game Over window opened via IWindowService.");
        }
        else
        {
            Debug.LogError("BattleManager: Missing Game Over prefab or IWindowService reference!");
        }

        yield return GetWait(2.5f);

        // Transition scene via injected/resolved ISceneService
        if (_sceneService != null)
        {
            _sceneService.LoadScene("MainMenu");
        }
        else
        {
            Debug.LogError("BattleManager: ISceneService is null, cannot load MainMenu!");
        }
    }

    private IEnumerator ResolveAttack(EnemyController attacker, EnemyController defender)
    {
        ServiceLocator.Current.TryGet<IPlayerStateService>(out var playerService);
        ServiceLocator.Current.TryGet<IEquipmentService>(out var equipmentService);

        bool playerAttacking = attacker == null;
        WeaponItemSO weapon = GetEquippedWeapon(attacker);

        // 1. Calculate Hit Chance
        float hitChance = 0.5f;
        if (playerAttacking)
        {
            int skillLevel = _skillService != null ? _skillService.GetLevel(PlayerSkill.WeaponHandling) : 0;
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
            AudioClip attackSound = weapon != null ? weapon.attackSound : null;
            if (attackSound != null) _audioService?.PlaySFX(attackSound);
            _uiService?.SetLogText($"You attempt to strike the {targetName}...");
        }
        else
        {
            AudioClip attackSound = attacker.data.attackSound;
            if (attackSound != null) _audioService?.PlaySFX(attackSound);
            _uiService?.SetLogText($"The {attackerName} lunges at you...");
        }

        yield return GetWait();

        if (!didHit)
        {
            string missText = playerAttacking
            ? "You swung wide and missed!"
            : $"You dodged the {attackerName}'s attack!";
            _uiService?.SetLogText(missText);
            yield break;
        }

        if (wasBlocked)
        {
            string blockText = playerAttacking
            ? $"The {targetName} <color=blue>BLOCKED</color> your strike with their shield!"
            : $"You <color=blue>BLOCKED</color> the attack!";
            _uiService?.SetLogText(blockText);

            AudioClip blockSFX = Resources.Load<AudioClip>("Audio/SFX/characters/enemies/attack/block");
            if (blockSFX != null) _audioService?.PlaySFX(blockSFX);

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
        double protection = 0;
        if (playerAttacking)
        {
            var enemyArmor = defender.GetComponent<EnemyArmorManager>();
            if (enemyArmor != null) protection = enemyArmor.GetProtection(partName, dType);
        }
        else
        {
            if (equipmentService != null)
            {
                protection = equipmentService.GetTotalDefenseForSlot(slot).GetTypedDefense(dType);
            }
        }

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
                if (playerService != null)
                {
                    var pim = playerService.GameObject.GetComponent<InjuryManager>();
                    if (pim != null) pim.AddInjury(slot, GetInjuryType(dType), severity);
                }

                // Check if the player's hit part is high-danger
                string bloodText = "";
                if (slot == ArmorSlot.Neck || slot == ArmorSlot.Head || slot == ArmorSlot.Torso)
                {
                    bloodText = " <color=red>The wound is bleeding heavily!</color>";
                }

                _uiService?.SetLogText($"<color=red>CRITICAL!</color> The {attackerName} hit your {partName} and caused a {GetInjuryType(dType)}!{bloodText}");

                AudioClip injurySFX = Resources.Load<AudioClip>("Audio/SFX/characters/enemies/attack/injury");
                if (injurySFX != null) _audioService?.PlaySFX(injurySFX);
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

                _uiService?.SetLogText($"<color=orange>Brutal Hit!</color> You struck the {targetName}'s {partName} and caused a {GetInjuryType(dType)}!{bleedNote}");

                AudioClip injurySFX = Resources.Load<AudioClip>("Audio/SFX/characters/enemies/attack/injury");
                if (injurySFX != null) _audioService?.PlaySFX(injurySFX);
            }
        }
        else if (damageDifference > 0)
        {
            // TIER B: SUCCESSFUL HIT
            finalDamage = damageDifference;
            string hitMsg = playerAttacking
                ? $"You hit the {targetName}'s {partName}."
                : "The strike landed, but your armor prevented a wound.";
            _uiService?.SetLogText(hitMsg);
        }
        else if (damageDifference > -5.0)
        {
            // TIER C: BRUISE
            finalDamage = Random.Range(1f, 3f);
            string bruiseMsg = playerAttacking
                ? $"Your blow was mostly deflected by the {targetName}'s armor."
                : $"The {attackerName} hit your {partName}! Your armor held, but you're bruised.";
            _uiService?.SetLogText(bruiseMsg);
        }
        else
        {
            // TIER D: TOTAL BLOCK
            finalDamage = 0;
            string blockMsg = playerAttacking
                ? $"The {targetName}'s armor is too thick!"
                : $"Your armor completely absorbed the {attackerName}'s impact.";
            _uiService?.SetLogText(blockMsg);
        }

        // 7. Apply Armor Wear and Final HP Changes
        if (!playerAttacking)
        {
            // Target is Player
            ApplyArmorWear(null, slot, finalDamage > 0, dType);
            playerService?.InflictDamage((float)finalDamage);
        }
        else
        {
            // Target is Enemy
            ApplyArmorWear(defender, slot, finalDamage > 0, dType);
            defender.GetComponent<EnemyStats>().TakeDamage(finalDamage);

            // Weapon Durability (Player only)
            if(equipmentService != null && equipmentService.mainHandWeapon != null)
            {
                var weaponInstance = equipmentService.mainHandWeapon;
                weaponInstance.currentDurability = System.Math.Max(0, weaponInstance.currentDurability - 1);
            }

            _skillService?.AddXP(PlayerSkill.WeaponHandling, 5f, true);
        }

        _uiService?.RefreshAll();
    }

    // --- HELPERS ---

    private WeaponItemSO GetEquippedWeapon(EnemyController enemy)
    {
        if (enemy == null) // Player
        {
            if (ServiceLocator.Current.TryGet<IEquipmentService>(out var equipmentService))
            {
                return equipmentService.mainHandWeapon?.itemSO as WeaponItemSO;
            }
            return null;
        }

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
            if (ServiceLocator.Current.TryGet<IEquipmentService>(out var equipmentService))
            {
                var offhand = equipmentService.offHandShield;
                return offhand != null && offhand.itemSO is ShieldItemSO;
            }
            return false;
        }
        else // Checking Enemy
        {
            var inv = entity.GetComponent<EnemyArmorManager>()?.rawInventory;
            var shield = inv?.Find(i => i.itemSO is ShieldItemSO);
            return shield?.itemSO is ShieldItemSO;
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
            if (ServiceLocator.Current.TryGet<IEquipmentService>(out var equipmentService))
            {
                if (!equipmentService.equippedArmor.TryGetValue(slot, out armorLayers)) return;
            }
            else
            {
                return;
            }
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
            ServiceLocator.Current.TryGet<IPlayerStateService>(out var playerStateService);
            var pim = playerStateService.GameObject.GetComponent<InjuryManager>();
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
            ServiceLocator.Current.TryGet<IPlayerStateService>(out var playerStateService);
            var pim = playerStateService.GameObject.GetComponent<InjuryManager>();
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