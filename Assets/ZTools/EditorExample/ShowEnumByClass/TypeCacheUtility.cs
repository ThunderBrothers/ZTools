using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace ZTools.Expand
{
    public class TypeCacheUtility
    {
        /// <summary>
        /// Type缓存
        /// </summary>
        private static Dictionary<Type, List<Type>> cache = new Dictionary<Type, List<Type>>();

        /// <summary>
        /// 获取T的所有子类的Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Type> GetSubClasses<T>()
        {
            return GetSubClasses(typeof(T));
        }

        public static List<Type> GetSubClasses(Type baseClassType)
        {
            if (baseClassType == null)
            {
                return null;
            }
            if (!cache.ContainsKey(baseClassType))
            {
                cache[baseClassType] = baseClassType.GetAllSubClassesOf();
            }
            return cache[baseClassType];
        }
    }

    /// <summary>
    /// Type的拓展方法
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// 获取所有子类的type
        /// </summary>
        /// <param name="rootType"></param>
        /// <param name="searchAssemblies"></param>
        /// <returns></returns>
        public static List<Type> GetAllSubClassesOf(this Type rootType, Assembly[] searchAssemblies = null)
        {
            if (!rootType.IsClass) return null;

            if (searchAssemblies == null) { searchAssemblies = AppDomain.CurrentDomain.GetAssemblies(); }

            var results = new List<Type>();

            Parallel.ForEach(searchAssemblies, (assembly) =>
            {
                Parallel.ForEach(assembly.GetTypes(), (type) =>
                {
                    if (type != null && type.IsClass && !type.IsAbstract && type.IsSubclassOf(rootType))
                    {
                        results.Add(type);
                    }
                });
            });
            return results;
        }
    }
}
