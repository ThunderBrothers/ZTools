using UnityEngine.EventSystems;

/// <summary>
/// 实例脚本
/// 设置设置物体SetActive
/// </summary>
public class SetActive_True : BundleEventInfoBase {

    public override void OnBundleAction(PointerEventData eventData) {
        gameObject.SetActive(true);
        SendMsg("true");
    }

    public override void OnReceiveMsg(string msg) {
        gameObject.SetActive(true);
    }

    public override bool supportPRS() {
        return true;
    }
}
