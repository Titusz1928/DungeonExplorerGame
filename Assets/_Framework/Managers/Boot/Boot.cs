using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Boot : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup bootCanvas; // assign your boot canvas in inspector

    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    private IEnumerator Start()
    {
        // Make sure canvas is visible at start
        bootCanvas.alpha = 0f;
        bootCanvas.gameObject.SetActive(true);

        // Fade in
        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));

        // === Initialize managers ===
        EnsureManager<SceneManagerEX>("SceneManager");
        EnsureManager<WindowManager>("WindowManager");

        if (LocalizationManager.Instance == null)
        {
            var locObj = new GameObject("LocalizationManager");
            var locManager = locObj.AddComponent<LocalizationManager>();
            DontDestroyOnLoad(locObj);
            locManager.Initialize();
        }
        else
        {
            LocalizationManager.Instance.Initialize();
        }

        // Audio Manager
        if (AudioManager.Instance == null)
        {
            var audioObj = new GameObject("AudioManager");
            audioObj.AddComponent<AudioManager>();
            DontDestroyOnLoad(audioObj);
        }

        InitializeMessageManager();

        // Optional small delay while visible
        yield return new WaitForSeconds(0.5f);

        // Fade out
        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));

        // Load Main Menu
        SceneManagerEX.Instance.LoadScene("MainMenu");
    }

    private void InitializeMessageManager()
    {
        if (MessageManager.Instance != null)
            return;

        // Create MessageManager
        var msgObj = new GameObject("MessageManager");
        var mm = msgObj.AddComponent<MessageManager>();
        DontDestroyOnLoad(msgObj);

        // Load the message box prefab (you still need this)
        mm.messagePrefab = Resources.Load<GameObject>("UI/Messaging/MessagePrefab");

        // Find MessageContainer in the current scene
        var container = GameObject.Find("MessageContainer");

        if (container != null)
        {
            mm.messageContainer = container.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("[Boot] No MessageContainer found in scene!");
        }
    }

    private void EnsureManager<T>(string name) where T : MonoBehaviour
    {
        if (FindFirstObjectByType<T>() == null)
        {
            var obj = new GameObject(name);
            obj.AddComponent<T>();
            DontDestroyOnLoad(obj);
        }
    }

    private IEnumerator FadeCanvas(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            bootCanvas.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }
        bootCanvas.alpha = end;
    }
}
