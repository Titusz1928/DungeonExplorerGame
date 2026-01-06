using System.Collections.Generic;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    public static WindowManager Instance;

    [Header("Window Parent")]
    public Transform uiRoot;  // Canvas content target

    private Stack<GameObject> windowStack = new Stack<GameObject>();

    public List<string> debugHistory = new List<string>();

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
        if (uiRoot == null)
        {
            Debug.LogError("WindowManager: No uiRoot registered! Cannot spawn window.");
            return null;
        }

        GameObject window = Instantiate(windowPrefab, uiRoot);
        windowStack.Push(window);

        if (UIManager.Instance != null)
            UIManager.Instance.SetWindowState(true);


        return window;
    }

    public void CloseTopWindow()
    {
        if (windowStack.Count == 0) return;

        GameObject top = windowStack.Pop();
        Destroy(top);

        if (UIManager.Instance != null)
            UIManager.Instance.SetWindowState(false);

    }

    public void CloseAllWindows()
    {
        while (windowStack.Count > 0)
            Destroy(windowStack.Pop());

        if (UIManager.Instance != null)
            UIManager.Instance.SetWindowState(false);
    }

    public void RegisterUIRoot(Transform root)
    {
        uiRoot = root;
    }
}
