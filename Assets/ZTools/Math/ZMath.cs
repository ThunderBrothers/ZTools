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
}

