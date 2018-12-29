using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using System.IO;

using System;
using System.Linq;
using System.Collections.Generic;

namespace ZTools.EditorExample
{
    [CustomEditor(typeof(CIList))]
    public class CIListEditor : Editor
    {
        ReorderableList _objList = null;
        ReorderableList _wavesList = null;
        ReorderableList _spriteList = null;

        private struct WaveCreationParams
        {
            public MobWave.WaveType Type;
            public string _Path;
        }

        private void OnEnable()
        {
            SetupObjList();
            SetupWaves();
            //SetupSprite();
        }
        /// <summary>
        /// 这里参考Anima2D插件的IkGroupEditor脚本
        /// </summary>
        void SetupObjList()
        {
            //我们使用的是SerializedProperty，因为这是在自定义检查器中使用属性的推荐方法。这使得代码更小，并且与Unity和Undo系统配合得很好
            SerializedProperty icListProperty = serializedObject.FindProperty("ObjList");
            if (icListProperty != null)
            {
                //绘制列表 拖曳排序 显示添加按钮 显示移除按钮等参数
                _objList = new ReorderableList(serializedObject, icListProperty, true, true, true, true);
                #region ReorderableList的所有回掉函数
                //drawHeaderCallback 绘制表头回调
                //drawFooterCallback 绘制尾部回调
                //drawElementCallback 绘制元素回调
                //drawElementBackgroundCallback 绘制元素背景回调
                //onReorderCallback 重新排序回调
                //onSelectCallback 选中回调
                //onAddCallback 添加按钮回调
                //onAddDropdownCallback 添加下拉选项回调
                //onRemoveCallback 移除元素回调
                //onMouseUpCallback 鼠标抬起回调
                //onCanRemoveCallback 是否显示可移除按钮回调
                //onChangedCallback 列表改变回调
                #endregion
                //绘制元素回调
                _objList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                    SerializedProperty boneProperty = _objList.serializedProperty.GetArrayElementAtIndex(index);
                    rect.y += 1.5f;
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), boneProperty, GUIContent.none);
                };
                //绘制表头回调
                _objList.drawHeaderCallback = (Rect rect) => {
                    EditorGUI.LabelField(rect, "可排序ObjList");
                };
            }
        }


        /// <summary>
        /// 设置怪物界面
        /// </summary>
        void SetupWaves()
        {
            SerializedProperty waveProperty = serializedObject.FindProperty("Waves");
            if (waveProperty != null)
            {
                _wavesList = new ReorderableList(serializedObject, waveProperty, true, true, true, true);
                _wavesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    //得到列表项 列表的一个元素及界面的一行
                    SerializedProperty element = _wavesList.serializedProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;
                    //FindPropertyRelative 查找结构体里的的属性
                    EditorGUI.PropertyField(new Rect(rect.x,rect.y,60f,EditorGUIUtility.singleLineHeight),element.FindPropertyRelative("Type"),GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x + 60,rect.y,rect.width - 60 - 30,EditorGUIUtility.singleLineHeight),element.FindPropertyRelative("Prefab"),GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.x + rect.width - 30,rect.y,30,EditorGUIUtility.singleLineHeight),element.FindPropertyRelative("Count"),GUIContent.none);
                };
                _wavesList.drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "怪物波设置");
                };
                _wavesList.onRemoveCallback = (ReorderableList list) =>
                {
                    if (EditorUtility.DisplayDialog("警告", "删除该元素？", "是", "否"))
                    {
                        ReorderableList.defaultBehaviours.DoRemoveButton(list);
                    }
                };
                //选择回调 当点击到拖拽三道杠时项目面板中突出显示相应的预制
                _wavesList.onSelectCallback = (ReorderableList list) =>
                {
                    GameObject prefab = list.serializedProperty.GetArrayElementAtIndex(list.index).FindPropertyRelative("Prefab").objectReferenceValue as GameObject;
                    if(prefab != null)
                    {
                        EditorGUIUtility.PingObject(prefab.gameObject);
                    }
                };
                //界面列表中只有一个元素，要禁用 - 按钮，但可以在数据List中设置 0
                //此回调函数有返回值
                _wavesList.onCanRemoveCallback = (ReorderableList list) =>
                {
                    return list.count > 1;
                };
                #region 设置默认 + 回调 
                //下拉菜单会覆盖此回调
                //_wavesList.onAddCallback = (ReorderableList list) =>
                //{
                //    if (_wavesList.serializedProperty != null)
                //    {
                //        list.serializedProperty.arraySize++;
                //        list.index = list.serializedProperty.arraySize - 1;
                //        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(list.index);
                //        //默认Enum的0
                //        element.FindPropertyRelative("Type").enumValueIndex = 0;
                //        element.FindPropertyRelative("Count").intValue = 20;
                //        //加载Prefab
                //        string path = "Assets/ZTools/EditorExample/Prefabs/Mod.prefab";
                //        GameObject obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                //        element.FindPropertyRelative("Prefab").objectReferenceValue = obj;
                //    }
                //    else
                //    {
                //        //defaultBehaviours包含各种功能的所有默认实现
                //        ReorderableList.defaultBehaviours.DoAddButton(list);
                //    }
                //};
                #endregion
                //添加 + 按钮的下拉菜单
                //菜单内容根据文件夹结构自动适配
                _wavesList.onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
                {
                    //一个目录
                    var menu = new GenericMenu();
                    //Mod文件夹下的资产GUID
                    //得到GUIDs字符串数组
                    var guids = AssetDatabase.FindAssets("", new[] { "Assets/ZTools/EditorExample/Prefabs/Mod" });
                    //遍历Mob文件夹下的子目录
                    foreach(var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        //增加一个目录项
                        //AddItem会自动把WaveCreationParams传入ClickHandLer函数中
                        menu.AddItem(new GUIContent("Mods/" + System.IO.Path.GetFileNameWithoutExtension(path)), false,
                            ClickHandLer,new WaveCreationParams() { Type = MobWave.WaveType.Mobs, _Path = path }); 
                    }
                    //第二个Boss文件夹
                    guids = AssetDatabase.FindAssets("", new[] { "Assets/ZTools/EditorExample/Prefabs/Boss" });
                    foreach(var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        menu.AddItem(new GUIContent("Bosses/" + System.IO.Path.GetFileNameWithoutExtension(path)), false,
                             ClickHandLer, new WaveCreationParams() { Type = MobWave.WaveType.Boss, _Path = path });
                    }
                    menu.ShowAsContext();
                }; 
            }
        }
        //菜单系统调用的回调
        private void ClickHandLer(object target)
        {
            var data = (WaveCreationParams)target;
            _wavesList.serializedProperty.arraySize++;
            _wavesList.index = _wavesList.serializedProperty.arraySize - 1;
            SerializedProperty element = _wavesList.serializedProperty.GetArrayElementAtIndex(_wavesList.index);
            element.FindPropertyRelative("Type").enumValueIndex = (int)data.Type;
            element.FindPropertyRelative("Count").intValue = data.Type == MobWave.WaveType.Boss ? 1 : 20;
            element.FindPropertyRelative("Prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath(data._Path, typeof(GameObject)) as GameObject;
            //这里自定义了添加 所以要应用界面逆向修改List数组
            serializedObject.ApplyModifiedProperties();
        }

        void SetupSprite()
        {
            SerializedProperty icListProperty = serializedObject.FindProperty("S");
            if (icListProperty != null)
            {
                //绘制列表 拖曳排序 显示添加按钮 显示移除按钮等参数
                _spriteList = CreateList(serializedObject, icListProperty);
                ////绘制元素回调
                //_objList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                //    SerializedProperty boneProperty = _objList.serializedProperty.GetArrayElementAtIndex(index);
                //    rect.y += 1.5f;
                //    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), boneProperty, GUIContent.none);
                //};
                //绘制表头回调
                _spriteList.drawHeaderCallback = (Rect rect) => {
                    EditorGUI.LabelField(rect, "可排序SpriteList");
                };
            }
        }

        ReorderableList CreateList(SerializedObject obj, SerializedProperty prop)
        {
            ReorderableList list = new ReorderableList(obj, prop, true, true, true, true);

            list.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "Sprites");
            };

            List<float> heights = new List<float>(prop.arraySize);

            list.drawElementCallback = (rect, index, active, focused) => {
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                Sprite s = (element.objectReferenceValue as Sprite);

                bool foldout = active;
                float height = EditorGUIUtility.singleLineHeight * 1.25f;
                if (foldout)
                {
                    height = EditorGUIUtility.singleLineHeight * 5;
                }

                try
                {
                    heights[index] = height;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Debug.LogWarning(e.Message);
                }
                finally
                {
                    float[] floats = heights.ToArray();
                    Array.Resize(ref floats, prop.arraySize);
                    heights = floats.ToList();
                }

                float margin = height / 10;
                rect.y += margin;
                rect.height = (height / 5) * 4;
                rect.width = rect.width / 2 - margin / 2;

                if (foldout)
                {
                    if (s)
                    {
                        EditorGUI.DrawPreviewTexture(rect, s.texture);
                    }
                }
                rect.x += rect.width + margin;
                EditorGUI.ObjectField(rect, element, GUIContent.none);
            };

            list.elementHeightCallback = (index) => {
                Repaint();
                float height = 0;

                try
                {
                    height = heights[index];
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Debug.LogWarning(e.Message);
                }
                finally
                {
                    float[] floats = heights.ToArray();
                    Array.Resize(ref floats, prop.arraySize);
                    heights = floats.ToList();
                }

                return height;
            };

            list.drawElementBackgroundCallback = (rect, index, active, focused) => {
                rect.height = heights[index];
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, new Color(0.33f, 0.66f, 1f, 0.66f));
                tex.Apply();
                if (active)
                    GUI.DrawTexture(rect, tex as Texture);
            };

            list.onAddDropdownCallback = (rect, li) => {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Add Element"), false, () => {
                    serializedObject.Update();
                    li.serializedProperty.arraySize++;
                    serializedObject.ApplyModifiedProperties();
                });

                menu.ShowAsContext();

                float[] floats = heights.ToArray();
                Array.Resize(ref floats, prop.arraySize);
                heights = floats.ToList();
            };

            return list;
        }


        /// <summary>
        /// 这里参考Anima2D插件的IkGroupEditor脚本
        /// https://blog.csdn.net/akof1314/article/details/49642109  某些函数参考
        /// http://va.lent.in/unity-make-your-lists-functional-with-reorderablelist/  类似特殊格式数据排序及预览等高级操作
        /// </summary>
        public override void OnInspectorGUI()
        {
            //绘制基础  如Script图标灰色的那个
            DrawDefaultInspector();
            //空开距离
            EditorGUILayout.Space();
            //更新序列化对象的表示 没弄明白这干嘛用的先留这
            //参考Anima2D插件的IkGroupEditor脚本
            serializedObject.Update();
            //绘制接口
            if (_objList != null)
            {
                _objList.DoLayoutList();
            }
            if(_wavesList != null)
            {
                _wavesList.DoLayoutList();
            }
            if (_spriteList != null)
            {
                _spriteList.DoLayoutList();
            }
            //界面可以修改 用界面逆向修改List数组 下面的 + - 选择等可以修改列表 如果没有这句话 只能是列表数据修改后界面也修改单向修改
            serializedObject.ApplyModifiedProperties();
        }
    }
}


