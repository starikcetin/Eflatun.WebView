/*
 * Copyright (C) 2012 GREE, Inc.
 *
 * This software is provided 'as-is', without any express or implied
 * warranty.  In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

/*
 * 2019 - S. Tarık Çetin - cetinsamedtarik[at]gmail.com
 * The file is altered to fit the purposes of the project.
 */

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Eflatun.WebView
{
    public class WebViewController : MonoBehaviour
    {
        private string _url;
        [SerializeField] private WebViewObject _webViewObject;

        // This is used for destruction on close command
        [SerializeField] private GameObject _root;

        [SerializeField] private RectTransform _placeholderRect;
        [SerializeField] private Button _forwardButton;
        [SerializeField] private Button _backButton;

        [FormerlySerializedAs("_progressText")] [FormerlySerializedAs("_status")] [SerializeField]
        private TMP_Text _statusText;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Init(string url)
        {
            _url = url;
            StartCoroutine(Initializer());
        }

        private IEnumerator Initializer()
        {
            _webViewObject.Init(
                cb: (msg) =>
                {
                    Debug.Log($"CallFromJS[{msg}]");
                    if (_statusText != null)
                    {
                        _statusText.text = msg;
                        _statusText.GetComponent<Animation>()?.Play();
                    }
                },
                err: (msg) =>
                {
                    Debug.Log($"CallOnError[{msg}]");
                    if (_statusText != null)
                    {
                        _statusText.text = msg;
                        _statusText.GetComponent<Animation>()?.Play();
                    }
                },
                started: (msg) => { Debug.Log($"CallOnStarted[{msg}]"); },
                ld: (msg) =>
                {
                    Debug.Log($"CallOnLoaded[{msg}]");
#if UNITY_EDITOR_OSX || !UNITY_ANDROID
                    _webViewObject.EvaluateJS(@"
                  if (window && window.webkit && window.webkit.messageHandlers && window.webkit.messageHandlers.unityControl) {
                    window.Unity = {
                      call: function(msg) {
                        window.webkit.messageHandlers.unityControl.postMessage(msg);
                      }
                    }
                  } else {
                    window.Unity = {
                      call: function(msg) {
                        var iframe = document.createElement('IFRAME');
                        iframe.setAttribute('src', 'unity:' + msg);
                        document.documentElement.appendChild(iframe);
                        iframe.parentNode.removeChild(iframe);
                        iframe = null;
                      }
                    }
                  }
                ");
#endif
                    _webViewObject.EvaluateJS(@"Unity.call('ua=' + navigator.userAgent)");
                },
                //ua: "custom user agent string",
                enableWKWebView: true);
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        _webViewObject.bitmapRefreshCycle = 1;
#endif

            SyncMarginsWithPlaceholderRect();
            _webViewObject.SetVisibility(true);

#if !UNITY_WEBPLAYER
            if (_url.StartsWith("http"))
            {
                _webViewObject.LoadURL(_url.Replace(" ", "%20"));
            }
            else
            {
                var exts = new string[]
                {
                    ".jpg",
                    ".js",
                    ".html" // should be last
                };
                foreach (var ext in exts)
                {
                    var url = _url.Replace(".html", ext);
                    var src = System.IO.Path.Combine(Application.streamingAssetsPath, url);
                    var dst = System.IO.Path.Combine(Application.persistentDataPath, url);
                    byte[] result = null;
                    if (src.Contains("://"))
                    {
                        // for Android
                        var www = new WWW(src);
                        yield return www;
                        result = www.bytes;
                    }
                    else
                    {
                        result = System.IO.File.ReadAllBytes(src);
                    }

                    System.IO.File.WriteAllBytes(dst, result);
                    if (ext == ".html")
                    {
                        _webViewObject.LoadURL("file://" + dst.Replace(" ", "%20"));
                        break;
                    }
                }
            }
#else
        if (Url.StartsWith("http")) {
            _webViewObject.LoadURL(Url.Replace(" ", "%20"));
        } else {
            _webViewObject.LoadURL("StreamingAssets/" + Url.Replace(" ", "%20"));
        }
        _webViewObject.EvaluateJS(
            "parent.$(function() {" +
            "   window.Unity = {" +
            "       call:function(msg) {" +
            "           parent.unityWebView.sendMessage('WebViewObject', msg)" +
            "       }" +
            "   };" +
            "});");
#endif
            yield break;
        }

        private void SyncMarginsWithPlaceholderRect()
        {
            (int topMargin, int bottomMargin, int leftMargin, int rightMargin) =
                ((int, int, int, int)) _placeholderRect.CalculateMarginsFromScreen();

            Debug.Log($"[{nameof(WebViewController)}] Syncing margins: " +
                $"{nameof(topMargin)}={topMargin} " +
                $"{nameof(bottomMargin)}={bottomMargin} " +
                $"{nameof(leftMargin)}={leftMargin} " +
                $"{nameof(rightMargin)}={rightMargin}");

            _webViewObject.SetMargins(leftMargin, topMargin, rightMargin, bottomMargin);
        }

        private Rect _prevRect;
        private Rect _prevScreenRect;
        private RectTransform _rectTransform;

        private void Update()
        {
            if (_prevRect != _rectTransform.rect || _prevScreenRect != Drawing.ScreenRect)
            {
                SyncMarginsWithPlaceholderRect();
            }

            _prevRect = _rectTransform.rect;
            _prevScreenRect = Drawing.ScreenRect;

            _forwardButton.interactable = _webViewObject.CanGoForward();
            _backButton.interactable = _webViewObject.CanGoBack();

            var progress = _webViewObject.Progress();

            if (progress == 100)
            {
                _statusText.text = "Loaded.";
            }
            else if (progress == 0)
            {
                _statusText.text = $"Ready.";
            }
            else
            {
                _statusText.text = $"Loading... {progress} %";
            }
        }

        public void Forward()
        {
            _webViewObject.GoForward();
        }

        public void Back()
        {
            _webViewObject.GoBack();
        }

        public void Reset()
        {
            if (_webViewObject != null)
            {
                var oldWvo = _webViewObject;
                _webViewObject = _webViewObject.gameObject.AddComponent<WebViewObject>();
                Destroy(oldWvo);
                StartCoroutine(Initializer());
            }
        }

        public void Close()
        {
            Destroy(_root);
        }
    }
}
