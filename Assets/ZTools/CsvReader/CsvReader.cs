using UnityEngine;
using System.Collections.Generic;

namespace ZTools.CsvReader
{
    /// <summary>
    /// 读取字符串，提取信息，返回格式化后的数据
    /// </summary>
    public class CsvReader : CsvBase
    {
        /// <summary>
        /// 某个表的数据
        /// 行数据
        /// Key 表格第一列配置ID
        /// Value 表格数据部分的单元格数据包括ID
        /// </summary>
        private Dictionary<string, string[]> _data = new Dictionary<string, string[]>();
        /// <summary>
        /// 存放读取到的表格第一行
        /// 也就是第一类的具体参数名称
        /// 方便做类型赋值时的对比
        /// </summary>
        private string[] _keys;

        /// <summary>
        /// 解析字符串
        /// 把一个表格的所有字符分割保存在String[]
        /// </summary>
        /// <param name="text">UTF8解析出来的全部字符串</param>
        public void Read(string text)
        {
            text = text.Replace("\r", "");
            string[] arr = text.Split('\n');
            _keys = arr[0].Split(',');
            for (int i = 2; i < arr.Length; i++)
            {
                if (!string.IsNullOrEmpty(arr[i]))
                {
                    //单元格数据
                    string[] temp = arr[i].Split(',');
                    _data.Add(temp[0], temp);
                }
            }
        }
        /// <summary>
        /// 转化数据
        /// 根据传入泛型把数据转化成对应类
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="id">表格配置第一列的ID</param>
        /// <returns>返回对应包含数据的类</returns>
        public T GetData<T>(string id) where T : class, new()
        {
            if (!_data.ContainsKey(id))
            {
                return null;
            }
            T t = new T();
            string[] d = _data[id];
            //给类赋值
            for (int i = 0; i < _keys.Length; i++)
            {
                System.Reflection.FieldInfo info = t.GetType().GetField(_keys[i]);
                if (info == null)
                {
                    Debug.Log(_keys[i]);
                    Debug.Log(typeof(T));
                }
                System.Type type = info.FieldType;
                if (type == typeof(System.Int32))
                {
                    info.SetValue(t, int.Parse(d[i]));
                }
                else if (type == typeof(System.Single))
                {
                    info.SetValue(t, (float)double.Parse(d[i]));
                }
                else if (type == typeof(System.Double))
                {
                    info.SetValue(t, double.Parse(d[i]));
                }
                else if (type == typeof(System.Boolean))
                {
                    info.SetValue(t, d[i] == "0" ? false : true);
                }
                else if (type == typeof(System.String[]))
                {
                    info.SetValue(t, d[i].Split('|'));
                }
                else
                {
                    if (string.IsNullOrEmpty(d[i]) || d[i] == "#")
                    {
                        info.SetValue(t, "");
                    }
                    else
                    {
                        info.SetValue(t, d[i]);
                    }
                }
            }
            return t;
        }
    }
}

