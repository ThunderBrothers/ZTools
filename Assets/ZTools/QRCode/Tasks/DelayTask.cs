using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aquaivy.Unity
{
    /// <summary>
    /// 延迟类型
    /// </summary>
    public enum DelayType
    {
        /// <summary>
        /// 按照帧数延迟
        /// </summary>
        Frame,

        /// <summary>
        /// 按照时间延迟，单位：ms
        /// </summary>
        Time
    }

    /// <summary>
    /// 延迟任务，可按照帧数或者ms毫秒值来延迟
    /// </summary>
    public class DelayTask
    {
        /// <summary>
        /// 延迟类型
        /// </summary>
        DelayType Type;

        /// <summary>
        /// 累计时间
        /// </summary>
        public int Timer { get { return taskLite.Timer; } }
        public int TimerRemaining { get { return Delay - taskLite.Timer; } }

        /// <summary>
        /// 当前执行过的帧数
        /// </summary>
        public int Frame { get { return taskLite.Frame; } }
        public int FrameRemaining { get { return Delay - taskLite.Frame; } }

        /// <summary>
        /// 需要延迟的数量
        /// </summary>
        public int Delay;

        /// <summary>
        /// 关联数据
        /// </summary>
        public object Tag;

        public Action Task;

        private TaskLite taskLite;

        public void Release()
        {
            Release(this);
        }

        /// <summary>
        /// 压入一个延迟函数，按照时间来延迟
        /// </summary>
        /// <param name="task"></param>
        /// <param name="delayTime">延迟时间，单位：ms毫秒</param>
        /// <param name="type"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static DelayTask Invoke(Action task, int delayTime, object tag = null)
        {
            var delayTask = new DelayTask
            {
                Task = task,
                Delay = delayTime,
                Type = DelayType.Time,
                Tag = tag
            };

            delayTask.taskLite = TaskLite.Invoke(t =>
            {
                if (t < delayTime)
                    return false;

                task();

                return true;
            });

            return delayTask;
        }

        /// <summary>
        /// 压入一个延迟函数，按照帧数来延迟
        /// </summary>
        /// <param name="task"></param>
        /// <param name="delayFrame">延迟帧数</param>
        /// <param name="type"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static DelayTask InvokeFrame(Action task, int delayFrame, object tag = null)
        {
            var delayTask = new DelayTask
            {
                Task = task,
                Delay = delayFrame,
                Type = DelayType.Frame,
                Tag = tag
            };

            delayTask.taskLite = TaskLite.Invoke(t =>
            {
                if (delayTask.Frame < delayFrame)
                    return false;

                task();

                return true;
            });

            return delayTask;
        }

        public static void Release(DelayTask delayTask)
        {
            delayTask.taskLite.Release();
        }

        public override string ToString()
        {
            if (Type == DelayType.Time)
                return $"DelayTask tag={Tag}  type={Type}  Time={Timer}/{Delay}";
            else
                return $"DelayTask tag={Tag}  type={Type}  Frame={Frame}/{Delay}";
        }
    }
}
