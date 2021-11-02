using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Expand
{
    /// <summary>
    /// 获取所有子物体下的第一个T组件，以防自带的超过3层或者未激活状态查找不到
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static List<T> GetComponentFormChild<T>(this Transform transform, bool allowSubclass = false)
    {
        List<T> components = new List<T>();
        T temp = transform.GetComponent<T>();
        if (temp!=null)
        {
            if (temp.ToString().Equals("null"))
            {
                temp = default;
            }
        }
        
        if (temp != null && TypeFilter(temp.GetType(), typeof(T), allowSubclass))
        {
            components.Add(temp);
        }

        if (transform.childCount > 0)
        {
            Transform child;
            for (int i = 0; i < transform.childCount; i++)
            {
                child = transform.GetChild(i);
                List<T> @var = GetComponentFormChild<T>(child, allowSubclass);
                if (@var != null && @var.Count > 0)
                {
                    components.AddRange(@var);
                }
            }
        }
        return components;
    }

    /// <summary>
    /// 获取所有子物体下的所有T组件，以防自带的超过3层或者未激活状态查找不到
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static List<T> GetComponentsFormChild<T>(this Transform transform, bool allowSubclass = false)
    {
        List<T> components = new List<T>();
        T[] temp = transform.GetComponents<T>();
        if (temp != null)
        {
            if (temp.ToString().Equals("null"))
            {
                temp = default;
            }
        }

        if (temp != null)
        {
            for (int i=0;i< temp.Length;i++)
            {
                if (TypeFilter(temp[i].GetType(), typeof(T), allowSubclass))
                {
                    components.Add(temp[i]);
                }
            }
        }

        if (transform.childCount > 0)
        {
            Transform child;
            for (int i = 0; i < transform.childCount; i++)
            {
                child = transform.GetChild(i);
                List<T> @var = GetComponentsFormChild<T>(child, allowSubclass);
                if (@var != null && @var.Count > 0)
                {
                    components.AddRange(@var);
                }
            }
        }
        return components;
    }


    private static bool TypeFilter(Type curType, Type target, bool allowSubclass)
    {
        bool equals = false;
        if (allowSubclass)
        {
            equals = curType.IsSubclassOf(target);
        }
        else
        {
            equals = (curType == target);
        }
        return equals;
    }
}
