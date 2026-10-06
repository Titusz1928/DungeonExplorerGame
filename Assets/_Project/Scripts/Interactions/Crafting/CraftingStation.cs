using System.Collections.Generic;
using TitusGames.Framework;
using UnityEngine;

public class CraftingStation : Interactable
{
    public GameObject craftingStationWindowPrefab;
    public List<RecipeSO> availableRecipes = new List<RecipeSO>();

    public override void OnInteract()
    {

        // 2. Open window
        if (!ServiceLocator.Current.TryGet<IWindowService>(out var windowService)){
            GameObject windowGO = windowService.OpenWindow(craftingStationWindowPrefab);
            CraftingStationWindow ui = windowGO.GetComponent<CraftingStationWindow>();

            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            Sprite spriteToSend = (sr != null) ? sr.sprite : null;

            if (ui != null)
            {
                ui.Initialize(this, spriteToSend);
            } 
        }
       
    }
}