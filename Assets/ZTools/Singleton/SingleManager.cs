using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZTools.SingletonModule {
    public class SingleManager : Singleton<SingleManager> {

        public string str = "这是一个单实例的模板";
        /// <summary>
        /// 构造函数
        /// </summary>
        private SingleManager() {

        }
    }
}

