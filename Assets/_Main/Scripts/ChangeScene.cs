using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    string _sceneName;
    public void ChangeToScene(string sceneName)
    {
        _sceneName = sceneName;
        //SceneManager.LoadSceneAsync(sceneName);
        Invoke(nameof(_ChangeToScene), 0.1f);
    }

    public void _ChangeToScene()
    {
        SceneManager.LoadSceneAsync(_sceneName);
    }
}
