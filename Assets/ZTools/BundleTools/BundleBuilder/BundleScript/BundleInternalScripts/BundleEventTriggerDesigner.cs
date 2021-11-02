using System;
using System.Collections.Generic;
using UnityEngine;
using static BundleEventTrigger;

[AddComponentMenu("Bundle/BundleEventTriggerDesigner")]
//暂定自动添加BoxCollider
[RequireComponent(typeof(Collider))]
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
    /// <summary>
    /// 从BundleEventTrigger设计师组件获取json格式数据
    /// </summary>
    /// <returns></returns>
    public BundleEventTriggerDesignerJson GetJsonInfo() {
        BundleEventTriggerDesignerJson temp = new BundleEventTriggerDesignerJson();

        for (int i = 0; i < bundleEventTriggerInfos.Count;i++)
        {
            BundleEventTriggerJson betj = new BundleEventTriggerJson();
            betj.target = bundleEventTriggerInfos[i].target.name;
            betj.method = bundleEventTriggerInfos[i].method.name;
            betj.triggerType = (int)bundleEventTriggerInfos[i].triggerType;
            betj.parameterMode = (int)bundleEventTriggerInfos[i].parameterMode;
            //根据类型赋值
            switch (bundleEventTriggerInfos[i].parameterMode)
            {
                case BundleListenerMode.None:
                    ; break;
                case BundleListenerMode.Void:
                    ; break;
                case BundleListenerMode.Int:
                    betj.IntParameter = bundleEventTriggerInfos[i].IntParameter; break;
                case BundleListenerMode.Float:
                    betj.FloatParameter = bundleEventTriggerInfos[i].FloatParameter; break;
                case BundleListenerMode.String:
                    betj.StringParameter = bundleEventTriggerInfos[i].StringParameter; break;
                case BundleListenerMode.Bool:
                    betj.BoolParameter = bundleEventTriggerInfos[i].BoolParameter; break;
            }
            temp.bundleEventTriggerJsons.Add(betj);
        }
        return temp;
    }
}


