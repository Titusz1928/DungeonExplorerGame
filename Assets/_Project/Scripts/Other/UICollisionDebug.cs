using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UICollisionDebug : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"[UI DEBUG] You clicked: {eventData.pointerCurrentRaycast.gameObject.name}");

        // This lists EVERY object the mouse is currently over
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        Debug.Log("[UI DEBUG] Objects under mouse:");
        foreach (var result in results)
        {
            Debug.Log($" - {result.gameObject.name} (Layer: {result.gameObject.layer})");
        }
    }
}