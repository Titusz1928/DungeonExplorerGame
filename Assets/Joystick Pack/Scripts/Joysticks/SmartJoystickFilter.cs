using UnityEngine;
using UnityEngine.UI;

public class SmartJoystickFilter : MonoBehaviour, ICanvasRaycastFilter
{
    // The layer your World Space Buttons are on
    public LayerMask interactableLayer;
    public Camera mainCamera;

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        // 1. Cast a ray from the screen point into the world
        Ray ray = mainCamera.ScreenPointToRay(sp);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, interactableLayer);

        // 2. If we hit an Interactable (Bush Button), return FALSE
        // This tells the Joystick: "Pretend you didn't see this click"
        if (hit.collider != null)
        {
            return false;
        }

        // 3. Otherwise, return TRUE (Joystick handles the click)
        return true;
    }
}