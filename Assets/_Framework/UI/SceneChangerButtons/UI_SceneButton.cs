using UnityEngine;

public class UI_SceneButton : MonoBehaviour
{
    public string sceneName;

    public void LoadScene()
    {
        SceneManagerEX.Instance.LoadScene(sceneName);
    }
}
