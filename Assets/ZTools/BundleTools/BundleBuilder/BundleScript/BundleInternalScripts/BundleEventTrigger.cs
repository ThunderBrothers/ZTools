using System;
using System.Collections.Generic;
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
    public bool preview = false;
    public BundleEventTriggerDesigner selfDesigner;

    private void Start() {
        if (preview)
        {
            selfDesigner = gameObject.GetComponent<BundleEventTriggerDesigner>();
            if (selfDesigner != null)
            {
                selfBundleEventTriggerInfo = selfDesigner.bundleEventTriggerInfos;
            }
        }
    }

    public void AddTriggerByInfo(BundleEventTriggerInfo info) {
        if (info != null)
        {
            selfBundleEventTriggerInfo.Add(info);
        }
    }
    //参数调用
    public void AddTriggerByElement(GameObject _target, UnityEngine.Object _method, int _triggerType, int _parameterMode) {
        BundleEventTriggerInfo temp = new BundleEventTriggerInfo(_target, _method, _triggerType, _parameterMode);
        if (temp != null)
        {
            selfBundleEventTriggerInfo.Add(temp);
        }
    }
    //有参数调用
    public void AddTriggerByElement(GameObject _target, UnityEngine.Object _method, int _triggerType, int _parameterMode, int _parameter) {
        BundleEventTriggerInfo temp = new BundleEventTriggerInfo(_target, _method, _triggerType, _parameterMode, _parameter);
        if (temp != null)
        {
            selfBundleEventTriggerInfo.Add(temp);
        }
    }
    public void AddTriggerByElement(GameObject _target, UnityEngine.Object _method, int _triggerType, int _parameterMode, float _parameter) {
        BundleEventTriggerInfo temp = new BundleEventTriggerInfo(_target, _method, _triggerType, _parameterMode, _parameter);
        if (temp != null)
        {
            selfBundleEventTriggerInfo.Add(temp);
        }
    }
    public void AddTriggerByElement(GameObject _target, UnityEngine.Object _method, int _triggerType, int _parameterMode, string _parameter) {
        BundleEventTriggerInfo temp = new BundleEventTriggerInfo(_target, _method, _triggerType, _parameterMode, _parameter);
        if (temp != null)
        {
            selfBundleEventTriggerInfo.Add(temp);
        }
    }
    public void AddTriggerByElement(GameObject _target, UnityEngine.Object _method, int _triggerType, int _parameterMode, bool _parameter) {
        BundleEventTriggerInfo temp = new BundleEventTriggerInfo(_target, _method, _triggerType, _parameterMode, _parameter);
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
    /// 判断逻辑  
    /// 1 ent元素判空
    /// 2 ent.parameterMode执行参数类型
    /// 3 preview预览模式
    /// 4 根据预览模式判断函数类型
    /// </summary>
    /// <param name="id"></param>
    /// <param name="eventData"></param>
    private void Execute(BundleEventTriggerType id, PointerEventData eventData) {
        for (int i = 0, imax = triggers.Count; i < imax; ++i)
        {
            var ent = triggers[i];
            if (ent.target != null && ent.method != null)
            {
                if (ent.target != null && ent.triggerType == id && ent.method != null)
                {
                    //执行不带参数的方法
                    if (ent.parameterMode == BundleListenerMode.None)
                    {
                        Type type = ent.method.GetType();
                        //获取targer上所有组件
                        //GetComponents<T>和GetComponents(Type)是获取不到运行时加载出来的组件必须这样获取后处理
                        Component[] components = ent.target.GetComponents<Component>();
                        for (int j = 0; j < components.Length; j++)
                        {
                            Type typecomponents = components[j].GetType();
                            if (preview)
                            {
                                if (typecomponents.FullName == ent.method.name)
                                {
                                    //Debug.Log("预览模式触发" + ent.triggerType + "事件---->操作对象" + ent.target + "---->执行" + typecomponents);
                                    //执行
                                    ((BundleEventInfoBase)components[j]).OnBundleAction(eventData);
                                }
                            }
                            else
                            {
                                if (typecomponents == type)
                                {
                                    //Debug.Log("View触发" + ent.triggerType + "事件---->操作对象" + ent.target + "---->执行" + typecomponents);
                                    //执行
                                    ((BundleEventInfoBase)components[j]).OnBundleAction(eventData);
                                }
                            }
                        }
                    }
                    //执行带参数的方法（其中包括无参数的执行方法）
                    else
                    {
                        Type type = ent.method.GetType();
                        //获取targer上所有组件
                        //GetComponents<T>和GetComponents(Type)是获取不到运行时加载出来的组件必须这样获取后处理
                        Component[] components = ent.target.GetComponents<Component>();
                        for (int j = 0; j < components.Length; j++)
                        {
                            Type typecomponents = components[j].GetType();
                            if (preview)
                            {
                                if (typecomponents.FullName == ent.method.name)
                                {
                                    //Debug.Log("预览模式触发" + ent.triggerType + "事件---->操作对象" + ent.target + "---->执行" + typecomponents);
                                    //执行
                                    switch (ent.parameterMode)
                                    {
                                        case BundleListenerMode.Void:
                                            ((BundleEventInfoParameterBase)components[j]).OnBundleAction(eventData); break;
                                        case BundleListenerMode.Int:
                                            ((BundleEventInfoParameterBase)components[j]).OnBundleAction(eventData, ent.IntParameter); break;
                                        case BundleListenerMode.Float:
                                            ((BundleEventInfoParameterBase)components[j]).OnBundleAction(eventData, ent.FloatParameter); break;
                                        case BundleListenerMode.String:
                                            ((BundleEventInfoParameterBase)components[j]).OnBundleAction(eventData, ent.StringParameter); break;
                                        case BundleListenerMode.Bool:
                                            ((BundleEventInfoParameterBase)components[j]).OnBundleAction(eventData, ent.BoolParameter); break;
                                        default: break;
                                    }
                                }
                            }
                            else
                            {
                                if (typecomponents == type)
                                {
                                    //Debug.Log("View触发" + ent.triggerType + "事件---->操作对象" + ent.target + "---->执行" + typecomponents);
                                    //执行
                                    switch (ent.parameterMode)
                                    {
                                        case BundleListenerMode.Void:
                                            ((BundleEventInfoParameterBase)components[j]).OnBundleAction(eventData); break;
                                        case BundleListenerMode.Int:
                                            ((BundleEventInfoParameterBase)components[j]).OnBundleAction(eventData, ent.IntParameter); break;
                                        case BundleListenerMode.Float:
                                            ((BundleEventInfoParameterBase)components[j]).OnBundleAction(eventData, ent.FloatParameter); break;
                                        case BundleListenerMode.String:
                                            ((BundleEventInfoParameterBase)components[j]).OnBundleAction(eventData, ent.StringParameter); break;
                                        case BundleListenerMode.Bool:
                                            ((BundleEventInfoParameterBase)components[j]).OnBundleAction(eventData, ent.BoolParameter); break;
                                        default: break;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (ent.triggerType == id)
                {
                    Debug.Log("物体为" + ent.target.name + "执行函数为" + ent.method);
                }
            } else
            {
                Debug.LogError($"执行物体{ent.target}为空");
                Debug.LogError($"者执行脚本{ent.method}为空");
            }
        }
    }
    public virtual void OnPointerEnter(PointerEventData eventData) {
        Debug.Log("GazeOn");
        Execute(BundleEventTriggerType.GazeOn, eventData);
    }

    public virtual void OnPointerExit(PointerEventData eventData) {
        Debug.Log("GazeOff");
        Execute(BundleEventTriggerType.GazeOff, eventData);
    }

    public virtual void OnPointerClick(PointerEventData eventData) {
        Debug.Log("GazeClick");
        Execute(BundleEventTriggerType.GazeClick, eventData);
    }
}
