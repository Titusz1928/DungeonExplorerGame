using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameBoot : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject terrainGeneratorPrefab;
    public GameObject playerPrefab;

    public static GameObject PersistentWorld;
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
        yield return null;
    }

    private IEnumerator HandleLoadGame()
    {
        Debug.Log("<color=cyan><b>[LOAD]</b></color> Load process started...");

        SaveGame data = SaveSystem.LoadGame();
        if (data == null)
        {
            Debug.LogError("<color=red><b>[LOAD ERROR]</b></color> Save file could not be read. Check persistentDataPath.");
            yield return StartCoroutine(HandleNewGame());
            yield break;
        }

        // 1. Restore Global Settings
        GameSettings.Instance.seed = data.gameSettings.seed;
        GameSettings.Instance.difficulty = data.gameSettings.difficulty;
        Debug.Log($"<color=cyan><b>[LOAD]</b></color> Settings Restored. Seed: {data.gameSettings.seed}");

        // 2. Create the Physical Objects
        CreatePersistentObjects(Vector2.zero);

        // FORCE the Instance assignment if Awake hasn't run yet
        if (WorldSaveData.Instance == null)
        {
            WorldSaveData.Instance = PersistentWorld.GetComponent<WorldSaveData>();
        }

        // 3. Restore World Save Data
        if (WorldSaveData.Instance != null)
        {
            // data.world is the 'WorldSave' object from your JSON
            WorldSaveData.Instance.LoadFromWorldSave(data.world);
            Debug.Log($"[LOAD] World Data injected into WorldSaveData manager.");
        }

        // 4. Restore Player State
        // Note: Since data.player.position is a Vector2, we can assign it directly
        // Inside HandleLoadGame, change step 4 to this:
        Rigidbody2D rb = PersistentPlayer.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.position = data.player.position; // Physics-safe position set
            rb.linearVelocity = Vector2.zero;   // Stop any carry-over movement
        }
        PersistentPlayer.transform.position = data.player.position;

        // 5. Apply Stats, Inventory, Skills, Equipment
        Debug.Log("<color=cyan><b>[LOAD]</b></color> Applying Player Statistics...");
        PlayerSaveBuilder.Apply(PersistentPlayer, data.player);

        // 6. Final Verification
        yield return new WaitForEndOfFrame(); // Wait for one frame to let physics/UI update
        Debug.Log($"<color=green><b>[LOAD SUCCESS]</b></color> Player HP now: {PersistentPlayer.GetComponent<PlayerStateManager>().health}");

        yield return null;
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