using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Android;

public class SimpleCameraTest : MonoBehaviour
{
    public RawImage display;

    private WebCamTexture cam;

    void Start()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            Permission.RequestUserPermission(Permission.Camera);
        }

        cam = new WebCamTexture();
        display.texture = cam;
        cam.Play();
    }
}
