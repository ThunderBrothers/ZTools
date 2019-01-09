using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class BundleEventTrigger : MonoBehaviour
{
    [Header("可排序ObjList")]
    public BundleEventInfo bundleEventInfo;
    public List<BundleEventTriggerInfo> _bundleEventTriggerInfos;

}
[Serializable]
public class BundleEventInfo {
    
    public List<BundleEventTriggerInfo> bundleEventTriggerInfos;
}
[Serializable]
public class BundleEventTriggerInfo {
    public GameObject target;
    public UnityEngine.Object method;
    public PersistentListenerMode mode;
}

