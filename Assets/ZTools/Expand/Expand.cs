using UnityEngine;
using System.Collections.Generic;

namespace ZTools.Expand
{
    public static class Expand 
    {
        #region 关于GameObject
        //得到子物体中的组件 GameObject.GetComponentsInChildren<T>()会包含自己身上的组件
        public static T[] GetComponentsInRealChildren<T>(this GameObject go)
        {
            List<T> TList = new List<T>();
            TList.AddRange(go.GetComponentsInChildren<T>());
            TList.RemoveAt(0);
            return TList.ToArray();
        }

        #endregion
        #region 关于Debug.Log
        /// <summary>
        /// 快速定位到输出Log的物体上
        /// 和 Log(object message, Object context)一样
        /// </summary>
        /// <param name="go"></param>
        /// <param name="message"></param>
        public static void ZLog(this Object go,object message)
        {
            Object o = go;
            Debug.Log(message,o);
        }
        #endregion Debug.Log
    }

}

