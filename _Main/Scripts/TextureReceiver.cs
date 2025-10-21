using UnityEngine;
using UnityEngine.UI;

namespace MoodMe
{
    public class TextureReceiver : MonoBehaviour
    {
        public RawImage textureSource;
        // Public static texture to be accessed globally
        public static Texture2D ExportWebcamTexture { get; private set; }

        // RGBA pixel buffer (optional)
        public static Color32[] GetPixels { get; private set; }

        // Flag to know if texture is ready
        public static bool TextureReady => ExportWebcamTexture != null;

        /// <summary>
        /// Receives a Texture2D from an external source and stores it for global access.
        /// </summary>
        /// <param name="externalTexture">The input texture to store.</param>
        public static void ReceiveTexture(Texture2D externalTexture)
        {
            if (externalTexture == null)
            {
                Debug.LogWarning("Received null texture. Ignored.");
                return;
            }

            // Clone the texture so it's independent of the original source
            ExportWebcamTexture = new Texture2D(externalTexture.width, externalTexture.height, TextureFormat.RGBA32, false);
            Graphics.CopyTexture(externalTexture, ExportWebcamTexture);

            // Optionally store pixel buffer
            GetPixels = ExportWebcamTexture.GetPixels32();

            Debug.Log($"[TextureReceiver] Received external texture {externalTexture.width}x{externalTexture.height}");
        }

        /// <summary>
        /// Clears the currently stored texture.
        /// </summary>
        public static void ClearTexture()
        {
            if (ExportWebcamTexture != null)
            {
                Object.Destroy(ExportWebcamTexture);
                ExportWebcamTexture = null;
                GetPixels = null;
                Debug.Log("[TextureReceiver] Texture cleared.");
            }
        }
    }
}
