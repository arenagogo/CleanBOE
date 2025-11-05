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

  [TextArea(3, 10)]
  public string textToShare;

  [Obsolete]
  private void Awake()
  {
    _share.onClick.AddListener(ShareClicked);
    //  _logView.LogMessage($"{_title} started.");
  }

  private void OnDestroy() => _share.onClick.RemoveAllListeners();

  [Obsolete]
  public void ShareDiKlik()
  {
    ShareClicked();
  }

  [Obsolete]
  private void ShareClicked()
  {
    if (!Share.IsPlatformSupported)
    {
      // _logView.LogError("Share: platform not supported");
      return;
    }

    var items = new List<string>();

    // Adjust link by device
    string linkToShare = textToShare;
#if UNITY_ANDROID
    linkToShare = "https://play.google.com/store/apps?id=com.EmotionDuel.ArenaGo";
#elif UNITY_IOS
    linkToShare = "https://apps.apple.com/app/id6753614362";
#endif

    items.Add(linkToShare);

    //  _logView.LogMessage("Share: requested");
    Share.Items(items, success =>
    {
      Debug.Log($"Share: {(success ? "success" : "failed")}");
    });
  }
}
