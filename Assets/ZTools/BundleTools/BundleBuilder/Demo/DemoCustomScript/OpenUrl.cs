using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OpenUrl : BundleEventInfoParameterBase
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
    /// <summary>
    /// 多人同步回调函数
    /// </summary>
    /// <param name="msg"></param>
    /// --------------------------------注意---------------------------------------------
    /// 只能实现对应的OnBundleAction重写函数中的一个即可，负责导致回调函数无法判断参数类型
    public override void OnReceiveMsg(string msg)
    {
        Debug.Log("OpenUrl OnReceiveMsg = " + msg);
        Application.OpenURL(msg);
    }

    public override bool supportPRS()
    {
        return false;
    }

    public override void OnBundleAction(PointerEventData eventData, int _IntParameter)
    {

    }

    public override void OnBundleAction(PointerEventData eventData, float _FloatParameter)
    {

    }

    public override void OnBundleAction(PointerEventData eventData, string _StringParameter)
    {
        Debug.Log("OpenUrl OnBundleAction = " + _StringParameter);
        Application.OpenURL(_StringParameter);
        SendMsg(_StringParameter);
    }

    public override void OnBundleAction(PointerEventData eventData, bool _BoolParameter)
    {

    }
}
