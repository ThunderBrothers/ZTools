using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(ExecuteExamiation))]
public class ExecuteExamiationInspector : Editor
{
    private ExecuteExamiation executeExamiation;
    private ReorderableList executeInfo;

    private void OnEnable()
    {
        executeExamiation = target as ExecuteExamiation;
        SetupBundleEventTriggerList();
    }

    public override void OnInspectorGUI()
    {
        //绘制脚本基础
        DrawDefaultInspector();
        EditorGUILayout.Space();
        serializedObject.Update();
        GUILayout.Label("题目编辑");
        EditorGUILayout.Space();
        executeExamiation.stem = EditorGUILayout.TextField("题干", executeExamiation.stem);
        EditorGUILayout.Space();
        executeExamiation.score = EditorGUILayout.IntField("该题目分数", executeExamiation.score);
        EditorGUILayout.Space();

        if (executeInfo != null)
        {
            executeInfo.DoLayoutList();
        }
        serializedObject.ApplyModifiedProperties();
    }

    void SetupBundleEventTriggerList()
    {
        SerializedProperty serializedProperty = serializedObject.FindProperty("executeExamiationTarget");
        if (serializedProperty != null)
        {
            executeInfo = new ReorderableList(serializedObject, serializedProperty, true, true, true, true);
            executeInfo.elementHeight = 30;
            //标题
            executeInfo.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "题干");
                EditorGUI.LabelField(new Rect(rect.width - rect.x - 50, rect.y, rect.width, rect.height), " 是否为正确答案");
            };
            //元素绘制
            executeInfo.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = executeInfo.serializedProperty.GetArrayElementAtIndex(index);
                SerializedProperty _ObjectParameter = element.FindPropertyRelative("target");
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, 240, EditorGUIUtility.singleLineHeight), _ObjectParameter, GUIContent.none);
                //_StringParameter.objectReferenceValue = EditorGUI.ObjectField(new Rect(rect.x, rect.y + 2, rect.width - 50, EditorGUIUtility.singleLineHeight), _StringParameter.objectReferenceValue);
                SerializedProperty _BoolParameter = element.FindPropertyRelative("right");
                _BoolParameter.boolValue = EditorGUI.Toggle(new Rect(rect.width - rect.x + 50, rect.y, rect.width, EditorGUIUtility.singleLineHeight), _BoolParameter.boolValue);
            };
            //添加 + 按钮的下拉菜单
            //菜单内容根据文件夹结构自动适配
            executeInfo.onAddCallback = (ReorderableList list) =>
            {
                executeInfo.serializedProperty.arraySize++;
                executeInfo.index = executeInfo.serializedProperty.arraySize - 1;
                SerializedProperty element = executeInfo.serializedProperty.GetArrayElementAtIndex(executeInfo.index);
                element.FindPropertyRelative("target").objectReferenceValue = null;
                element.FindPropertyRelative("right").boolValue = false;
                //这里自定义了添加 所以要应用界面逆向修改List数组
                serializedObject.ApplyModifiedProperties();
            };
            //移除菜单回调
            executeInfo.onRemoveCallback = (ReorderableList list) =>
            {

                if (EditorUtility.DisplayDialog("警告", "删除该元素？", "是", "否"))
                {
                    ////被移除的物体上脚本处理
                    //ExaminationItemData removeIndo = examinationItemDatas[list.index];//被移除物体配置信息
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };
            //界面改变回调
            executeInfo.onCanAddCallback = (ReorderableList list) =>
            {
                for (int i = 0; i < list.count; i++)
                {
                    //每次界面对Target物体做修改时，物体不同就要处理
                    //原来的物体去掉添加的脚本，当前新选择添加脚本
                    SerializedProperty element = executeInfo.serializedProperty.GetArrayElementAtIndex(i);
                    UnityEngine.Object last = executeExamiation.executeExamiationTarget[i].target;//原来
                    UnityEngine.Object cur = element.FindPropertyRelative("target").objectReferenceValue;//当前
                    if (last != cur)
                    {
                        //去掉旧物体上的脚本
                        if (last != null)
                        {
                            ExecuteEventTrigger bundleAction = ((GameObject)last).GetComponent<ExecuteEventTrigger>();
                            if (bundleAction != null)
                            {
                                //生成之前物体脚本
                                DestroyImmediate(bundleAction);
                            }
                        }
                        //增加当前物体脚本
                        if (cur != null)
                        {
                            if (((GameObject)cur).GetComponent<ExecuteEventTrigger>() == null)
                            {
                                ExecuteEventTrigger temp = ((GameObject)cur).AddComponent<ExecuteEventTrigger>();
                                temp.index = i;
                            }       
                        }
                    }
                }
                return true;
            };
        }
    }
}
