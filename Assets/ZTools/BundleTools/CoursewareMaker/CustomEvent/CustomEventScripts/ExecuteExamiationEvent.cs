using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 实操课程模型触发事件，结合ExecuteExamiation的配置数据
/// 1 根据ExecuteExamiation数据配置
/// 2 点击物体向ExecuteExamiation发送消息
/// 3 ExecuteExamiation进行处理
/// 4 发送消息到客户端，如得分情况
/// </summary>
public class ExecuteExamiationEvent : BundleEventInfoParameterBase
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnBundleAction(PointerEventData eventData)
    {

    }

    public override void OnBundleAction(PointerEventData eventData, int _IntParameter)
    {

    }

    public override void OnBundleAction(PointerEventData eventData, float _FloatParameter)
    {

    }

    public override void OnBundleAction(PointerEventData eventData, string _StringParameter)
    {

    }

    public override void OnBundleAction(PointerEventData eventData, bool _BoolParameter)
    {
       
    }

    public override void OnReceiveMsg(string msg)
    {

    }

    public override bool supportPRS()
    {
        return true;
    }
}
