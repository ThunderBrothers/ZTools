using System;
using System.Collections.Generic;
using UnityEngine;

#region 加载时的json文件类
/// <summary>
/// 所有Trigger和Desinger的对应关系配置
/// </summary>
public class AllTriggerToDesingerJson {
    public List<triggerToDesingerJson> allJson;
}
/// <summary>
/// 单个Trigger和Desinger的对应关系配置
/// </summary>
public class triggerToDesingerJson {
    /// <summary>
    /// 当前Trigger名称，制作时名称+配置序号，以供加载时查询
    /// </summary>
    public string objName;
    /// <summary>
    /// Desinger设计师组件上的信息
    /// </summary>
    public BundleEventTriggerDesignerJson bundleEventTriggerDesigners;
}
/// <summary>
/// 单个设计师信息
/// </summary>
public class BundleEventTriggerDesignerJson {
    public List<BundleEventTriggerJson> bundleEventTriggerJsons = new List<BundleEventTriggerJson>();
}
/// <summary>
/// 单个触发事件信息
/// </summary>
[Serializable]
public class BundleEventTriggerJson {
    /// <summary>
    /// 被操作物体
    /// </summary>
    public string target;
    /// <summary>
    /// 执行函数
    /// </summary>
    public string method;
    /// <summary>
    /// 触发类型
    /// </summary>
    public int triggerType;
    /// <summary>
    /// 参数类型
    /// </summary>
    public int parameterMode;
    public int IntParameter;
    public float FloatParameter;
    public string StringParameter;
    public bool BoolParameter;
}
#endregion


#region 触发事件类
/// <summary>
/// 单个触发事件信息
/// </summary>
[Serializable]
public class BundleEventTriggerInfo {
    public BundleEventTriggerInfo() {
      
    }
    public BundleEventTriggerInfo(GameObject _target, UnityEngine.Object _method, BundleEventTriggerType _triggerType) {
        target = _target;
        method = _method;
        triggerType = _triggerType;
        parameterMode = BundleListenerMode.None;
        IntParameter = 0;
        FloatParameter = 0f;
        StringParameter = null;
        BoolParameter = false;
    }
    public BundleEventTriggerInfo(GameObject _target, UnityEngine.Object _method, int _triggerType) {
        target = _target;
        method = _method;
        triggerType = (BundleEventTriggerType)_triggerType;
        parameterMode = BundleListenerMode.None;
        IntParameter = 0;
        FloatParameter = 0f;
        StringParameter = null;
        BoolParameter = false;
    }
    public BundleEventTriggerInfo(GameObject _target, UnityEngine.Object _method, int _triggerType, int _parameterMode) {
        target = _target;
        method = _method;
        triggerType = (BundleEventTriggerType)_triggerType;
        parameterMode = (BundleListenerMode)_parameterMode;
        IntParameter = 0;
        FloatParameter = 0f;
        StringParameter = null;
        BoolParameter = false;
    }
    public BundleEventTriggerInfo(GameObject _target, UnityEngine.Object _method, int _triggerType, int _parameterMode, int _parameter) {
        target = _target;
        method = _method;
        triggerType = (BundleEventTriggerType)_triggerType;
        parameterMode = (BundleListenerMode)_parameterMode;
        IntParameter = _parameter;
    }
    public BundleEventTriggerInfo(GameObject _target, UnityEngine.Object _method, int _triggerType, int _parameterMode, float _parameter) {
        target = _target;
        method = _method;
        triggerType = (BundleEventTriggerType)_triggerType;
        parameterMode = (BundleListenerMode)_parameterMode;
        FloatParameter = _parameter;
    }
    public BundleEventTriggerInfo(GameObject _target, UnityEngine.Object _method, int _triggerType, int _parameterMode, string _parameter) {
        target = _target;
        method = _method;
        triggerType = (BundleEventTriggerType)_triggerType;
        parameterMode = (BundleListenerMode)_parameterMode;
        StringParameter = _parameter;
    }
    public BundleEventTriggerInfo(GameObject _target, UnityEngine.Object _method, int _triggerType, int _parameterMode, bool _parameter) {
        target = _target;
        method = _method;
        triggerType = (BundleEventTriggerType)_triggerType;
        parameterMode = (BundleListenerMode)_parameterMode;
        BoolParameter = _parameter;
    }

    public GameObject target;
    public UnityEngine.Object method;
    public BundleEventTriggerType triggerType;
    public BundleListenerMode parameterMode;
    public int IntParameter;
    public float FloatParameter;
    public string StringParameter;
    public bool BoolParameter;
}
/// <summary>
/// 触发类型
/// </summary>
public enum BundleEventTriggerType {
    GazeOn = 0,
    GazeOff = 1,
    GazeClick = 2,
}
/// <summary>
/// Bundle函数参数类型类型
/// </summary>
public enum BundleListenerMode {
    None = -1,
    Void = 0,
    Int = 1,
    Float = 2,
    String = 3,
    Bool = 4
}
#endregion

public class AllCustomExaminationDataJson
{
    public List<CustomExaminationData> customExaminationDatas;
}

[Serializable]
public class CustomExaminationData
{  
    /// <summary>
   /// 当前Trigger名称，制作时名称+配置序号，以供加载时查询
   /// </summary>
    public string objName;
    /// <summary>
    /// 1 Choice 选择题
    /// 2 Decide 判断题
    /// </summary>
    public QuestionType questionType;
    /// <summary>
    /// 题干
    /// </summary>
    public string stem;
    /// <summary>
    /// 分数
    /// </summary>
    public int score;
    /// <summary>
    /// 顺序题的正确顺序
    /// </summary>
    public string flow_answer = "234615";
    /// <summary>
    /// 选项
    /// </summary>
    public List<ExaminationItemData> examinationItemDatas;
}

[Serializable]
public enum QuestionType
{
    /// <summary>
    /// 选择题
    /// </summary>
    Choice = 1,
    /// <summary>
    /// 判断题
    /// </summary>
    Decide = 2,
    /// <summary>
    /// 顺序题
    /// </summary>
    Flow = 3

}
[Serializable]
public class ExaminationItemData
{
    /// <summary>
    /// 题目或者选项
    /// </summary>
    public string option;
    /// <summary>
    /// 是否 是正确答案
    /// </summary>
    public bool right;

    public ExaminationItemData()
    {
        option = "";
    }
}

