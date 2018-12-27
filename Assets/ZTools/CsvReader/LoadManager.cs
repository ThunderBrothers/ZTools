using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZTools.CsvReader
{
    public class LoadManager
    {
        public delegate void CallBack();
        /// <summary>
        /// Key 表格地址
        /// Value 表格字符串数据
        /// </summary>
        private static Dictionary<string, string> cache = new Dictionary<string, string>();

        private static int loadIndex = 0;
        /// <summary>
        /// 加载某个地址的表格
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="call">回调函数</param>
        public static void Load(string url, CallBack call = null)
        {
            Load(new string[] { url }, call);
        }
        /// <summary>
        /// 加载多个地址的表格
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="call">回调函数</param>
        public static void Load(string[] urls, CallBack call = null)
        {
            //如果下载地址为空
            if (urls == null || urls.Length <= 0)
            {
                if (call != null)
                    call();
                return;
            }
            List<string> temp = new List<string>();
            string n;
            //遍历所有地址
            for (int i = 0; i < urls.Length; i++)
            {
                //如果地址不为空 或者 地址内容没有被下载过
                if (!string.IsNullOrEmpty(urls[i]) && !cache.ContainsKey(urls[i]))
                {
                    temp.Add(urls[i]);
                    n = File.ReadAllText(urls[i], Encoding.UTF8);
                    cache.Add(urls[i], n);
                }
            }
            //执行回调
            if (call != null)
            {
                call();
            }
            loadIndex++;
        }
        /// <summary>
        /// 得到某个地址的表格的所有数据字符串
        /// </summary>
        /// <param name="url">地址</param>
        /// <returns>字符串</returns>
        public static string GetString(string url)
        {
            if (cache.ContainsKey(url))
            {
                if (cache[url] != null)
                    return cache[url];
            }
            return "";
        }
    }
}

