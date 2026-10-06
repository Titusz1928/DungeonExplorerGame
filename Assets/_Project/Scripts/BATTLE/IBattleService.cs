using System;
using System.Collections.Generic;

public interface IBattleService
{
    bool IsSkipLogEnabled { get; set; }
    int TargetedEnemyIndex { get; }

    List<EnemyController> GetActiveCombatants();
    EnemyController GetTargetedEnemy();

    void StartBattle(EnemyController mainTarget, List<EnemyController> helpers);
    void OnEnemyClicked(int index);
    void SetPendingAction(PlayerActionType action);
    void ManualTargetPart(ArmorSlot slot, string partName);
    void EndTurn();

    // Events for UI and other systems to listen to
    event Action<string> OnLogMessage;
    event Action<bool> OnTurnProcessingStateChanged; // Passes true if turn is processing
    event Action OnVisualsRefreshRequested;
    event Action OnBattleEnded;
}