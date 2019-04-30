using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace ZTools.FileIO
{
    ///Bundle文件加载保存监视器
    public class FileIOBundleChecker : MonoBehaviour
    {
        public string path;
        public string reader;
        public string writer;
        public AssetBundle assetBundle;
        public Object[] objects;

        public void Save()
        {

        }
        #region 不同的Save方式
        private void SaveByBundle(AssetBundle assetBundle)
        {

        }
        private void SaveByBytes(byte[] bytes)
        {

        }
        private void SaveByPath(string savePath)
        {

        }
        #endregion

        public void Load()
        {
            StartCoroutine(LoadByWWW());
            StartCoroutine(LoadByWebrequst());
            LoadByFile();
            byte[] bytes = new byte[2];
            LoadByMemory(bytes);
        }

        #region 不同的Load方式
        private IEnumerator LoadByWWW()
        {
            while (!Caching.ready)
            {
                yield return null;
            }
            WWW www = WWW.LoadFromCacheOrDownload(path, 1);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log(www.error);
                yield return null;
            }
            assetBundle = www.assetBundle;
            objects = assetBundle.LoadAllAssets();
        }
        private IEnumerator LoadByWebrequst()
        {
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(path, 0);
            yield return request.SendWebRequest();
            assetBundle = DownloadHandlerAssetBundle.GetContent(request);
            objects = assetBundle.LoadAllAssets();
        }
        private void LoadByFile()
        {
            assetBundle = AssetBundle.LoadFromFile(path);
            if(assetBundle == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }
            objects = assetBundle.LoadAllAssets();
        }
        private IEnumerator LoadByMemory(byte[] bytes)
        {
            AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(bytes);
            yield return createRequest;
            assetBundle = createRequest.assetBundle;
            objects = assetBundle.LoadAllAssets();
        }
        #endregion

        public void Claer()
        {
            assetBundle = null;
            reader = null;
            writer = null;
        }
    }

    [CustomEditor(typeof(FileIOBundleChecker))]
    public class FileIOBundleInspector : Editor
    {
        private FileIOBundleChecker obj;
        private void OnEnable()
        {
            obj = (FileIOBundleChecker)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("快速的Bundle文件读取查看及修改");
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("加载Bundle数据"))
            {
                obj.Load();
            }
            if (GUILayout.Button("保存Bundle数据"))
            {
                obj.Save();
            }
            if (GUILayout.Button("清空加载的Bundle数据"))
            {
                obj.Claer();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }
    }
}