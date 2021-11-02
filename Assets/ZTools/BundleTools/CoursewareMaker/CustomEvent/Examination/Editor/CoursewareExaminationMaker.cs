using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

/// <summary>
/// 课程设计组件
/// </summary>
[CustomEditor(typeof(Examination))]
public class CoursewareExaminationMaker : Editor
{
    private Examination examination;
    private ReorderableList eventInfo;

    private void OnEnable()
    {
        examination = target as Examination;
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
        examination.customExaminationData.score = EditorGUILayout.IntField("该题目分数",examination.customExaminationData.score);
        EditorGUILayout.Space();

        GUILayout.BeginVertical();
        GUILayout.Label("题目");
        examination.customExaminationData.stem = GUILayout.TextField(examination.customExaminationData.stem);
        GUILayout.EndVertical();

        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.Label("题目类型(1 Choice 选择题 2 Decide 判断题)");
        EditorGUILayout.Space();
        examination.customExaminationData.questionType = (QuestionType)EditorGUILayout.EnumPopup(examination.customExaminationData.questionType);
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (eventInfo != null)
        {
            eventInfo.DoLayoutList();
        }
        serializedObject.ApplyModifiedProperties();
    }

    void SetupBundleEventTriggerList()
    {
        SerializedProperty serializedProperty = serializedObject.FindProperty("customExaminationData").FindPropertyRelative("examinationItemDatas");
        if (serializedProperty != null)
        {
            eventInfo = new ReorderableList(serializedObject, serializedProperty, true, true, true, true);
            eventInfo.elementHeight = 50;
            //标题
            eventInfo.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect ,"题干");
                EditorGUI.LabelField(new Rect(rect.width - rect.x - 50, rect.y, rect.width, rect.height), " 是否为正确答案");
            };
            //元素绘制
            eventInfo.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = eventInfo.serializedProperty.GetArrayElementAtIndex(index);
                SerializedProperty _StringParameter = element.FindPropertyRelative("option");
                _StringParameter.stringValue = EditorGUI.TextField(new Rect(rect.x, rect.y + 2, rect.width - 50, EditorGUIUtility.singleLineHeight), _StringParameter.stringValue);
                SerializedProperty _BoolParameter = element.FindPropertyRelative("right");
                _BoolParameter.boolValue = EditorGUI.Toggle(new Rect(rect.width - rect.x + 50, rect.y, rect.width, EditorGUIUtility.singleLineHeight), _BoolParameter.boolValue);
            };
            //添加 + 按钮的下拉菜单
            //菜单内容根据文件夹结构自动适配
            eventInfo.onAddCallback = (ReorderableList list) =>
            {
                eventInfo.serializedProperty.arraySize++;
                eventInfo.index = eventInfo.serializedProperty.arraySize - 1;
                SerializedProperty element = eventInfo.serializedProperty.GetArrayElementAtIndex(eventInfo.index);
                element.FindPropertyRelative("option").stringValue = "";
                element.FindPropertyRelative("right").boolValue = false;
                //这里自定义了添加 所以要应用界面逆向修改List数组
                serializedObject.ApplyModifiedProperties();
            };
            //移除菜单回调
            eventInfo.onRemoveCallback = (ReorderableList list) =>
            {

                if (EditorUtility.DisplayDialog("警告", "删除该元素？", "是", "否"))
                {
                    ////被移除的物体上脚本处理
                    //ExaminationItemData removeIndo = examinationItemDatas[list.index];//被移除物体配置信息
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                }
            };
            //界面改变回调
            eventInfo.onCanAddCallback = (ReorderableList list) =>
            {
                return true;
            };
        }
    }
}