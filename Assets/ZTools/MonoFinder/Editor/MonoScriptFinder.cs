using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

namespace ZTools.MonoScriptFinder
{
    public class MonoScriptFinder : EditorWindow
    {
        public MonoScript scriptObj = null;
        int loopCount = 0;
        Transform selectRoot = null;
        List<Transform> roots = new List<Transform>();
        List<Transform> result = new List<Transform>();
        bool SearchAll = true;//查找所有
        bool SearchSelectOnly = false;//查找选择的物体
        bool temp = true;


        [MenuItem("ZTools/FindScript")]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(MonoScriptFinder));
        }
        private void OnGUI()
        {
            if (GUILayout.Button("Reset"))
            {
                Claer();
                scriptObj = null;
                loopCount = 0;
                selectRoot = null;
                roots = new List<Transform>();
                result = new List<Transform>();
                SearchAll = true;//查找所有
                SearchSelectOnly = false;//查找选择的物体
                temp = true;
            }
            //SearchAll = true 默认查找所有物体上的脚本
            //SearchAll = false 出现选项 
            SearchAll = GUILayout.Toggle(SearchAll, "查找所有物体");
            if (temp != SearchAll)
            {
                Claer();
                temp = SearchAll;
            }
            GUILayout.BeginVertical();
            if (!SearchAll)
            {
                GUILayout.Label("当前查找方式 点击可修改");
                //SearchSelectOnly = true 查找选择的物体
                if (SearchSelectOnly)
                {
                    if (GUILayout.Button("仅选择物体"))
                    {
                        SearchSelectOnly = false;
                    }
                }
                //SearchSelectOnly = false 查找所有根物体
                else
                {
                    if (GUILayout.Button("查找所有根物体"))
                    {
                        SearchSelectOnly = true;
                    }
                }
            }
            GUILayout.EndHorizontal();
            if (!SearchAll && SearchSelectOnly)
            {
                GUILayout.Label("选择节点");
                selectRoot = (Transform)EditorGUILayout.ObjectField(selectRoot, typeof(Transform), true);
            }
            GUILayout.Label("脚本类型");
            //得到脚本类型
            scriptObj = (MonoScript)EditorGUILayout.ObjectField(scriptObj, typeof(MonoScript), true);
            if (GUILayout.Button("Find"))
            {
                Claer();
                //查找选择的
                if (SearchSelectOnly)
                {
                    if (selectRoot == null)
                    {
                        EditorUtility.DisplayDialog("没有添加要查找的选择物体", "在选择框\"选择节点\"中添加Transform", "我知道了");
                    }
                    if (scriptObj == null)
                    {
                        EditorUtility.DisplayDialog("没有添加要查找脚本", "在选择框\"脚本类型\"中添加脚本", "我知道了");
                    }
                    FindScript(selectRoot, true);
                }
                //查找所有
                else
                {
                    //得到所有节点
                    Transform[] root = Transform.FindObjectsOfType<Transform>();
                    foreach (Transform t in root)
                    {
                        if (t.parent == null)
                        {
                            roots.Add(t);
                        }
                    }
                    loopCount = 0;
                    if (scriptObj == null)
                    {
                        EditorUtility.DisplayDialog("没有添加要查找脚本", "在选择框\"脚本类型\"中添加脚本", "我知道了");
                    }
                    foreach (Transform t in roots)
                    {
                        FindScript(t, SearchAll);
                    }
                }
            }
            if (result.Count > 0)
            {
                foreach (Transform t in result)
                {
                    EditorGUILayout.ObjectField(t, typeof(Transform), false);
                }
            }
            else
            {
                GUILayout.Label("无数据");
            }
        }
        /// <summary>
        /// 查找函数
        /// </summary>
        /// <param name="root">查找的节点</param>
        /// <param name="excavate">是否向下挖掘 继续查找子物体的节点</param>
        void FindScript(Transform root, bool excavate)
        {
            if (root != null && scriptObj != null)
            {
                loopCount++;
                if (root.GetComponent(scriptObj.GetClass()) != null)
                {
                    result.Add(root);
                }
                if (excavate)
                {
                    foreach (Transform t in root)
                    {
                        FindScript(t, excavate);
                    }
                }
            }
        }
        void Claer()
        {
            roots.Clear();
            result.Clear();
            //scriptObj = null;
        }
    }
}

