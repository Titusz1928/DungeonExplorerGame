using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameBoot : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject terrainGeneratorPrefab;
    public GameObject playerPrefab;
    public GameObject worldSaveData;

    public static GameObject PersistentWorld;
    public static GameObject PersistentWSD;
    public static GameObject PersistentPlayer;

    [Header("UI References")]
    public CanvasGroup gameBootCanvas;
    public float fadeDuration = 1f;

    private IEnumerator Start()
    {
        // 1. Initial UI Setup
        gameBootCanvas.alpha = 1f;
        gameBootCanvas.gameObject.SetActive(true);

        // 2. The Decision Logic (Bootstrapping)
        if (GameSettings.Instance == null)
        {
            Debug.LogError("GameSettings missing! Defaulting to New Game.");
            yield return StartCoroutine(HandleNewGame());
        }
        else if (GameSettings.Instance.loadFromSave)
        {
            yield return StartCoroutine(HandleLoadGame());
        }
        else
        {
            yield return StartCoroutine(HandleNewGame());
        }

        // 4. Scene Transition
        AsyncOperation loadScene = SceneManager.LoadSceneAsync("GameProcedural");
        while (!loadScene.isDone) yield return null;

        // 5. Cleanup and Show Game
        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));
        gameBootCanvas.gameObject.SetActive(false);
    }

    private IEnumerator HandleNewGame()
    {
        Debug.Log("Booting: NEW GAME");

        CreatePersistentObjects(Vector2.zero); // Temporary (0,0) position

        // Calculate actual coastline spawn
        TileTerrainGenerator gen = PersistentWorld.GetComponent<TileTerrainGenerator>();
        int seed = GameSettings.Instance.seed;
        Vector2 spawnPos = gen.FindRandomCoastlineSpawn(1280, seed);

        PersistentPlayer.transform.position = spawnPos;

        if (WorldSaveData.Instance == null)
            WorldSaveData.Instance = PersistentWorld.GetComponent<WorldSaveData>();

        // Open the gate for a fresh game
        WorldSaveData.Instance.InitializeNewGame();

        yield return null;
    }

    private IEnumerator HandleLoadGame()
    {
        Debug.Log("<color=cyan><b>[LOAD]</b></color> Load process started...");

        // 1. Load data from disk FIRST (before spawning anything)
        SaveGame data = SaveSystem.LoadGame();
        if (data == null)
        {
            Debug.LogError("<color=red><b>[LOAD ERROR]</b></color> Save file could not be read.");
            yield return StartCoroutine(HandleNewGame());
            yield break;
        }

        // 2. Restore Global Settings
        GameSettings.Instance.seed = data.gameSettings.seed;
        GameSettings.Instance.difficulty = data.gameSettings.difficulty;

        // 3. Create Physical Objects (This spawns WorldSaveData and ChunkManager)
        CreatePersistentObjects(Vector2.zero);

        // 4. Ensure Instance is linked immediately
        if (WorldSaveData.Instance == null)
        {
            WorldSaveData.Instance = PersistentWorld.GetComponent<WorldSaveData>();
        }

        // 5. INJECT DATA BEFORE RELEASING CHUNK MANAGER
        // Calling LoadFromWorldSave sets IsLoaded = true internally.
        if (WorldSaveData.Instance != null)
        {
            WorldSaveData.Instance.LoadFromWorldSave(data.world);
            Debug.Log($"<color=cyan><b>[LOAD]</b></color> World Data injected. Gate is now OPEN.");
        }

        // 6. Restore Player Position and State
        Rigidbody2D rb = PersistentPlayer.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Use rb.position for physics-objects to avoid "teleport jitters"
            rb.position = data.player.position;
            rb.linearVelocity = Vector2.zero;
        }
        PersistentPlayer.transform.position = data.player.position;

        // 7. Apply Inventory/Stats
        PlayerSaveBuilder.Apply(PersistentPlayer, data.player);

        // 8. Wait for one frame to allow ChunkManager's Start() to see IsLoaded = true
        yield return new WaitForEndOfFrame();

        Debug.Log($"<color=green><b>[LOAD SUCCESS]</b></color> Game fully restored.");
    }

    private void CreatePersistentObjects(Vector2 startPos)
    {
        if (PersistentWorld == null)
        {
            PersistentWorld = Instantiate(terrainGeneratorPrefab);
            PersistentWorld.name = "WORLD_SYSTEM";
            DontDestroyOnLoad(PersistentWorld);
        }

        if (PersistentPlayer == null)
        {
            PersistentPlayer = Instantiate(playerPrefab, startPos, Quaternion.identity);
            PersistentPlayer.name = "PLAYER";
            DontDestroyOnLoad(PersistentPlayer);
        }

        if (PersistentWSD == null)
        {
            PersistentWSD = Instantiate(worldSaveData);
            PersistentWSD.name = "WORLD_SAVE_DATA";
            DontDestroyOnLoad(PersistentWSD);
        }

    }


    private IEnumerator FadeCanvas(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            gameBootCanvas.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        gameBootCanvas.alpha = end;
    }
}