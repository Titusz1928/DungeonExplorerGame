using UnityEngine;

public class UI_OpenWindow : MonoBehaviour
{
    public GameObject windowPrefab;

    public void Open()
    {
        WindowManager.Instance.OpenWindow(windowPrefab);
    }
}
