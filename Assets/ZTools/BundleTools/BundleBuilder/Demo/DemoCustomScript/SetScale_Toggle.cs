using UnityEngine.EventSystems;
/// <summary>
/// 这是一个自定义事件的模板
/// 重复点击以设置物体Scale
/// 自定义脚本必须继承自BundleEventInfoBase类
/// ---------------------必须实现的函数------------------------------------
///         OnBundleAction =====>  执行的事件
///         OnReceiveMsg   =====>  执行本地事件后其他客户端接收回调，通常和OnBundleAction内一样但不调用SendMsg
///         supportPRS     =====>  一个动画，返回值为False即可
/// ---------------------必须在OnBundleAction实现后调用的函数--------------
///         SendMsg        =====>  执行本地事件后发送消息给其他客户端
/// 
///  ---------------------否则---------------------------------------------
///  1 OnBundleAction中没有调用SendMsg()和传递正确值
///  2 没有重写OnReceiveMsg函数
///  会导致多人无法互动
///  
/// 
/// </summary>
public class SetScale_Toggle : BundleEventInfoBase {

    /// <summary>
    /// 自定义值，一般为private
    /// </summary>
    private bool scale;
	void Start () {
        scale = true;
    }
    /// <summary>
    /// 事件执行主体
    /// </summary>
    /// <param name="eventData"></param>
    public override void OnBundleAction(PointerEventData eventData) {
        scale = !scale;
        gameObject.transform.localScale = scale ?  UnityEngine.Vector3.one: UnityEngine.Vector3.one * 2f;
        SendMsg(scale.ToString());
    }
    /// <summary>
    /// 执行本地事件后其他客户端接收回调，通常和OnBundleAction内一样但不调用SendMsg
    /// </summary>
    /// <param name="msg">接受参数，需要转换一下</param>
    public override void OnReceiveMsg(string msg) {
        scale = bool.Parse(msg);
        gameObject.transform.localScale = scale ? UnityEngine.Vector3.one : UnityEngine.Vector3.one * 2f;
    }
    /// <summary>
    /// 一个动画，返回值为False即可
    /// </summary>
    /// <returns></returns>
    public override bool supportPRS() {
        return false;
    }
}
