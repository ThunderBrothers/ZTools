using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace ZTools.FileIO
{
    /// <summary>
    /// Bundle文件加载保存监视器
    /// 1 提供不同的Load方式：
    ///     LoadByWWW(string path);
    ///     LoadByWebrequst(string path);
    ///     LoadByFile(string path);
    ///     LoadByMemory(byte[] bytes);
    /// 2 提供不同Save方法：
    /// 
    /// </summary>
    public class FileIOBundleChecker : MonoBehaviour
    {
        /// <summary>
        /// 加载或者服务器url地址
        /// </summary>
        public string loadPath;
        /// <summary>
        /// Bundle的全名
        /// 如：Car.assetbundle,Car.Unity等等
        /// </summary>
        public string bundleName;
        /// <summary>
        /// url + fileFullName
        /// 如：C:\Users\Administrator\Desktop\Bundle工具演示\Car.assetbundle
        /// </summary>
        private string fileFullName;
        /// <summary>
        /// 保存地址
        /// 如：C:\Users\Administrator\Desktop\Bundle工具演示\
        /// </summary>
        public string savePath;
        public AssetBundle assetBundle;
        public Object[] objects;
        public GameObject obj;

        public void InstantiateObj()
        {
            Object temp = assetBundle.LoadAsset(assetBundle.mainAsset.name);
            obj = Instantiate(temp,null) as GameObject;
        }

        public void Save()
        {
            string sPath = Path.Combine(savePath, bundleName);
            string dataPath = Path.Combine(loadPath, bundleName);
            SaveByPath(dataPath, sPath);
        }
        #region 不同的Save方式
        private void SaveByBundle(string path, AssetBundle assetBundle)
        {

        }
        private void SaveByBytes(string path, byte[] bytes)
        {

        }
        private void SaveByPath(string path, string savePath)
        {
            StartCoroutine(DownloadAssetBundleAndSave(path, savePath));
        }
        #endregion

        public void Load()
        {
            fileFullName = Path.Combine(loadPath, bundleName);
            //StartCoroutine(LoadByWWW(fileFullName));
            StartCoroutine(LoadByWebrequst(fileFullName));
            //LoadByFile(loadPath);
            //byte[] bytes = new byte[2];
            //LoadByMemory(bytes);
        }
        IEnumerator DownloadAssetBundleAndSave(string dataPath,string savePath)
        {
            WWW www = new WWW(dataPath);
            yield return www;
            if (www.isDone)
            {
                SaveAssetBundle(savePath, www.bytes, www.bytes.Length);
            }
        }
        private void SaveAssetBundle(string fileName, byte[] bytes, int count)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            FileStream fs = fileInfo.Create();
            fs.Write(bytes, 0, count);
            fs.Flush();
            fs.Close();
            fs.Dispose();
        }
        #region 不同的Load方式
        private IEnumerator LoadByWWW(string path)
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
            if (www.isDone)
            {
                assetBundle = www.assetBundle;
                objects = assetBundle.LoadAllAssets();
            }

        }
        private IEnumerator LoadByWebrequst(string path)
        {
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(path, 0);
            yield return request.SendWebRequest();
            assetBundle = DownloadHandlerAssetBundle.GetContent(request);
            objects = assetBundle.LoadAllAssets();
        }
        private void LoadByFile(string path)
        {
            assetBundle = AssetBundle.LoadFromFile(path);
            if (assetBundle == null)
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
            objects = null;
            obj = null;
            AssetBundle.UnloadAllAssetBundles(false);
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
            if (GUILayout.Button("实例化Bundle的主物体"))
            {
                obj.InstantiateObj();
            }
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }
    }
}