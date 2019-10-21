using UnityEngine;

namespace Eflatun.MobileWebView
{
    public class WebViewTestLauncher : MonoBehaviour
    {
        [SerializeField] private GameObject _webViewModal;
        [SerializeField] private string _url;

        private void Start()
        {
            Instantiate(_webViewModal).GetComponent<WebViewController>().Init(_url);
        }
    }
}
