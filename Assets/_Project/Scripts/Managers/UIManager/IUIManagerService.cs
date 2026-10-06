using UnityEngine;


public interface IUIManagerService
{
    GameObject GameObject { get; }

    bool IsWindowOpen { get; }
        bool IsInBattle { get; }
        void EnterBattleState();
        void ExitBattleState();
        void SetWindowState(bool opening);
        void ZoomIn();
        void ZoomOut();
        void deactivateExtraHUD();
        void activateExtraHUD();
        void changeVisibilityUIStatus();
}