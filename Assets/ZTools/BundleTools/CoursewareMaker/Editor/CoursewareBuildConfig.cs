using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 生成课程编辑预制体后打包步骤时会生成此文件
/// 用来记录打包课程的所有物体索引，包括生成的bundle文件
/// </summary>
public class CoursewareBuildConfig : ScriptableObject
{
    [Header("当前所有设置好的步骤文件列表")]
    /// <summary>
    /// 打包时的原始Prefab文件
    /// </summary>
    public List<GameObject> allStepTargetPrefabsOriginal;

    [Header("已经制作好的课程列表")]
  
    public BuildCoursewareData coursewareData;

    [SerializeField]
    [Header("已经制作好的Bundle列表")]
    /// <summary>
    /// 此课程中所有的步骤配置信息
    /// </summary>
    public List<CoursewareItemData> allCoursewareDatas;

    public void SetStepTargetPrefabsOriginal(List<StepConfig> data)
    {
        allStepTargetPrefabsOriginal = new List<GameObject>();
        GameObject temp;
        string path;
        data.ForEach((item) =>
        {
            //把Sence中引用转变为对文件资源的引用，做打包做准备
            //PrefabUtility.GetCorrespondingObjectFromOriginalSource 对于传递给此函数的任何对象，它将遵循相应对象的链，直到没有更多对象为止，并返回找到的最后一个对象。这对于查找对象源自的预制资产很有用
            //PrefabUtility.GetCorrespondingObjectFromSource         使用此函数获取source实例化的Prefab Asset对象。如果Prefab实例已断开连接，这还将从Prefab Asset中返回相应的对象，然后可以将其用于将Prefab实例重新连接到Prefab Asset。
            if (item.target != null)
            {
                temp = PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(item.target);
                path = AssetDatabase.GetAssetPath(item.target);
                if (temp != null)
                {
                    allStepTargetPrefabsOriginal.Add(temp);
                }
            }
        });
    }

    public void SetCoursewareDatas(List<CoursewareItemData> data, BuildCoursewareData buildCoursewareData)
    {
        //需要设置要不然退出项目不会被保存
        //1 SetDirty
        //2 然后修改数据
        //3 SaveAssets
        //并且只能在这里调用其他地方调用不知道为什么无效
        //SetDirty();
        EditorUtility.SetDirty(this);
        coursewareData = buildCoursewareData;
        Dictionary<string, CoursewareItemData> keyValuePairs = new Dictionary<string, CoursewareItemData>();
        allCoursewareDatas.ForEach((item) =>
        {
            keyValuePairs.Add(item.index, item);
        });

        data.ForEach((item) => {
            if (keyValuePairs.ContainsKey(item.index))
            {
                keyValuePairs[item.index] = item;
            }
            else
            {
                keyValuePairs.Add(item.index, item);
            }
        });
        List<CoursewareItemData> temp = new List<CoursewareItemData>();
        temp = keyValuePairs.Values.ToList();

        allCoursewareDatas = temp;
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
