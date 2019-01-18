using System;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using static BundleEventTrigger;

[CustomEditor(typeof(BundleEventTriggerDesigner))]
public class BundleEventTriggerEditor : Editor
{
    ReorderableList eventInfo;
    private bool isMethodVoid = false;
    //构建目标物体以修改事件
    private BundleEventTriggerDesigner bundleEventTriggerDesigner;
    //自定义脚本路径
    private string customScriptspath;
    private StreamReader sr = null;
    private bool init = false;

    private void OnEnable() {
        //ReadCustomStcriptsPath();
        bundleEventTriggerDesigner = (BundleEventTriggerDesigner)target;
        SetupBundleEventTriggerList();
    }


    /// <summary>
    /// 显示List界面
    /// </summary>
    void SetupBundleEventTriggerList() {
        SerializedProperty serializedProperty = serializedObject.FindProperty("bundleEventTriggerInfos");
        if (serializedProperty != null)
        {
            eventInfo = new ReorderableList(serializedObject, serializedProperty, true, true, true, true);
            //标题
            eventInfo.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Bundle触发器设置       Target物体      执行脚本         触发类型");
            };
            //元素绘制
            eventInfo.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = eventInfo.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                //脚本
                SerializedProperty _method = element.FindPropertyRelative("method");
                //参数类型
                SerializedProperty _mode = element.FindPropertyRelative("triggerType");
                //目标物体
                SerializedProperty _target = element.FindPropertyRelative("target");
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, 120, EditorGUIUtility.singleLineHeight), _target, GUIContent.none);
                EditorGUI.PropertyField(new Rect(rect.x + 120, rect.y, rect.width - 120 - 120, EditorGUIUtility.singleLineHeight), _method, GUIContent.none);
                //根据有无脚本绘制界面
                if(_method.objectReferenceValue != null){
                    EditorGUI.PropertyField(new Rect(rect.x + rect.width - 120, rect.y, 120, EditorGUIUtility.singleLineHeight), _mode, GUIContent.none);
                    //执行
                    if (_mode.enumValueIndex > 1)
                    {
                        //根据选择参数类型填入参数
                        //待续
                        switch (_mode.enumValueIndex)
                        {
                            case (int)PersistentListenerMode.EventDefined:;break;
                            default:;return;
                        }
                    }
                } 
            };
            //添加 + 按钮的下拉菜单
            //菜单内容根据文件夹结构自动适配
            eventInfo.onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
            {
                //一个目录 标准模板脚本
                var menu = new GenericMenu();
                //Mod文件夹下的资产GUID
                //得到GUIDs字符串数组
                var guids = AssetDatabase.FindAssets("", new[] { "Assets/ZTools/AssetBundleBuildInDLL/Scripts/CustomEventTemplateScripts" });
                //遍历CustomEventTemplateScripts文件夹下的子目录
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    //增加一个目录项
                    //AddItem会自动把WaveCreationParams传入ClickHandLer函数中
                    menu.AddItem(new GUIContent("TemplateEvent(自带模板)/" + System.IO.Path.GetFileNameWithoutExtension(path)), false,
                        ClickHandLer, new EventInfoCreationParams() { target = null, scriptPath = path, mode = BundleEventTriggerType.GazeClick});
                }
                //第二个目录
                //自定义内容
                ReadCustomStcriptsPath();
                if (!string.IsNullOrEmpty(customScriptspath) && customScriptspath != "")
                {
                    guids = AssetDatabase.FindAssets("", new[] { customScriptspath });
                    //遍历CustomEventTemplateScripts文件夹下的子目录
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        //增加一个目录项
                        //AddItem会自动把WaveCreationParams传入ClickHandLer函数中
                        menu.AddItem(new GUIContent("CustomEvent(自定义)/" + System.IO.Path.GetFileNameWithoutExtension(path)), false,
                            ClickHandLer, new EventInfoCreationParams() { target = null, scriptPath = path, mode = BundleEventTriggerType.GazeClick });
                    }
                }
                menu.ShowAsContext();
            };
            //移除菜单回调
            eventInfo.onRemoveCallback = (ReorderableList list) =>
            {

                if (EditorUtility.DisplayDialog("警告", "删除该元素？", "是", "否"))
                {
                    //被移除的物体上脚本处理
                    BundleEventTriggerInfo removeIndo = bundleEventTriggerDesigner.bundleEventTriggerInfos[list.index];//被移除物体配置信息
                    UnityEngine.Object @object = removeIndo.target;
                    if (@object != null)
                    {
                        //有脚本
                        BundleEventInfoBase bundleAction = ((GameObject)@object).GetComponent<BundleEventInfoBase>();
                        if (bundleAction != null)
                        {
                            //删除对应脚本
                            Type type = ((MonoScript)removeIndo.method).GetClass(); ;
                            DestroyImmediate(((GameObject)@object).GetComponent(type));
                        }
                    }
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };
            //界面改变回调
            eventInfo.onCanAddCallback = (ReorderableList list) =>
            {
                for (int i =0;i< list.count; i++)
                {
                    //每次界面对Target物体做修改时，物体不同就要处理
                    //原来的物体去掉添加的脚本，当前新选择添加脚本
                    SerializedProperty element = eventInfo.serializedProperty.GetArrayElementAtIndex(i);
                    UnityEngine.Object last = bundleEventTriggerDesigner.bundleEventTriggerInfos[i].target;//原来
                    UnityEngine.Object cur = element.FindPropertyRelative("target").objectReferenceValue;//当前
                    if (last != cur)
                    {
                        //去掉旧物体上的脚本
                        if (last != null)
                        {
                            BundleEventInfoBase bundleAction = ((GameObject)last).GetComponent<BundleEventInfoBase>();
                            if (bundleAction != null)
                            {
                                //生成之前物体脚本
                                DestroyImmediate(bundleAction);
                            }
                        }
                        //增加当前物体脚本
                        if (cur != null)
                        {
                            Type type = ((MonoScript)bundleEventTriggerDesigner.bundleEventTriggerInfos[i].method).GetClass();
                            ((GameObject)cur).AddComponent(type);
                            #region 不能重复添加
                            //Type type = ((MonoScript)bundleEventTriggerDesigner.bundleEventTriggerInfos[i].method).GetClass();
                            //if (((GameObject)cur).GetComponent<BundleEventInfoBase>() == null)
                            //{
                            //    ((GameObject)cur).AddComponent(type);
                            //}
                            //else
                            //{
                            //    Debug.Log("物体"+((GameObject)cur).name +"重复添加脚本" + type.Name);
                            //}
                            #endregion
                        }
                    }
                }
                return true;
            };
        }
    }
    /// <summary>
    /// 下拉菜单系统选择点击的回调
    /// </summary>
    /// <param name="target"></param>
    private void ClickHandLer(object target) {
        var data = (EventInfoCreationParams)target;
        eventInfo.serializedProperty.arraySize++;
        eventInfo.index = eventInfo.serializedProperty.arraySize - 1;
        SerializedProperty element = eventInfo.serializedProperty.GetArrayElementAtIndex(eventInfo.index);
        element.FindPropertyRelative("target").objectReferenceValue = null;
        element.FindPropertyRelative("method").objectReferenceValue = AssetDatabase.LoadAssetAtPath(data.scriptPath, typeof(UnityEngine.Object)) as UnityEngine.Object;
        //这里自定义了添加 所以要应用界面逆向修改List数组
        serializedObject.ApplyModifiedProperties();
    }
    /// <summary>
    /// 配置自定义脚本路径
    /// </summary>
    private void ReadCustomStcriptsPath() {
        string configFilePath = Application.dataPath + "/ZTools/AssetBundleBuildInDLL/Resources/BundleConfig/config.bytes";
        sr = new StreamReader(new FileStream(configFilePath, FileMode.Open));
        customScriptspath = sr.ReadLine();
        customScriptspath = sr.ReadLine();
        string[] split = { "Assets" };
        string[] results = customScriptspath.Split(split, StringSplitOptions.None);
        if (results.Length < 2)
        {
            customScriptspath = "";
            sr.Close();
            return;
        }
        customScriptspath = "Assets" + results[1];
        sr.Close();
    }

    /// <summary>
    /// 选择回调
    /// </summary>
    private struct EventInfoCreationParams {
        public GameObject target;
        public BundleEventTriggerType mode;
        public string scriptPath;
    }
    public override void OnInspectorGUI() {
        //绘制脚本基础
        //DrawDefaultInspector();
        EditorGUILayout.Space();
        serializedObject.Update();
        if (eventInfo != null)
        {
            eventInfo.DoLayoutList();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
