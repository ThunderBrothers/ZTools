using System;
using System.Collections.Generic;
using UnityEngine;
using static BundleEventTrigger;

[AddComponentMenu("Bundle/BundleEventTriggerDesigner")]
//暂定自动添加BoxCollider
[RequireComponent(typeof(BoxCollider))]
//使唯一
[DisallowMultipleComponent]
/// <summary>
/// BundleEventTrigger设计师
/// 构建数据给BundleEventTrigger执行
/// bundleEventTriggerInfos保存此BundleEventTriggerDesigner上定制的所有事件信息
/// </summary>
public class BundleEventTriggerDesigner : MonoBehaviour
{
    //预留生成操作log日志
    //public bool recordDebugLog = false;
    [Header("可排序ObjList")]
    public List<BundleEventTriggerInfo> bundleEventTriggerInfos = new List<BundleEventTriggerInfo>();
    public BundleEventTriggerDesignerJson GetJsonInfo() {
        BundleEventTriggerDesignerJson temp = new BundleEventTriggerDesignerJson();

        for (int i = 0; i < bundleEventTriggerInfos.Count;i++)
        {
            BundleEventTriggerJson betj = new BundleEventTriggerJson();
            betj.target = bundleEventTriggerInfos[i].target.name;
            betj.method = bundleEventTriggerInfos[i].method.name;
            betj.triggerType = (int)bundleEventTriggerInfos[i].triggerType;
            temp.bundleEventTriggerJsons.Add(betj);
        }
        return temp;
    }
}
///// <summary>
///// 单个触发事件信息
///// </summary>
//[Serializable]
//public class BundleEventTriggerInfo {
//    public GameObject target;
//    public UnityEngine.Object method;
//    public BundleEventTriggerType triggerType;
//}

