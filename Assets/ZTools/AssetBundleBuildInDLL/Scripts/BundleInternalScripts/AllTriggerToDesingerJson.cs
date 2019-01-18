using System;
using System.Collections.Generic;
using UnityEngine;


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
    public string target;
    public string method;
    public int triggerType;
}
/// <summary>
/// 单个触发事件信息
/// </summary>
[Serializable]
public class BundleEventTriggerInfo {
    public BundleEventTriggerInfo() {
      
    }
    public BundleEventTriggerInfo(GameObject _target, UnityEngine.Object _method, int _triggerType) {
        target = _target;
        method = _method;
        triggerType = (BundleEventTriggerType)_triggerType;
    }

    public GameObject target;
    public UnityEngine.Object method;
    public BundleEventTriggerType triggerType;
}
/// <summary>
/// 触发类型
/// </summary>
public enum BundleEventTriggerType {
    GazeOn = 0,
    GazeOff = 1,
    GazeClick = 2,
}
