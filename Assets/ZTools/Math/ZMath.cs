using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ZTools.ZMath
{
    //ZMath
    public class ZMath
    {
        /// <summary>
        /// 从给列表里随机n个不同的元素
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="ChoiceLsit">待选列表</param>
        /// <param name="n">个数</param>
        /// <returns>返回List</returns>
        public static List<T> GetRandom<T>(List<T> ChoiceLsit, int n)
        {
            if (ChoiceLsit == null)
            {
                return null;
            }
            if (n > ChoiceLsit.Count)
            {
                n = ChoiceLsit.Count;
            }
            List<int> temp = new List<int>();
            for (int i = 0; i < n;)
            {
                //获取随机角标
                int v = UnityEngine.Random.Range(0, ChoiceLsit.Count);
                // 种子数够用 或者种子没有用完
                if (ChoiceLsit.Count >= n || temp.Count < ChoiceLsit.Count)
                {
                    //不重复的角标
                    if (temp.IndexOf(v) < 0)
                    {
                        temp.Add(v);
                        i++;
                    }
                }
                else
                {
                    i++;
                }
            }
            List<T> tempT = new List<T>();
            for (int i = 0; i < temp.Count; i++)
            {
                tempT.Add(ChoiceLsit[temp[i]]);
            }
            return tempT;
        }


        /// <summary>
        /// 从给定数组里随机n个不同的元素
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="ChoiceLsit">待选列表</param>
        /// <param name="n">个数</param>
        /// <returns>返回List</returns>
        public static List<T> GetRandom<T>(T[] ChoiceLsit, int n)
        {
            if (ChoiceLsit == null)
            {
                return null;
            }
            if (n > ChoiceLsit.Length)
            {
                n = ChoiceLsit.Length;
            }
            List<int> temp = new List<int>();
            for (int i = 0; i < n;)
            {
                //获取随机角标
                int v = UnityEngine.Random.Range(0, ChoiceLsit.Length);
                // 种子数够用 或者种子没有用完
                if (ChoiceLsit.Length >= n || temp.Count < ChoiceLsit.Length)
                {
                    //不重复的角标
                    if (temp.IndexOf(v) < 0)
                    {
                        temp.Add(v);
                        i++;
                    }
                }
                else
                {
                    i++;
                }
            }
            List<T> tempT = new List<T>();
            for (int i = 0; i < temp.Count; i++)
            {
                tempT.Add(ChoiceLsit[temp[i]]);
            }
            return tempT;
        }
    }
    #region 比较器模板 List.Sort排序模板 IComparer比较器
    //比较器模板 
    //按照给定List根据元素GameObject的大小排序  从小到大
    public class ZComparerTemplate : IComparer<GameObject>
    {
        public int Compare(GameObject x, GameObject y)
        {
            return x.transform.localScale.x.CompareTo(y.transform.localScale.x);
        }
    }
    //从大到小
    public class ZComparerTemplate1 : IComparer<GameObject>
    {
        public int Compare(GameObject x, GameObject y)
        {
            return y.transform.localScale.x.CompareTo(x.transform.localScale.x);
        }
    }
    #endregion

    #region UserState两种List.Sort排序模板 IComparer比较器和IComparable类内定义比较
    /// <summary>
    /// 对自定义类型比较
    /// IComparer操作两个参数的比较排序Compare
    /// </summary>
    public class ZUserStateComparable : IComparer<UserState> {
        public int Compare(UserState UserX, UserState UserY) {
            if (UserY.editing)
                return 1;
            else if (UserX.editing && !UserY.editing)
                return -1;
            else
                return 1;
        }
    }
    /// <summary>
    /// 自定义类型
    /// IComparable操作一个参数的比较排序CompareTo
    /// </summary>
    public class UserState : IComparable {
        public string userID;
        public bool editing;
        public string userName;

        public UserState(string ID, bool isediting) {
            userID = ID;
            editing = isediting;
        }
        /// <summary>
        /// 说明 按照User.edition值优先排序
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj) {
            UserState UserY = obj as UserState;
            //UserY在操作上移一位
            if (UserY.editing)
                return 1;
            //UserX在editing并且UserY不在操editing，UserY下移一位
            //前者改为在editing后顶替后者
            else if (editing && !UserY.editing)
                return -1;
            //多种情况 1.UserX和UserY同时不在在editing
            //         2.UserX不在editing和UserY在editing
            //         3.UserX在editing和UserY时在editing     
            else
                return 1;
        }
    }
    #endregion
}

