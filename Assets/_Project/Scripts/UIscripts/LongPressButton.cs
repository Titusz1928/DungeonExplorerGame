using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private float holdThreshold = 0.5f; // How long to hold for a long press

    public UnityEvent onShortClick;
    public UnityEvent onLongPress;

    private bool isPointerDown;
    private bool longPressTriggered;
    private float pointerDownTimer;

    private void Update()
    {
        if (isPointerDown && !longPressTriggered)
        {
            pointerDownTimer += Time.deltaTime;
            if (pointerDownTimer >= holdThreshold)
            {
                longPressTriggered = true;
                onLongPress.Invoke();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        longPressTriggered = false;
        pointerDownTimer = 0;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // If they release BEFORE the threshold, it's a short click
        if (isPointerDown && !longPressTriggered)
        {
            onShortClick.Invoke();
        }
        Reset();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Reset();
    }

    private void Reset()
    {
        isPointerDown = false;
        longPressTriggered = false;
        pointerDownTimer = 0;
    }
}