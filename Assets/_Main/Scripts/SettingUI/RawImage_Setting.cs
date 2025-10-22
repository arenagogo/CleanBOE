using UnityEngine;

public class RawImage_Setting : MonoBehaviour
{
    public Transform transformRawImage;
    //  public RenderTexture renderTexture;
    // public RectTransform rawImagePreview;
    void Start()
    {
#if UNITY_ANDROID
        transformRawImage.localRotation = Quaternion.Euler(0, 0, -90);
        transformRawImage.localScale = new Vector3(-640, 480, 1); // Flip horizontal
                                                                  //  renderTexture.width = 480;
                                                                  //  renderTexture.height = 640;
#elif UNITY_IOS
        transformRawImage.localRotation = Quaternion.Euler(0, 0, 90);
        transformRawImage.localScale = new Vector3(-640, -480, 1); // Flip horizontal
#else
        Debug.Log("Windows");
#endif
    }

}
