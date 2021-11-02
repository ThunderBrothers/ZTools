using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

#region Json数据
/// <summary>
/// 课程打包前的数据定义
/// 维护一个总的课程数据，为应用打包及加载提供索引
/// </summary>
[Serializable]
public class CoursewareData
{
    public string coursewareName;
    /// <summary>
    /// 所有配置好的一级菜单
    /// </summary>
    public List<StageData> stageDatas = new List<StageData>();
}

/// <summary>
/// 课程打包前的数据定义
/// 维护一个数据，为应用打包及加载提供索引
/// </summary>
[Serializable]
public class StageData
{
    /// <summary>
    /// 一级菜单，课程内容部分如，菜单名称可配置
    /// 1 场景介绍 2 设备讲解 3 实操培训 4 评价考核
    /// </summary>
    public string stageName;
    /// <summary>
    /// 二级菜单
    /// </summary>
    public List<SubdivideMenuData> subdivideMenuDatas = new List<SubdivideMenuData>();
}

/// <summary>
/// 二级菜单的数据格式
/// </summary>
[Serializable]
public class SubdivideMenuData
{
    /// <summary>
    /// 二级菜单，如场景介绍中的二级列表
    /// 1 光纤熔接简介 2 故障分析
    /// </summary>
    public string menuName;
    /// <summary>
    /// 当前细分目录下的课程Bundle配置
    /// </summary>
    public List<CoursewareItemData> coursewareItemDatas = new List<CoursewareItemData>();
}


#endregion

#region 打包时的实时数据,此数据会转换到Json数据的对应上去，给到zip包内方便应用加载配置

/// <summary>
/// 课程打包前的数据定义
/// 维护一个总的课程数据，为应用打包及加载提供索引
/// </summary>
[Serializable]
public class BuildCoursewareData
{
    public string coursewareName = "coursewareName";
    /// <summary>
    /// 所有配置好的一级菜单
    /// </summary>
    public List<BuildStageData> stageDatas;
}
/// <summary>
/// 课程打包前的数据定义
/// 维护一个数据，为应用打包及加载提供索引
/// </summary>
[Serializable]
public class BuildStageData
{
    /// <summary>
    /// 一级菜单，课程内容部分如，菜单名称可配置
    /// 1 场景介绍 2 设备讲解 3 实操培训 4 评价考核
    /// </summary>
    public string stageName;
    /// <summary>
    /// 二级菜单
    /// </summary>
    public List<BuildSubdivideMenuData> subdivideMenuDatas;
}

/// <summary>
/// 二级菜单的数据格式
/// </summary>
[Serializable]
public class BuildSubdivideMenuData
{
    /// <summary>
    /// 二级菜单，如场景介绍中的二级列表
    /// 1 光纤熔接简介 2 故障分析
    /// </summary>
    public string menuName;
    /// <summary>
    /// 当前细分目录下的课程Bundle配置
    /// </summary>
    public List<StepConfig> stepConfigs;
}

/// <summary>
/// 步骤配置
/// </summary>
[Serializable]
public class StepConfig
{
    public CoursewareType coursewareType = CoursewareType.Default;
    public string setpName;
    [JsonIgnore]
    public GameObject target;
    [HideInInspector]
    public string bundleFileName;
    /// <summary>
    /// StepConfig的位置在整体配置表得序号
    /// 格式stage = i，subdivide = j，setp = k
    /// "#" + i + "@" + j + "@" + k
    /// </summary>
    [HideInInspector]
    public string indexCode;
}


[Serializable]
/// <summary>
/// 单个步骤的配置文件
/// </summary>
public class CoursewareItemData
{
    /// <summary>
    /// 步骤下标，0开始
    /// </summary>
    public string index;
    /// <summary>
    /// Prefab名称
    /// </summary>
    public string name;
    /// <summary>
    /// 步骤名称
    /// </summary>
    public string stepName;
    /// <summary>
    /// 课程内容类型
    /// </summary>
    public CoursewareType coursewareType;
    /// <summary>
    /// 打包时的物体
    /// </summary>
    public GameObject targetPrefab;
    /// <summary>
    /// 对应生成的bundle保存位置，包含文件名
    /// </summary>
    public string assetBundlePath;
    /// <summary>
    /// 生成的文件索引
    /// </summary>
    public UnityEngine.Object assetbundleFile;
}


/// <summary>
/// 打包生成的json格式数据
/// 应用下载zip包后用于加载
/// </summary>
public class CoursewareBuildData
{
    /// <summary>
    /// 课程类型
    /// anchor类型的只有一个bundle，并且不需要加载模型
    /// </summary>
    public CoursewareType coursewareType;
    /// <summary>
    /// 课程名称
    /// </summary>
    public string coursewareName;
    /// <summary>
    /// 步骤
    /// </summary>
    public List<CoursewareBuildItemData> allCoursewareBuildItemDatas;
}

/// <summary>
/// 课程类型
/// </summary>
public enum CoursewareType
{
    /// <summary>
    /// 普通课程
    /// </summary>
    Default = 1,
    /// <summary>
    /// 锚点
    /// </summary>
    Anchor = 2,
    /// <summary>
    /// 其他课程
    /// </summary>
    Other1 = 3,
    /// <summary>
    /// 普通课程Pro
    /// </summary>
    OrdinaryPro = 4,
    /// <summary>
    /// 普通课程Pro附带识别
    /// </summary>
    OrdinaryProRec = 5
}

#endregion

/// <summary>
/// 对应每个步骤加载时的配置json文件
/// </summary>
public class CoursewareBuildItemData
{
    /// <summary>
    /// 步骤序号
    /// </summary>
    public int index;
    /// <summary>
    /// 课程内容部分
    /// </summary>
    public CoursewareType coursewareType;
    /// <summary>
    /// 步骤名称
    /// </summary>
    public string stepName;
    /// <summary>
    /// bundle文件名，不包含后缀名
    /// </summary>
    public string bundleFileName;
}


