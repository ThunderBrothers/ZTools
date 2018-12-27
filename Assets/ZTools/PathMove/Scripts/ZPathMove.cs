using UnityEngine;
using System.Collections;
using System;

namespace ZTools.Path
{
    //表现类型
    public enum ZActionType : int
    {
        move = 0,
        stop = 1
    }

    //根据TBehavior数据结构配置行为
    //配置好的行为是按照点执行
    //Path的点要少于配置行为的个数
    //例：执行到path的第3个点 就去读取第3个配置的行为
    public class ZPathMove : MonoBehaviour
    {
        //表现
        [Serializable]
        public class ZBehavior
        {
            public int time;
            public ZActionType type;
        }
        [Header("本脚本要被继承 然后实现接口 此脚本挂在Player的父物体 移动控制此物体 旋转控制子物体的body", order = 1)]
        [Header("表现配置", order = 2)]
        public ZBehavior[] MyAction;
        public bool loop = false;
        public float speed = 2f;//速度
        public Transform body;//动物身体 控制朝向
        public ZBehavior _Action;//当前的表现
        private ZBehavior _last;//上一个表现
        private int index;//当前的目标点序号
        [Header("路径")]
        public ZPath path;
        private Vector3[] points;//初始化后目标点就不能编辑了
        private Vector3 _targetVec;//当前目标点Vector3
        private bool findNextPoint;
        private bool arrive;//到达
        private bool isStop;//是否停止

        void Init()
        {
            index = 0;
            points = path.nodes.ToArray();
            for (int i = 0; i < points.Length; i++)
            {
                points[i] += new Vector3(UnityEngine.Random.Range(-path.radius, path.radius), UnityEngine.Random.Range(-path.radius, path.radius), UnityEngine.Random.Range(-path.radius, path.radius));
            }
            _targetVec = points[index];
            findNextPoint = true;
            arrive = false;
            _Action = MyAction[0];
            _last = _Action;
            isStop = false;
            if (MyAction.Length < points.Length)
            {
                Debug.LogWarning("设置的动作数少于路径的点数");
            }
            OnPathStart();
        }
        private void OnDrawGizmos()
        {
            if (path.IsDebug)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position, _targetVec);
            }
        }

        void Start()
        {
            Init();
        }

        void Update()
        {
            //没到达
            if (!arrive)
            {
                if (findNextPoint)
                {
                    FindNextNode();
                    ChangeAction();
                }
                //如果行为是stop
                //停下
                if (_Action.type == ZActionType.stop)
                {
                    //配置动作是停止
                    //在下面修改了isStop = true
                    //进入执行函数 如果Stop 就计时间到了配置时间就继续走
                    //但是此时的动作还是stop
                    if (isStop)
                    {
                        StartCoroutine(Stoptimer(_Action.time));
                    }
                    else
                    {
                        transform.Translate((_targetVec - transform.position).normalized * speed * Time.deltaTime, transform);
                    }
                }
                else
                {
                    transform.Translate((_targetVec - transform.position).normalized * speed * Time.deltaTime, transform);
                }
                //到终点了 消失
                if (Vector3.Distance(transform.position, _targetVec) < 0.1f && index == points.Length - 1 && findNextPoint)
                {
                    if (loop)
                    {
                        index = 0;
                        _targetVec = points[index];
                    }
                    else
                    {
                        findNextPoint = false;
                        OnPathComplete();
                    }
                }
                //朝向
                //会报错 如果Look rotation viewing vector is zero不知道为啥
                body.LookAt(_targetVec);
                //body.DOLookAt(_targetVec, 1f);
            }
        }
        //找下一个点及对应的表现
        private void FindNextNode()
        {
            if (Vector3.Distance(transform.position, _targetVec) < 0.1f)
            {
                index++;
                if (index >= points.Length - 1)
                {
                    index = points.Length - 1;
                }
                _targetVec = points[index];
                _Action = MyAction[index - 1];
            }
        }
        //表现
        void ChangeAction()
        {
            //如果当前目标点有变化
            if (_Action.type != _last.type)
            {
                _last = _Action;
                //如果是移动
                if (_Action.type == ZActionType.move)
                {
                    OnActionMove();
                }
                else if (_Action.type == ZActionType.stop)
                {
                    isStop = true;
                    OnActionStop();
                }
            }
        }
        //停止计时器
        IEnumerator Stoptimer(float t)
        {
            yield return new WaitForSeconds(t);
            isStop = false;
        }
        int shu;
        //实现的接口
        public void OnPathStart()
        {

        }

        public void OnPathComplete()
        {

        }

        public void OnActionMove()
        {

        }

        public void OnActionStop()
        {

        }
    }
}

