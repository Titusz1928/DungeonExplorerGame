using TitusGames.Framework;
using UnityEngine;
using UnityEngine.UI;

public class BodyPartTargetButton : MonoBehaviour
{
    private ArmorSlot mySlot;
    private string myPartName;

    public void Setup(EnemyBodyPart part)
    {
        mySlot = part.associatedSlot;
        myPartName = part.partName;

        Image img = GetComponent<Image>();
        img.sprite = part.partClickSprite;
        img.raycastTarget = true; // FORCE ON

        // TEST: Lower the threshold significantly or comment it out 
        // to ensure transparency isn't the problem
        img.alphaHitTestMinimumThreshold = 0.01f;

        Button btn = GetComponent<Button>();
        btn.interactable = true;

        // Explicitly clear and re-add
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            Debug.Log("[BODYPARTBTN] Manual Listener Fired!");
            OnClicked();
        });
    }

    private void OnClicked()
    {
        Debug.Log("[BODYPARTBTN] bodypart clicked");
        // Tell the BattleManager exactly what we hit
        if (ServiceLocator.Current.TryGet<IBattleService>(out var battleService))
        {
            battleService.ManualTargetPart(mySlot, myPartName);
        }

        // Resolve UI service and hide anatomy overlay
        if (ServiceLocator.Current.TryGet<IBattleUIService>(out var uiService))
        {
            uiService.HideAnatomyOverlay();
        }
    }

    //public void test()
    //{
    //    Debug.Log("test");
    //}
}