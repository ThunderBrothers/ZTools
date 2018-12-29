using System;
using System.Reflection;

namespace ZTools.SingletonModule {

    /// <summary>
    /// 单例模式模板
    /// </summary>
    /// <typeparam name="T">类名</typeparam>
    public class Singleton<T> where T : class {
        /// <summary>
        /// 用于多线程的环境，保证读取该变量的信息都是最新的，而无论其他线程如何更新这个变量
        /// </summary>
        private static volatile T instance;
        /// <summary>
        /// 限定资源
        /// </summary>
        private static object syncRoot = new Object();

        protected Singleton() { }

        public static T GetInstance {
            get
            {
                //多线程安全检测
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            Type t = typeof(T);
                            //返回为当前 Type 定义的所有公共构造函数
                            ConstructorInfo[] constructorInfos = t.GetConstructors();
                            if (constructorInfos.Length > 0)
                            {
                                throw new InvalidOperationException(String.Format("{0} has accesible constructor, can't enforce singleton behaviour", t.Name));
                            }
                            //使用与指定参数匹配程度最高的构造函数创建指定类型的实例
                            instance = (T)Activator.CreateInstance(t,true);
                        }
                    }
                }
                return instance;
            }
        }
    }
}


