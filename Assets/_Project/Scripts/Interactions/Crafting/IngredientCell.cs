using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientCell : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI quantityText;
    public GameObject quantityBadge;

    public void Setup(ItemSO item, int amount)
    {
        icon.sprite = item.icon;
        quantityText.text = amount.ToString();
        quantityBadge.SetActive(amount > 1);
    }
}