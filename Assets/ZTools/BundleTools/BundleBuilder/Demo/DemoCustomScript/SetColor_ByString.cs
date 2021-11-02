using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 附带参数类型触发示例脚本
/// 设置设置物体的Color
/// 根据在设计师组件上写入的参数类型及参数执行一下对应重写函数
/// _StringParameter传入的string类型值
/// </summary>
public class SetColor_ByString : BundleEventInfoParameterBase {

    /// <summary>
    /// 多人同步回调函数
    /// </summary>
    /// <param name="msg"></param>
    /// --------------------------------注意---------------------------------------------
    /// 只能实现对应的OnBundleAction重写函数中的一个即可，负责导致回调函数无法判断参数类型

    public override void OnReceiveMsg(string msg) {
        MeshRenderer mesh = gameObject.GetComponent<MeshRenderer>();
        switch (msg)
        {
            case "black":
                mesh.material.color = Color.black; break;
            case "yellow":
                mesh.material.color = Color.yellow; break;
            case "white":
                mesh.material.color = Color.white; break;
            default: break;
        }
    }

    public override bool supportPRS() {
        return false;
    }

    public override void OnBundleAction(PointerEventData eventData) {

    }
    public override void OnBundleAction(PointerEventData eventData, int _IntParameter) {
        
    }
 
    public override void OnBundleAction(PointerEventData eventData, float _FloatParameter) {
      
    }
    /// <summary>
    /// 这里是重写float类型的触发逻辑
    /// /// --------------------------------注意---------------------------------------------
    /// 这里禁止实现所有重写方法因为在OnReceiveMsg无法判断参数类型
    /// 从而导致参数错误
    /// </summary>
    /// <param name="eventData"></param>
    /// <param name="_FloatParameter"></param>
    public override void OnBundleAction(PointerEventData eventData, string _StringParameter) {
        MeshRenderer mesh = gameObject.GetComponent<MeshRenderer>();
        switch (_StringParameter)
        {
            case "black":
                mesh.material.color = Color.black; break;
            case "yellow":
                mesh.material.color = Color.yellow; break;
            case "white":
                mesh.material.color = Color.white; break;
            default:break;
        }
        SendMsg(_StringParameter.ToString());
    }

    public override void OnBundleAction(PointerEventData eventData, bool _BoolParameter) {
        
    }
}
