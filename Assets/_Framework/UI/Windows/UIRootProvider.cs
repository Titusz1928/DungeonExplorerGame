using UnityEngine;

public class UIRootProvider : MonoBehaviour
{
    public Transform windowRoot;

    private void Start()
    {
        WindowManager.Instance.RegisterUIRoot(windowRoot);
    }
}
