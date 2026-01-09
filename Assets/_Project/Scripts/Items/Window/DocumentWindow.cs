using UnityEngine;
using UnityEngine.UI;

public class DocumentWindow : MonoBehaviour
{
    [Header("UI Containers")]
    public Transform contentParent;     // Under the BackgroundImage
    public Transform controlsParent;    // Under the BottomSettings

    [Header("Book Prefabs")]
    public GameObject bookPagesPrefab;
    public GameObject bookControlsPrefab;

    [Header("Generic Document Prefabs")]
    public GameObject singlePageImagePrefab;
    public GameObject paintingPrefab;
    public GameObject singlePageControlsPrefab;
    public GameObject textDocumentPrefab;

    [Header("Background Art")]
    public Image backgroundImage;
    public Sprite bookBackgroundSprite;
    public Sprite defaultFirstPage;
    // Add more sprites here as you get the art (scrollBackground, etc.)

    private ItemInstance currentItem;
    private DocumentSO docData;

    // Controllers
    private SinglePageController activeSingleController;
    private BookController activeBookController;

    // State
    private bool showingFront = true;
    private int currentPaperIndex = 0; // For books
    private GameObject lastSpawnedPrefab;

    public void OpenDocument(ItemInstance item)
    {
        currentItem = item;
        docData = item.itemSO as DocumentSO;

        if (docData == null)
        {
            Debug.LogError("Item passed to DocumentWindow is not a DocumentSO!");
            return;
        }

        // Reset states
        showingFront = true;
        currentPaperIndex = 0;
        lastSpawnedPrefab = null;


        ClearExistingUI();
        SetupLayout();
    }


    private void SetupLayout()
    {
        GameObject contentGo = null;
        GameObject controlsGo = null;

        if (docData.docType == GDocumentType.Book)
        {
            if (backgroundImage != null && bookBackgroundSprite != null)
            {
                backgroundImage.sprite = bookBackgroundSprite;
                backgroundImage.enabled = true;
            }

            contentGo = Instantiate(bookPagesPrefab, contentParent);
            controlsGo = Instantiate(bookControlsPrefab, controlsParent);

            activeBookController = contentGo.GetComponent<BookController>();

            // Link Book Buttons (Assuming buttons are named LeftButton/RightButton or similar)
            Button[] buttons = controlsGo.GetComponentsInChildren<Button>();
            foreach (var btn in buttons)
            {
                if (btn.name.Contains("previousPageButton")) btn.onClick.AddListener(() => ChangeBookPage(-1));
                if (btn.name.Contains("nextPageButton")) btn.onClick.AddListener(() => ChangeBookPage(1));
            }

            UpdateBookDisplay();
        }
        else
        {
            // Single View Logic (Page, Map, Painting)
            controlsGo = Instantiate(singlePageControlsPrefab, controlsParent);
            Button flipBtn = controlsGo.GetComponentInChildren<Button>();
            if (flipBtn != null) flipBtn.onClick.AddListener(ToggleFlip);

            UpdateSingleDisplay();
        }
    }

    #region Book Logic
    public void ChangeBookPage(int direction)
    {
        int nextIndex = currentPaperIndex + direction;

        // Bounds check
        if (nextIndex < 0 || nextIndex > docData.papers.Count) return;

        currentPaperIndex = nextIndex;
        UpdateBookDisplay();
    }

    private void UpdateBookDisplay()
    {
        if (activeBookController == null) return;

        // --- RIGHT SIDE LOGIC ---
        PaperSO rightPaperData = null;
        PaperInstance rightPaperInst = null;
        string rightLabel = "";

        // Only assign right side if we haven't turned past the last piece of paper
        if (currentPaperIndex < docData.papers.Count)
        {
            rightPaperData = docData.papers[currentPaperIndex];
            rightPaperInst = currentItem.paperInstances[currentPaperIndex];
            rightLabel = "Frontside";
        }

        // --- LEFT SIDE LOGIC ---
        PaperSO leftPaperData = null;
        PaperInstance leftPaperInst = null;
        string leftLabel = "";

        if (currentPaperIndex == 0)
        {
            // First spread: Left is empty
            leftPaperData = null;
        }
        else
        {
            // Left is ALWAYS the back of the paper we just turned (index - 1)
            leftPaperData = docData.papers[currentPaperIndex - 1];
            leftPaperInst = currentItem.paperInstances[currentPaperIndex - 1];
            leftLabel = "Backside";
        }

        // --- REFRESH ---
        activeBookController.Refresh(
            leftPaperData?.backSide, leftPaperInst?.backSide, leftLabel,
            rightPaperData?.frontSide, rightPaperInst?.frontSide, rightLabel,
            defaultFirstPage
        );

        // ADD THIS:
        // We check both pages currently visible on the screen!
        if (leftPaperData != null) KnowledgeManager.Instance.TryReadPage(leftPaperData.backSide);
        if (rightPaperData != null) KnowledgeManager.Instance.TryReadPage(rightPaperData.frontSide);
    }
    #endregion


    #region Single Page Logic
    public void ToggleFlip()
    {
        showingFront = !showingFront;
        UpdateSingleDisplay();
    }

    private void UpdateSingleDisplay()
    {
        if (docData.papers.Count == 0) return;

        PageSideSO sideData = showingFront ? docData.papers[0].frontSide : docData.papers[0].backSide;
        PageSideInstance sideInst = showingFront ? currentItem.paperInstances[0].frontSide : currentItem.paperInstances[0].backSide;

        GameObject neededPrefab = GetPrefabForSide(sideData);

        if (neededPrefab != lastSpawnedPrefab)
        {
            foreach (Transform child in contentParent) Destroy(child.gameObject);
            GameObject newContent = Instantiate(neededPrefab, contentParent);
            activeSingleController = newContent.GetComponent<SinglePageController>();
            lastSpawnedPrefab = neededPrefab;
        }

        if (activeSingleController != null)
        {
            string sideLabelText = showingFront ? "Frontside" : "Backside";
            activeSingleController.Refresh(sideData, sideInst, sideData.background != null, sideLabelText);

            KnowledgeManager.Instance.TryReadPage(sideData);
        }
    }

    private GameObject GetPrefabForSide(PageSideSO side)
    {
        if (side.background == null) return textDocumentPrefab;
        return (docData.docType == GDocumentType.Page) ? singlePageImagePrefab : paintingPrefab;
    }
    #endregion

    private void ClearExistingUI()
    {
        foreach (Transform child in contentParent) Destroy(child.gameObject);
        foreach (Transform child in controlsParent) Destroy(child.gameObject);
        activeSingleController = null;
        activeBookController = null;
    }

    public void CloseWindow()
    {
        WindowManager.Instance.CloseTopWindow();
    }
}