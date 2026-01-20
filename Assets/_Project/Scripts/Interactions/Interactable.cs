using UnityEngine;
using UnityEngine.UI;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string interactText = "Interact";
    public float interactionRange = 2f;

    [Header("UI Reference")]
    public GameObject interactionCanvas; // A World-Space Canvas on the prefab
    public Button interactButton;

    protected Transform playerTransform;
    private bool isPlayerInRange = false;

    protected virtual void Start()
    {
        // Use your static reference for safety
        if (GameBoot.PersistentPlayer != null)
        {
            playerTransform = GameBoot.PersistentPlayer.transform;
        }
        else
        {
            // Fallback just in case
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        if (interactButton != null)
            interactButton.onClick.AddListener(OnInteract);

        interactionCanvas.SetActive(false);
    }

    protected virtual void Update()
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= interactionRange && !isPlayerInRange)
        {
            ShowUI();
        }
        else if (distance > interactionRange && isPlayerInRange)
        {
            HideUI();
        }
    }

    private void ShowUI()
    {
        isPlayerInRange = true;
        interactionCanvas.SetActive(true);
    }

    private void HideUI()
    {
        isPlayerInRange = false;
        interactionCanvas.SetActive(false);
    }

    // This is the "Magic" part: Every child class must define what this does
    public abstract void OnInteract();
}
