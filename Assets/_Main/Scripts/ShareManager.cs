using System;
using System.Collections.Generic;

using Sych.ShareAssets.Example.Tools;
using Sych.ShareAssets.Runtime;

using UnityEngine;
using UnityEngine.UI;

public class ShareManager : MonoBehaviour
{
    //[SerializeField] private LogView _logView;
   // [SerializeField] string _title;
    [SerializeField] private Button _share;

    [TextArea]
    public string textToShare;

    private void Awake()
    {
        _share.onClick.AddListener(ShareClicked);
      //  _logView.LogMessage($"{_title} started.");
    }

    private void OnDestroy() => _share.onClick.RemoveAllListeners();

    [Obsolete]
    private void ShareClicked()
    {
        if (!Share.IsPlatformSupported)
        {
           // _logView.LogError("Share: platform not supported");
            return;
        }

        var items = new List<string>();
        items.Add(textToShare);

      //  _logView.LogMessage("Share: requested");
        Share.Items(items, success =>
        {
            Debug.Log($"Share: {(success ? "success" : "failed")}");
        });
    }
}
