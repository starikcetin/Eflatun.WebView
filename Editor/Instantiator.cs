using UnityEditor;
using UnityEngine;

namespace Eflatun.WebView.Editor
{
    internal class Instantiator : ScriptableObject
    {
        private static Instantiator _instance;
        private static Instantiator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CreateInstance<Instantiator>();
                }

                return _instance;
            }
        }

        [SerializeField] private GameObject _webView;
        [SerializeField] private GameObject _webViewTestLauncher;

        [MenuItem("GameObject/Eflatun/WebView", false, 12)]
        private static void InstantiateWebView()
        {
            Selection.activeObject = PrefabUtility.InstantiatePrefab(Instance._webView);
        }

        [MenuItem("GameObject/Eflatun/WebView", true, 12)]
        private static bool ValidateInstantiateWebView()
        {
            var go = Instance._webView;
            return go != null && PrefabUtility.IsPartOfPrefabAsset(go);
        }

        [MenuItem("GameObject/Eflatun/Test/WebView Test Launcher", false, 12)]
        private static void InstantiateWebViewTestLauncher()
        {
            Selection.activeObject = PrefabUtility.InstantiatePrefab(Instance._webViewTestLauncher);
        }

        [MenuItem("GameObject/Eflatun/Test/WebView Test Launcher", true, 12)]
        private static bool ValidateInstantiateWebViewTestLauncher()
        {
            var go = Instance._webViewTestLauncher;
            return go != null && PrefabUtility.IsPartOfPrefabAsset(go);
        }
    }
}
