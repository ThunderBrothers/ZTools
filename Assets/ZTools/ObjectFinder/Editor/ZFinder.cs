using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

namespace ZTools.Finder
{
    /// <summary>
    /// 查找工具类
    /// 
    /// </summary>
    public class ZFinder : EditorWindow
    {
        /// <summary>
        /// 搜索位置
        /// 分为场景内和资源内查找
        /// </summary>
        private string[] searchOptions = new string[]
        {
            "场景内",
            "Asset资源",
        };
        /// <summary>
        /// 搜索区域代号
        /// </summary>
        private int searchAreaOptionIndex = 0;
        /// <summary>
        /// 查找类型设置
        /// 简单的查找脚本和高级查找
        /// </summary>
        private string[] finderModuleOptions = new string[]
        {
            "查找脚本",
            "高级搜索",
        };
        /// <summary>
        /// 搜索等级代号
        /// 先保留搜索脚本功能，之后主攻高级搜索方法
        /// </summary>
        private int searchLevelOptionIndex = 1;
        /// <summary>
        /// 设置查找模式
        /// 物体自身属性,如查找隐藏的物体
        /// 组件查找，填入要查找的组件索引，如脚本
        /// 
        /// </summary>
        private string[] searcherOption = new string[]
        {
            "物体自身属性",
            "挂载组件",
            "被引用",
        };
        private int searcherOptionIndex = 0;
        private Vector2 mEditorSVPos = Vector2.zero;


        public MonoScript scriptObj = null;
        Transform selectRoot = null;
        List<Transform> roots = new List<Transform>();
        List<UnityEngine.Object> result = new List<UnityEngine.Object>();
        bool SearchAll = true;//查找所有
        bool SearchSelectOnly = false;//查找选择的物体
        bool temp = true;

        private List<UnityEngine.Object> finderResult = new List<UnityEngine.Object>();

        [MenuItem("ZTools/FindScript")]
        static void Init()
        {
            EditorWindow.GetWindow(typeof(ZFinder));
        }
        private void OnGUI()
        {
            if (GUILayout.Button("Reset"))
            {
                Claer();
                scriptObj = null;
                selectRoot = null;
                roots = new List<Transform>();
                result = new List<UnityEngine.Object>();
                SearchAll = true;//查找所有
                SearchSelectOnly = false;//查找选择的物体
                temp = true;
            }

            //SearchAll = true 默认查找所有物体上的脚本
            //SearchAll = false 出现选项 
            using (var tempSV = new EditorGUILayout.ScrollViewScope(mEditorSVPos))
            {
                mEditorSVPos = tempSV.scrollPosition;
                EditorGUILayout.Space();
                GroupExample("SearchOptions", SearchOptions);
                EditorGUILayout.Space();
                //搜索结果界面
                DrawSearchResult();
                EditorGUILayout.Space();
            }
            #region Test
            if (GUILayout.Button("Test"))
            {
                finderResult.Clear();
                result.Clear();
                finderResult = GetAllSceneObjects<Transform>();
                finderResult.ForEach(x => result.Add(x));

                DrawSearchResult();
            }
            #endregion
            
            #region 高级搜索逻辑
            //if (GUILayout.Button("FindMissingComponent"))
            //{
            //    Claer();
            //    //得到所有节点
            //    Transform[] root = Transform.FindObjectsOfType<Transform>();
            //    foreach (Transform t in root)
            //    {
            //        roots.Add(t);
            //    }
            //    foreach (Transform t in roots)
            //    {
            //        FindMissingComponent(t);
            //    }
            //}
            #endregion
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
        void FindMissingComponent(Transform root)
        {
            List<Component> components = new List<Component>();
            root.GetComponents(components);
            foreach (var component in components)
            {
                if (!component)
                {
                    result.Add(root);
                    continue;
                }
                SerializedObject so = new SerializedObject(component);
                var iter = so.GetIterator();
                while (iter.NextVisible(true))
                {
                    if (iter.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (iter.objectReferenceValue == null && iter.objectReferenceInstanceIDValue != 0)
                        {
                            result.Add(root);
                        }
                    }
                }
            }
        }


        #region [Kit]
        private void GroupExample(string varGroupName, Action varBusiness)
        {
            if (string.IsNullOrEmpty(varGroupName) == false) GUILayout.Label(varGroupName);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                if (null != varBusiness) varBusiness();
            }
            EditorGUILayout.Space();
        }
        #endregion

        private void SearchOptions()
        {
            GUILayout.Label("搜索内容设置");
            searchAreaOptionIndex = GUILayout.Toolbar(searchAreaOptionIndex, searchOptions);
            switch (searchAreaOptionIndex)
            {
                case 0:
                    HandleScenceSearcher(); break;
                case 1:
                    HandleAssetSearcher(); break;
                default: break;
            }
        }
        /// <summary>
        /// 处理场景内资源的搜索
        /// </summary>
        private void HandleScenceSearcher()
        {
            GUILayout.Label("搜索模式");
            searchLevelOptionIndex = GUILayout.Toolbar(searchLevelOptionIndex, finderModuleOptions);
            switch (searchLevelOptionIndex)
            {
                case 0:
                    DrawSearchScriptsWindow(); break;
                case 1:
                    DrawSearchAdvancedWindow(); break;
                default: break;
            }
            EditorGUILayout.Space();
        }
        /// <summary>
        /// 对工程内资源的处理
        /// </summary>
        private void HandleAssetSearcher()
        {
            EditorGUILayout.Space();
        }

        /// <summary>
        /// 搜索脚本界面绘制逻辑
        /// </summary>
        private void DrawSearchScriptsWindow()
        {
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
                if (SearchSelectOnly)
                {
                    if (GUILayout.Button("仅选择物体"))
                    {
                        SearchSelectOnly = false;
                    }
                }
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
            if (GUILayout.Button("FindScript"))
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

        }
        /// <summary>
        /// 绘制高级搜索界面
        /// </summary>
        private void DrawSearchAdvancedWindow()
        {
            GUILayout.Label("搜索方式");
            searcherOptionIndex = GUILayout.Toolbar(searcherOptionIndex, searcherOption);
            //根据搜索方式去绘制对应具体条件的界面
            switch (searcherOptionIndex)
            {
                case 0:
                    DrawDefaultSearchWindow(); break;
                case 1:
                    DrawComponentSearchWindow(); break;
                case 2:
                    DrawreFerencedSearchWindow(); break;
                default: break;
            }
        }

        private bool isSearchActive = true;
        private string gameobjectName = "";
        private void DrawDefaultSearchWindow()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.Space();
                isSearchActive = GUILayout.Toggle(isSearchActive, "查找激活");
                gameobjectName = EditorGUILayout.TextField("查找物体的名称", gameobjectName);
                //后续增加其他属性

                EditorGUILayout.Space();
            }
        }

        private UnityEngine.Object targetComponent;
        private void DrawComponentSearchWindow()
        {
            EditorGUILayout.Space();
            if (searchAreaOptionIndex == 0)
            {
                GUILayout.Label("选择场景内的物体");
                targetComponent = EditorGUILayout.ObjectField(targetComponent, typeof(UnityEngine.Object), true);

            }
            else if (searchAreaOptionIndex == 1)
            {
                GUILayout.Label("选择Asset文件夹的物体");
                targetComponent = EditorGUILayout.ObjectField(targetComponent, typeof(UnityEngine.Object), false);
            }

            //后续增加其他属性

            EditorGUILayout.Space();
        }

        private void DrawreFerencedSearchWindow()
        {

        }

        private void DrawSearchResult()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                if (result.Count > 0)
                {
                    foreach (UnityEngine.Object t in result)
                    {
                        EditorGUILayout.ObjectField(t, typeof(UnityEngine.Object), false);
                    }
                }
                else
                {
                    GUILayout.Label("无数据");
                    GUILayout.Label("无数据");
                    GUILayout.Label("无数据"); GUILayout.Label("无数据");
                }
            }
        }
        /// <summary>
        /// 查找所有场景内的UnityEngine.Object
        /// 获取所有节点
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private List<UnityEngine.Object> GetAllSceneObjects<T>() where T : Component
        {
            List<UnityEngine.Object> objects = new List<UnityEngine.Object>();
            var temp = Resources.FindObjectsOfTypeAll(typeof(T));
            //var temp = Resources.FindObjectsOfTypeAll(typeof(Transform));
            //1 强制转换到Transform
            //2 不为空
            //3 获取到GameObject
            //4 判断条件
            //5 返回
            objects = temp.Cast<Transform>()
                .Where(x => x != null)
                .Select(x => x.gameObject)
                //查看物体是否为激活等
                //.Where(x => x.activeInHierarchy == activeInHierarchy)
                //备注：这里Unity内部的一些东西吧 在Sence中的物体有好多默认不可见的，比Hierarchy窗口看到的要多好几个
                //1  x.scene.name不为空
                //2  GetActiveScene场景为当前场景，有些其他物体不在当前场景内
                .Where(x => x.scene.name != null && x.scene.name == SceneManager.GetActiveScene().name)
                .Cast<UnityEngine.Object>().ToList();
            return objects;
        }
        void Claer()
        {
            roots.Clear();
            result.Clear();
            //scriptObj = null;
        }
    }
}

