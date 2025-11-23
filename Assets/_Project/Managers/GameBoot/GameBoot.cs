using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameBoot : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup gameBootCanvas;

    [Header("Fade Settings")]
    public float fadeDuration = 1f;


    private IEnumerator Start()
    {
        // Make sure canvas is visible at start
        gameBootCanvas.alpha = 0f;
        gameBootCanvas.gameObject.SetActive(true);

        // Fade in
        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));

        // Optional small delay while visible
        yield return new WaitForSeconds(0.5f);

        // Fade out
        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));

        // Load Game scene
        SceneManagerEX.Instance.LoadScene("Game");
    }



    private IEnumerator FadeCanvas(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            gameBootCanvas.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }
        gameBootCanvas.alpha = end;
    }
}
