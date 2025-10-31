using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public void ChangeToScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}
