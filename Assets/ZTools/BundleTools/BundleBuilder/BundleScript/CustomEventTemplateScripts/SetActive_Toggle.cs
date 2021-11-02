using UnityEngine.EventSystems;
/// <summary>
/// 实例脚本
/// 设置设置物体SetActive
/// </summary>
public class SetActive_Toggle : BundleEventInfoBase {
    public  override void OnBundleAction(PointerEventData eventData) {
        bool active = !gameObject.activeSelf;
        gameObject.SetActive(active);
        SendMsg((active).ToString());
    }

    public override void OnReceiveMsg(string msg) {
        bool set = bool.Parse(msg);
        gameObject.SetActive(set);
    }

    public override bool supportPRS() {
        return true;
    }
}
