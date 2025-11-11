using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Boot : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(LoadMainMenuWithDelay());
    }

    private IEnumerator LoadMainMenuWithDelay()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("MainMenu");
    }
}
