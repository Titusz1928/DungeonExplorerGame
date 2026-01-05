using UnityEngine;
using UnityEngine.UI;

public class BattleInjuryCell : MonoBehaviour
{
    public Image typeImage;
    public Image bodyPartImage;

    public void Setup(Injury injury, InjuryDatabase db)
    {
        // Always show full details
        typeImage.gameObject.SetActive(true);
        bodyPartImage.gameObject.SetActive(true);

        // Set Body Part Sprite
        bodyPartImage.sprite = db.GetBodyPartSprite(injury.bodyPart);

        // Set Injury Type Sprite
        typeImage.sprite = injury.type switch
        {
            InjuryType.Cut => db.cutIcon,
            InjuryType.Stab => db.stabIcon,
            InjuryType.Fracture => db.fractureIcon,
            _ => db.unknownIcon
        };
    }
}