using System.Collections;
using UnityEngine;

namespace Eflatun.WebView
{
    public class WebViewTestLauncher : MonoBehaviour
    {
        [SerializeField] private GameObject _webViewModal;
        [SerializeField] private string _url;

        private IEnumerator Start()
        {
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;

            Instantiate(_webViewModal).GetComponent<WebViewController>().Init(_url);
        }
    }
}
