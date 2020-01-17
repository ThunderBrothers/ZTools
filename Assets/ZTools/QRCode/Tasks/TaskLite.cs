using System;
using System.Collections.Generic;

namespace Aquaivy.Unity
{
    /// <summary>
    /// 主线程回调任务
    /// </summary>
    public class TaskLite
    {
        /// <summary>
        /// 累计时间
        /// </summary>
        public int Timer;

        /// <summary>
        /// 当前执行过的帧数
        /// </summary>
        public int Frame;

        /// <summary>
        /// 需要延迟多少帧执行
        /// </summary>
        public int DelayFrame;

        /// <summary>
        /// 关联数据
        /// </summary>
        public object Tag;

        /// <summary>
        /// 执行函数，
        /// 注意：timer的时间为时间启动后的时间，而不是间隔时间
        /// </summary>
        public Func<int, bool> Func { get; set; }

        /// <summary>
        /// 是否完成
        /// </summary>
        public volatile bool IsDone;

        /// <summary>
        /// 是否初始化
        /// </summary>
        public volatile bool IsInited;

        /// <summary>
        /// 错误次数
        /// </summary>
        public int ErrorCount;

        /// <summary>
        /// 释放这个任务
        /// </summary>
        public void Release()
        {
            Release(this);
        }

        #region 静态方法

        private static readonly object m_tasklock = new object();
        private static List<TaskLite> m_tasks = new List<TaskLite>(8);
        private static int currentOperateTaskIndex = 0;

        /// <summary>
        /// 压入一个执行函数
        /// 如果返回true，则表示函数执行完成
        /// 如果返回false，则下一帧还会继续执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delayFrame"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static TaskLite Invoke(Func<int, bool> action, int delayFrame = 0, object tag = null)
        {
            lock (m_tasklock)
            {
                var r = new TaskLite
                {
                    Func = action,
                    DelayFrame = delayFrame,
                    Tag = tag
                };
                m_tasks.Add(r);
                return r;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elapseTime"></param>
        public static void Update(int elapseTime)
        {
            if (m_tasks.Count <= 0)
                return;

            for (currentOperateTaskIndex = 0; currentOperateTaskIndex < m_tasks.Count; currentOperateTaskIndex++)
            {
                var task = m_tasks[currentOperateTaskIndex];
                task.Timer += elapseTime;
                task.Frame++;

                if (task.Frame >= task.DelayFrame)
                {
                    try
                    {
                        task.IsDone = task.Func(task.Timer);

                        if (task.IsDone)
                        {
                            Release(task);
                        }
                    }
                    catch (Exception ex)
                    {
                        Release(task);
                    }

                }
            }
        }


        private static void Release(TaskLite task)
        {
            int index = m_tasks.IndexOf(task);
            if (index < 0)
                return;

            int current = currentOperateTaskIndex;
            if (index <= current)
            {
                //移除当前索引之前的task
                //移除当前索引的task
                m_tasks.RemoveAt(index);
                currentOperateTaskIndex--;
            }
            else
            {
                //移除当前索引的之后的task
                m_tasks.RemoveAt(index);
            }
        }

        public static void ReleaseAll()
        {
            m_tasks.Clear();
            currentOperateTaskIndex = 0;
        }

        public static string GetInfo()
        {
            return $"All TaskLites count: {m_tasks.Count}";
        }

        #endregion
    }

}
