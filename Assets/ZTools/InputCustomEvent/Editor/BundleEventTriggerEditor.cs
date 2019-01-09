using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

[CustomEditor(typeof(BundleEventTrigger))]
public class BundleEventTriggerEditor : Editor
{
    ReorderableList eventInfo;
    private bool isMethodVoid = false;
    private PersistentListenerMode MethodMode = PersistentListenerMode.Void;

    private void OnEnable() {
        SetupBundleEventTriggerList();
    }


    /// <summary>
    /// 显示List界面
    /// </summary>
    void SetupBundleEventTriggerList() {
        SerializedProperty serializedProperty = serializedObject.FindProperty("_bundleEventTriggerInfos");
        if (serializedProperty != null)
        {
            eventInfo = new ReorderableList(serializedObject, serializedProperty, true, true, true, true);
            eventInfo.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = eventInfo.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, 120, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("target"), GUIContent.none);
                SerializedProperty _method = element.FindPropertyRelative("method");
                EditorGUI.PropertyField(new Rect(rect.x + 120, rect.y, rect.width - 120 - 120, EditorGUIUtility.singleLineHeight), _method, GUIContent.none);
                SerializedProperty _mode = element.FindPropertyRelative("mode");
                if(_method.objectReferenceValue != null){
                    EditorGUI.PropertyField(new Rect(rect.x + rect.width - 120, rect.y, 120, EditorGUIUtility.singleLineHeight), _mode, GUIContent.none);
                } 
            };
        }
    }
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        EditorGUILayout.Space();
        serializedObject.Update();
        if (eventInfo != null)
        {
            eventInfo.DoLayoutList();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
