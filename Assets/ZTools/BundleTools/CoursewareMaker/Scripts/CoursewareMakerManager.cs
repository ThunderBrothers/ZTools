using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

/// <summary>
/// 打包物体的管理器，功能
/// 1 打包物体汇总
/// 2 定义课程打包物体的步骤
/// 3 输出打包列表
/// </summary>
public class CoursewareMakerManager : MonoBehaviour
{
    [Header("课程打包内容配置")]
    public BuildCoursewareData buildCoursewareData;

    [Header("打包配置")]
    public ScriptableObject scriptableObject;

    public Dictionary<string,CoursewareItemData> coursewareItemDatas;
}