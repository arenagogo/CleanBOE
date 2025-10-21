using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class DynamicCanvasScaler : MonoBehaviour
{
    void Start()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();

        // Make sure using Scale With Screen Size mode
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Set the reference resolution to the current screen resolution
        scaler.referenceResolution = new Vector2(Screen.width, Screen.height);

        // Optional: You can adjust this to prefer width or height matching
       // scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
       // scaler.matchWidthOrHeight = 0.5f;
    }

    [ContextMenu("SetResolusi")]
    public void SetResolusion()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();

        // Make sure using Scale With Screen Size mode
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Set the reference resolution to the current screen resolution
        scaler.referenceResolution = new Vector2(Screen.width, Screen.height);

        // Optional: You can adjust this to prefer width or height matching
        // scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        // scaler.matchWidthOrHeight = 0.5f;
    }
}
