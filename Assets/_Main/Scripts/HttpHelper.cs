using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;

public static class HttpHelper
{
    public static async void PostNoBodyAsync(string url, string token)
    {
        Debug.Log("Posting to: " + url + "  ____  " + token);
        using (var req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(new byte[0]);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(token))
                req.SetRequestHeader("Authorization", "Bearer " + token);

            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();


            if (req.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("OnDestroy API Success " + url);
                Debug.Log("OnDestroy API hit: " + req.downloadHandler.text);
            }
            else
            {
                Debug.LogError("OnDestroy API failed: " + req.error + "  " + url);
                Debug.LogError("Message : " + req.downloadHandler.text);
            }

        }
    }
}
