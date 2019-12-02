using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace ZTools.Expand
{
    /// <summary>
    /// 一个绘制Enum框的示例
    /// 根据一个父类绘制所有子类的枚举
    /// </summary>
    [CustomEditor(typeof(InteractableReceiver))]
    public class InteractableEventInspector : Editor
    {
        private static readonly GUIContent SelectEventLabel = new GUIContent("Select Event Type", "Select the event type from the list");
        Type receiverType;
      
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField(new GUIContent("Event Receiver Type"));
            using (new EditorGUILayout.HorizontalScope())
            {
                Rect position = EditorGUILayout.GetControlRect();
                SerializedProperty className = serializedObject.FindProperty("ClassName");
                using (new EditorGUI.PropertyScope(new Rect(0, 0, 0, 0), SelectEventLabel, className))
                {
                    var receiverTypes = TypeCacheUtility.GetSubClasses<ReceiverBase>();
                    var receiverClassNames = receiverTypes.Select(t => t?.Name).ToArray();
                    int id = Array.IndexOf(receiverClassNames, className.stringValue);
                    int newId = EditorGUI.Popup(position, id, receiverClassNames);
                  
                    if (newId == -1)
                    {
                        newId = 0;
                    }
                    receiverType = receiverTypes[newId];
                    if (id != newId)
                    {
                        className.stringValue = receiverType.Name;
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}