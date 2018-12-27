using UnityEngine;
using UnityEditor;
namespace ZTools.FileIO
{
    public class FileJosnIOChecker : MonoBehaviour
    {

        public string path;
        public string reader;
        public string writer;
        public JsonDataBase Json;

        public void Start()
        {

        }
        public void Save()
        {
            if (writer.Length <= 5)
            {
                return;
            }
            writer = JsonUtility.ToJson(Json);
            IOBase.WriteByFileStream(path, writer);
        }
        public void Load()
        {
            reader = IOBase.ReadByFileStream(path);
            Json = JsonUtility.FromJson<JsonDataBase>(reader);
        }
        public void Claer()
        {
            Json = null;
        }
    }

    [CustomEditor(typeof(FileJosnIOChecker))]
    public class FileIOJsonInspector : Editor
    {
        private FileJosnIOChecker obj;
        void OnEnable()
        {
            obj = (FileJosnIOChecker)target;
            obj.path = Application.dataPath;
            obj.path += "/ZTools/FileIO/JsonData.json";
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("快速的json配置文件读取查看及修改");
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("加载Json数据"))
            {
                obj.Load();
            }
            if (GUILayout.Button("保存Json数据"))
            {
                obj.Save();
            }
            if (GUILayout.Button("清空加载的Json数据"))
            {
                obj.Claer();
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }
    }
}
