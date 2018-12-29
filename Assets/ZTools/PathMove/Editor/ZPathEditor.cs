using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace ZTools.ZPath
{
	[CustomEditor(typeof(ZPath))]
	public class ZPathEditor : Editor {
		private ZPath _path;
		public static int count = 0;
		static GUIStyle _style = new GUIStyle();

		void OnEnable()
		{
			_path = (ZPath)target;
			_style.fontStyle = FontStyle.Bold;
			_style.normal.textColor = Color.white;
			if(!_path.isInitialized)
			{
				_path.isInitialized = true;
				_path.pathName = "New Path " + ++count;
				_path.initialName = _path.pathName;
			}
		}
		public override void OnInspectorGUI()
		{
			EditorGUILayout.Space();
			Undo.RecordObject(target, "ZPath name");
			_path.pathName = EditorGUILayout.TextField(_path.pathName);
            if (string.IsNullOrEmpty(_path.pathName))
            {
                _path.pathName = _path.initialName;
            }
			_path.pathColor = EditorGUILayout.ColorField("Path color", _path.pathColor);
            GUILayout.BeginHorizontal();
            GUILayout.Label("半径");
            _path.radius = EditorGUILayout.Slider(_path.radius, 0f, 5f);
            GUILayout.EndHorizontal();
            _path.IsDebug = EditorGUILayout.Toggle("IsDebug", _path.IsDebug);
            if (_path.IsDebug)
            {
                _path.ShowNode = EditorGUILayout.Toggle("ShowNode", _path.ShowNode);
            }
            GUILayout.Space(20);
			EditorGUILayout.LabelField("Functions");
			ShowButtonDeleteAll();
			ShowButton();
			GUILayout.Space(20);
			EditorGUILayout.LabelField("Paths node data");
			EditorGUI.indentLevel = 1;
			ShowNodeList();
			if(GUI.changed)
				EditorUtility.SetDirty(_path);
		}
		void ShowNodeList()
		{
			for(int i = 0; i < _path.nodes.Count; i++)
			{
				EditorGUILayout.BeginHorizontal(); 
                _path.nodes[i] = EditorGUILayout.Vector3Field("node " + i, _path.nodes[i]);
				if(_path.nodes.Count > 2)
				{
					if(GUILayout.Button("-", GUILayout.Width(30)))
					{
						_path.nodes.RemoveAt(i);
						return;
					}
				}

				if(GUILayout.Button("+", GUILayout.Width(30)))
				{
					_path.nodes.Insert(i+1, _path.nodes[i]+ _path.transform.position + new Vector3(0,0,0.1f));
					return;
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		void ShowButton()
		{
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Add node"))
			{
				_path.nodes.Add(_path.nodes[_path.nodes.Count-1]+ new Vector3(0, 0, 1f) );
			}
			if(GUILayout.Button("Delete node"))
			{
				if(_path.nodes.Count > 2)
				{
					_path.nodes.RemoveAt(_path.nodes.Count-1);
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		void ShowButtonDeleteAll()
		{
			if(GUILayout.Button("Delete All"))
			{
				for(int i = 0; i < _path.nodes.Count-2; )
				{
					_path.nodes.RemoveAt(_path.nodes.Count - 1);
				}
			}
		}
		void OnSceneGUI()
		{
			Undo.RecordObject(target, "ZPath name");
            if (_path.nodes.Count < 2)
            {
                for (int i = _path.nodes.Count; i < 2; i++)
                {
                    _path.nodes.Add(Vector3.zero);
                }
            }
            ShowGUIInSecen(_path,_style);
		}
		public static void ShowGUIInSecen(ZPath path, GUIStyle style)
		{
			if(path == null)
				return;
			if(!path.gameObject.activeInHierarchy)
				return;

			Handles.Label(path.nodes[0], "'" + path.pathName + "' Begin", style);
			Handles.Label(path.nodes[path.nodes.Count - 1], "'" + path.pathName + "' End", style);

			for(int i = 0; i < path.nodes.Count; i++)
			{
				path.nodes[i] = Handles.PositionHandle(path.nodes[i], Quaternion.identity);
				if(i != 0 && i != path.nodes.Count - 1)
				{
					Handles.Label(path.nodes[i], "node " + i.ToString(), style);
				}
			}
		}
	}
	[CustomEditor(typeof(ZPath))]
	public class ZPathCreater : Editor
	{
		[MenuItem ("ZTools/CreatZPath")]
		static private void Creat()
		{
			GameObject go = new GameObject();
			go.AddComponent<ZPath>();
		}
	}
}