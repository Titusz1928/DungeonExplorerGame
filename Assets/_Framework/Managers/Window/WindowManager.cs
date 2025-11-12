using System.Collections.Generic;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    public static WindowManager Instance;

    [Header("Window Parent")]
    public Transform uiRoot;  // Canvas content target

    private Stack<GameObject> windowStack = new Stack<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public GameObject OpenWindow(GameObject windowPrefab)
    {
        GameObject window = Instantiate(windowPrefab, uiRoot);
        windowStack.Push(window);
        return window;
    }

    public void CloseTopWindow()
    {
        if (windowStack.Count == 0) return;

        GameObject top = windowStack.Pop();
        Destroy(top);
    }

    public void CloseAllWindows()
    {
        while (windowStack.Count > 0)
            Destroy(windowStack.Pop());
    }

    public void RegisterUIRoot(Transform root)
    {
        uiRoot = root;
    }
}
