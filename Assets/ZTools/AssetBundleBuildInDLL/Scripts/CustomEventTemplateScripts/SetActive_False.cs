using UnityEngine.EventSystems;
/// <summary>
/// 实例脚本
/// 设置设置物体SetActive
/// </summary>
public class SetActive_False : BundleEventInfoBase {

    public override void OnBundleAction(PointerEventData eventData) {
        gameObject.SetActive(false);
        SendMsg("false");
    }

    public override void OnReceiveMsg(string msg) {
        bool set = bool.Parse(msg);
        gameObject.SetActive(set);
    }

    public override bool supportPRS() {
        return true;
    }
}

