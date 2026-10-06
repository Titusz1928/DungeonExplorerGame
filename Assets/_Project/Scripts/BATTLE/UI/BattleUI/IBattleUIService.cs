using System.Collections.Generic;
using UnityEngine;


public interface IBattleUIService
{
        GameObject treatInjuryWindow { get; }
        void RefreshAll();
        void ToggleSkipLog();
        void ToggleAnatomyOverlay();
        void ShowAnatomyOverlay();
        void HideAnatomyOverlay();
        void SwitchToLogTab();
        void SwitchToPlayerTab();

        void SetLogText(string text);
        void SetEndTurnButtonInteractable(bool interactable);
        void SetupEnemySlots(List<EnemyController> activeCombatants, int targetedIndex, System.Action<int> onEnemyClicked);
        void UpdateTargetVisuals(int targetIndex, string targetPartName);
}