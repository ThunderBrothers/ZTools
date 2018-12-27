using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace ZTools.CsvReader
{
    public class CsvManager
    {
        public delegate void CallBack();
        private static CallBack onFinish;
        /// <summary>
        /// 非配置csv表数据
        /// Key 表格名称.csv
        /// Value 表格地址 + 表格名称.csv
        /// </summary>
        private static Dictionary<string, string> _csvFileUrls = new Dictionary<string, string>();
        /// <summary>
        /// 读取过的数据
        /// csv表缓存
        /// Key 一串有含义的字符串   public static TypeARCsvLevel GetLevel(string id)" + id;
        /// Value 一个类  CsvBase数据结构
        /// </summary>
        private static Dictionary<string, CsvBase> _cache = new Dictionary<string, CsvBase>();

        /// <summary>
        /// 保存的所有表格
        /// 真正数据存储位置
        /// Key 表格名称.csv
        /// Value 一个类 CsvReader
        /// </summary>
        private static Dictionary<string, CsvReader> _reader = new Dictionary<string, CsvReader>();

        private static bool _ini = false;
        private static string pathCsv = "";
        private static string pathRoot = "";

        /// <summary>
        /// 初始读取配置
        /// </summary>
        /// <param name="root">所有CSV表格根目录</param>
        /// <param name="path">配置表格名称</param>
        /// <param name="callback">设置后回调函数</param>
        public static void Ini(string root, string path, CallBack callback = null)
        {
            if (_ini == false)
            {
                _ini = true;
                onFinish = callback;
                pathCsv = root + path;
                pathRoot = root;
                //加载配置表格数据
                LoadManager.Load(pathCsv, LoadCsvConfig);
            }
        }
        /// <summary>
        /// 先把配置的表格数据读取 
        /// 拿到读取的所有表名给_csvFileUrls赋值
        /// LoadManager.Load(list.ToArray(), LoadCsvs)读取全部表格
        /// </summary>
        public static void LoadCsvConfig()
        {
            //获取加载的配置表格的数据
            string text = LoadManager.GetString(pathCsv);
            text = text.Replace("\r", "");
            //此时数据格式
            //arr[i]配置表格的一行数据
            string[] arr = text.Split('\n');
            //解析数据
            List<string> list = new List<string>();
            //去掉读取到的表头
            for (int i = 1; i < arr.Length; i++)
            {
                if (!string.IsNullOrEmpty(arr[i]))
                {
                    //此时数据格式
                    //temp[i]配置表格的某一行每个单元格数据
                    string[] temp = arr[i].Split(',');
                    if (temp != null && temp.Length >= 1 && !string.IsNullOrEmpty(temp[1]))
                    {
                        //路径 + 表名
                        string url = pathRoot + temp[1];
                        if (_csvFileUrls.ContainsKey(temp[1]))
                        {
                            Debug.Log(arr[i]);
                            Debug.Log(temp[1]);
                        }
                        //去掉读取到的ID值
                        //所有表路径赋值
                        _csvFileUrls.Add(temp[1], url);
                        //准备其他表的读取
                        list.Add(url);
                    }
                }
            }
            //加载非配置表格数据
            LoadManager.Load(list.ToArray(), LoadCsvs);
        }
        /// <summary>
        /// 读取所有表格
        /// 根据表的配置信息读取相应的表格
        /// _reader保存所有表读取出来的CsvReader信息
        /// </summary>
        private static void LoadCsvs()
        {
            //便利所有表地址
            foreach (string k in _csvFileUrls.Keys)
            {
                CsvReader reader = new CsvReader();
                //地址
                string txt = LoadManager.GetString(_csvFileUrls[k]);
                //解析
                txt = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(txt));
                //二次解析
                reader.Read(txt);
                //保持引用
                _reader.Add(k, reader);
            }
            //完成CallBack
            if (onFinish != null)
            {
                onFinish();
            }
        }
        /// <summary>
        /// 读取TypeARCsvLevel表的第id行
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static TypeARCsvLevel GetLevel(string id)
        {
            //创建一个唯一的 有意义的key值
            string key = "public static TypeARCsvLevel GetLevel(string id)" + id;
            //判断当前的key值 所对应的数据是不是已经被缓存过了
            if (_cache.ContainsKey(key))
            {
                return (TypeARCsvLevel)_cache[key];
            }
            //读表 参数表的 全名
            CsvReader reader = _reader["level.csv"];
            //解析表  参数是  表中 第一列字段对应的 编号
            TypeARCsvLevel csv = reader.GetData<TypeARCsvLevel>(id);
            if (csv == null)
            {
                _cache.Add(key, null);
            }
            else
            {
                _cache.Add(key, (CsvBase)csv);
            }
            return csv;
        }
        public static TypeARGoldConfig GetGold(string id)
        {
            string key = "public static TypeARGoldConfig GetGold(string id)" + id;
            if (_cache.ContainsKey(key))
            {
                return (TypeARGoldConfig)_cache[key];
            }
            CsvReader reader = _reader["goldConfig.csv"];
            TypeARGoldConfig csv = reader.GetData<TypeARGoldConfig>(id);
            if (csv == null)
            {
                _cache.Add(key, null);
            }
            else
            {
                _cache.Add(key, (CsvBase)csv);
            }
            return csv;
        }
        public static TypeARCsvConfig GetARConfig(string id)
        {
            string key = "public static TypeARCsvConfig GetARConfig(int id)" + id;
            if (_cache.ContainsKey(key))
            {
                return (TypeARCsvConfig)_cache[key];
            }
            CsvReader reader = _reader["config.csv"];
            TypeARCsvConfig csv = reader.GetData<TypeARCsvConfig>(id);
            if (csv == null)
            {
                _cache.Add(key, null);
            }
            else
            {
                _cache.Add(key, (CsvBase)csv);
            }
            return csv;
        }
        public static List<TypeARGoldConfig> GetGoldList()
        {
            List<TypeARGoldConfig> temp = new List<TypeARGoldConfig>();
            int index = 1;
            TypeARGoldConfig csv = GetGold(index.ToString());
            while (csv != null)
            {
                temp.Add(csv);
                index++;
                csv = GetGold(index.ToString());
            }
            return temp;
        }
    }
}

