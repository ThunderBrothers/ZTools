using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZTools.AssetBundleBuildInDLL {
    #region 编辑时或保存时触发器Json信息类
    /// <summary>
    /// 所有Trigger和Desinger的对应关系配置
    /// </summary>
    public class AllTriggerToDesingerJson {
        public List<triggerToDesingerJson> allJson;
    }
    /// <summary>
    /// 单个Trigger和Desinger的对应关系配置
    /// </summary>
    public class triggerToDesingerJson {
        /// <summary>
        /// 当前Trigger名称，制作时名称+配置序号，以供加载时查询
        /// </summary>
        public string objName;
        /// <summary>
        /// Desinger设计师组件上的信息
        /// </summary>
        public BundleEventTriggerDesignerJson bundleEventTriggerDesigners;
    }
    /// <summary>
    /// 单个设计师信息
    /// </summary>
    public class BundleEventTriggerDesignerJson {
        /// <summary>
        /// 单个设计师上所有触发事件信息
        /// </summary>
        public List<BundleEventTriggerJson> bundleEventTriggerJsons = new List<BundleEventTriggerJson>();
    }
    /// <summary>
    /// 单个触发事件信息
    /// </summary>
    [Serializable]
    public class BundleEventTriggerJson {
        /// <summary>
        /// 执行物体名称
        /// </summary>
        public string target;
        /// <summary>
        /// 执行函数名称
        /// </summary>
        public string method;
        /// <summary>
        /// 触发类型Index
        /// </summary>
        public int triggerType;
    }
    #endregion


    #region 设计时或运行时触发器信息类
    /// <summary>
    /// 单个触发事件信息
    /// </summary>
    [Serializable]
    public class BundleEventTriggerInfo {
        public BundleEventTriggerInfo() {

        }
        public BundleEventTriggerInfo(GameObject _target, UnityEngine.Object _method, int _triggerType) {
            target = _target;
            method = _method;
            triggerType = (BundleEventTriggerType)_triggerType;
        }
        public BundleEventTriggerInfo(GameObject _target, UnityEngine.Object _method, BundleEventTriggerType _triggerType) {
            target = _target;
            method = _method;
            triggerType = _triggerType;
        }
        /// <summary>
        /// 执行物体
        /// </summary>
        public GameObject target;
        /// <summary>
        /// 执行脚本
        /// </summary>
        public UnityEngine.Object method;
        /// <summary>
        /// 触发类型
        /// </summary>
        public BundleEventTriggerType triggerType;
    }
    /// <summary>
    /// 触发类型
    /// </summary>
    public enum BundleEventTriggerType {
        GazeOn = 0,
        GazeOff = 1,
        GazeClick = 2,
    }
    #endregion
}
