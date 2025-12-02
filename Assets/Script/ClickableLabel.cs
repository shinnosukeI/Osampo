using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickableLabel : MonoBehaviour, IPointerClickHandler
{
    private Toggle targetToggle;

    public void Setup(Toggle toggle)
    {
        targetToggle = toggle;

        // Raycast Targetが見つからない・設定されていない場合のために強制的にONにする
        Graphic graphic = GetComponent<Graphic>();
        if (graphic != null)
        {
            graphic.raycastTarget = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (targetToggle != null && targetToggle.interactable)
        {
            targetToggle.isOn = true;
            // ToggleGroup will handle deselecting others
        }
    }
}
