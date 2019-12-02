using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

namespace ZTools.EditorExample
{
    /// <summary>
    /// C Custom自定义 A Assets资源
    /// 自定义配置资源 类似Dotween的Setting资源
    /// </summary>
    [CustomEditor(typeof(CAConfig))]
    public class CAConfigEditor : Editor
    {
        [MenuItem("ZTools/CreateConfigAsset")]
        static void CreateConfigAsset()
        {
            ScriptableObject bullet = ScriptableObject.CreateInstance<CABullet>();
            if (bullet == null)
            {
                Debug.LogError("Create Fail");
                return;
            }
            string path = Application.dataPath + "/ZTools/EditorExample/Recoures";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = "Assets/ZTools/EditorExample/Recoures/BulletAsset.asset";
            AssetDatabase.CreateAsset(bullet, path);
        }

        CAConfig _target;
        private void OnEnable()
        {
            _target = (CAConfig)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("加载Asset资源"))
            {
                _target.LoadAsset();
            }
            if (GUILayout.Button("显示资源中的数据"))
            {
                _target.ShowAssetData();
            }
            EditorGUILayout.EndVertical();
        }
    }
}

