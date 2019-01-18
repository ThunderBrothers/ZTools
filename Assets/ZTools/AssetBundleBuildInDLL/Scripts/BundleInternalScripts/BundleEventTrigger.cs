using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;


[DisallowMultipleComponent]
public class BundleEventTrigger : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler {
    /// <summary>
    /// 单个触发器上的触发信息表
    /// </summary>
    //[SerializeField]
    public List<BundleEventTriggerInfo> selfBundleEventTriggerInfo = new List<BundleEventTriggerInfo>();
    //留作测试用生成预览
    //public BundleEventTriggerDesigner selfDesigner;

    public void AddTriggerByInfo(BundleEventTriggerInfo info) {
        if (info != null)
        {
            selfBundleEventTriggerInfo.Add(info);
        }
    }
    public void AddTriggerByElement(GameObject _target, UnityEngine.Object _method, int _triggerType) {
        BundleEventTriggerInfo temp = new BundleEventTriggerInfo(_target, _method, _triggerType);
        if (temp != null)
        {
            selfBundleEventTriggerInfo.Add(temp);
        }
    }
    public List<BundleEventTriggerInfo> triggers {
        get
        {
            if (selfBundleEventTriggerInfo == null)
                selfBundleEventTriggerInfo = new List<BundleEventTriggerInfo>();
            return selfBundleEventTriggerInfo;
        }
        set { selfBundleEventTriggerInfo = value; }
    }
  
    /// <summary>
    /// 执行事件
    /// </summary>
    /// <param name="id"></param>
    /// <param name="eventData"></param>
    private void Execute(BundleEventTriggerType id, PointerEventData eventData) {
        for (int i = 0, imax = triggers.Count; i < imax; ++i)
        {
            var ent = triggers[i];
            if (ent.target != null && ent.triggerType == id && ent.method != null)
            {
                Type type = ent.method.GetType();
                //获取targer上所有组件
                //GetComponents<T>和GetComponents(Type)是获取不到运行时加载出来的组件必须这样获取后处理
                Component[] components = ent.target.GetComponents<Component>();
                for (int j = 0;j < components.Length;j++)
                {
                    Type typecomponents = components[j].GetType();
                    if (typecomponents == type)
                    {
                        //Debug.Log("触发" + ent.triggerType + "事件---->操作对象" + ent.target + "---->执行" + typecomponents);
                        //执行
                        ((BundleEventInfoBase)components[j]).OnBundleAction(eventData);
                    }
                }
            }     
        }
    }
    public virtual void OnPointerEnter(PointerEventData eventData) {
        //Debug.Log("GazeOn");
        Execute(BundleEventTriggerType.GazeOn, eventData);
    }

    public virtual void OnPointerExit(PointerEventData eventData) {
        //Debug.Log("GazeOff");
        Execute(BundleEventTriggerType.GazeOff, eventData);
    }

    public virtual void OnPointerClick(PointerEventData eventData) {
        //Debug.Log("GazeClick");
        Execute(BundleEventTriggerType.GazeClick, eventData);
    }
}
