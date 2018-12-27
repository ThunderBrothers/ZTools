using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ZTools.Path
{
	public class ZPath : MonoBehaviour {
		public bool isInitialized = false;
		public string initialName = "";
        public float radius = 0f;
        private static Dictionary<string, ZPath> _cache = new Dictionary<string, ZPath>();
       
		public string pathName;
		public Color pathColor = Color.red;
        public bool IsDebug = false;
        public bool ShowNode = false;
        public List<Vector3> nodes = new List<Vector3>(){Vector3.zero, Vector3.zero};

		/// 重复的路径不会被添加到全局静态缓存中
		/// 但是 如果group引用了 还是可以获得的(不建议使用相同的名字)
		void OnEnable()
		{
			if(!_cache.ContainsKey(pathName))
			{
				_cache.Add(pathName, this);
			}
        }

		void OnDisable()
		{
			_cache.Remove(pathName);
		}

		void OnDrawGizmosSelected()
		{
			for(int i = 1; i < nodes.Count; i++)
			{
				Gizmos.color = pathColor;
				Gizmos.DrawLine(nodes[i-1], nodes[i]);
            }
		}
        private void OnDrawGizmos()
        {
            if (IsDebug)
            {
                for (int i = 1; i < nodes.Count; i++)
                {
                    //路线
                    Color c = new Color(pathColor.r, pathColor.g, pathColor.b, pathColor.a / 2f);
                    Gizmos.color = c;
                    Gizmos.DrawLine(nodes[i - 1], nodes[i]);
                    //节点
                    if (ShowNode)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireSphere(nodes[i], radius);
                    }
                }
            }
        }
        public static Vector3[] GetPath(string name)
		{
			if(_cache.ContainsKey(name))
			{
				return _cache[name].nodes.ToArray();
			}
			return null;
		}
	}
}