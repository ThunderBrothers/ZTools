using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ZTools.FileIO
{
    public class FileTextIOChecker : MonoBehaviour
    {

        public string path;
        public string reader;
        public string writer;

        public void Start()
        {

        }
        public void Save()
        {
            if (writer == null)
            {
                return;
            }
            IOBase.WriteByFileStream(path, writer);
        }
        public void Load()
        {
            reader = IOBase.ReadByFileStream(path);
        }
        public void Claer()
        {
            reader = null;
            writer = null;
        }
    }

    [CustomEditor(typeof(FileTextIOChecker))]
    public class FileIOTextInspector : Editor
    {
        private FileTextIOChecker obj;
        void OnEnable()
        {
            obj = (FileTextIOChecker)target;
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("快速的文本配置文件读取查看及修改");
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("加载文本数据"))
            {
                obj.Load();
            }
            if (GUILayout.Button("保存文本数据"))
            {
                obj.Save();
            }
            if (GUILayout.Button("清空加载的文本数据"))
            {
                obj.Claer();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }

    }

}
