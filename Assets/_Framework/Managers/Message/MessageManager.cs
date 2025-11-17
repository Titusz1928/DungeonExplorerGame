using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance;

    [Header("References")]
    public RectTransform messageContainer;
    public GameObject messagePrefab;

    [Header("Settings")]
    public float messageDuration = 3f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find the container in the new scene
        var containerObj = GameObject.Find("MessageContainer");
        if (containerObj != null)
        {
            messageContainer = containerObj.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogWarning("[MessageManager] No MessageContainer found in scene " + scene.name);
        }
    }

    /// <summary>
    /// Shows a localized message using a key from JSON.
    /// </summary>
    public void ShowMessage(string key, Sprite icon = null)
    {
        if (messagePrefab == null || messageContainer == null)
        {
            Debug.LogError("[MessageManager] Missing UI references!");
            return;
        }

        GameObject msg = Instantiate(messagePrefab, messageContainer);

        var textObj = msg.transform.Find("MessageText");
        var iconObj = msg.transform.Find("MessageIcon");

        if (textObj != null)
        {
            var loc = textObj.GetComponent<LocalizedText>();
            if (loc != null)
                loc.SetKey(key);
        }

        if (iconObj != null)
        {
            var img = iconObj.GetComponent<Image>();
            if (img != null && icon != null)
                img.sprite = icon;
        }

        StartCoroutine(RemoveAfterDelay(msg));
    }

    private IEnumerator RemoveAfterDelay(GameObject msg)
    {
        yield return new WaitForSeconds(messageDuration);

        if (msg == null) yield break; // object already destroyed

        var cg = msg.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                if (cg == null) yield break; // object destroyed while fading
                t += Time.deltaTime;
                cg.alpha = 1f - t;
                yield return null;
            }
        }

        if (msg != null)
            Destroy(msg);
    }


    /// <summary>
    /// Optional: manually register a container (like WindowManager.RegisterUIRoot)
    /// </summary>
    public void RegisterContainer(RectTransform container)
    {
        messageContainer = container;
    }
}
