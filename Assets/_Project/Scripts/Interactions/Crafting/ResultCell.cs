using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultCell : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI quantityText;
    public GameObject quantityBadge;
    public Button craftButton;

    public void Setup(ItemSO item, int amount, bool canCraft, Action onCraftClicked)
    {
        icon.sprite = item.icon;
        quantityText.text = amount.ToString();
        quantityBadge.SetActive(amount > 1);

        // Clear previous listeners and add the new one
        craftButton.onClick.RemoveAllListeners();

        if (canCraft)
        {
            craftButton.onClick.AddListener(() => onCraftClicked?.Invoke());
        }

        // Visual feedback for the button state
        craftButton.interactable = canCraft;
    }
}