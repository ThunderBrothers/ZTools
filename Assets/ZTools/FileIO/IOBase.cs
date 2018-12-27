using System.IO;
using System.Text;

namespace ZTools.FileIO
{
    /// <summary>
    /// IO基础方法
    /// </summary>
    public class IOBase 
    {
        #region File读取
        /// <summary>
        /// 指定路径文件String覆盖写入
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="content">写入内容</param>
        public static void SaveFileByString(string path,string content)
        {
            if (!File.Exists(path))
            {
                FileStream fs = File.Create(path);
                fs.Close();
                fs.Dispose();
            }
            File.WriteAllText(path, content);
        }
        /// <summary>
        /// 指定路径文件读取返回String
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>返回文件的字符串</returns>
        public static string LoadFileForString(string path)
        {
            if (!File.Exists(path))
                return null;
            return File.ReadAllText(path);
        }
        #endregion


        #region FileStream读取
        /// <summary>
        /// 指定路径文件String覆盖写入
        /// 现阶段只对String处理
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="content">写入内容</param>
        public static void WriteByFileStream(string path,string content)
        {
            //创建(存在先删除在创建，不存在直接创建)
            FileStream fs = new FileStream(path, FileMode.Create);
            //转换 编码器Encoding
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            //按照字节写入
            fs.Write(bytes, 0, bytes.Length);
            //清空缓冲区
            fs.Flush();
            //关闭流
            fs.Close();
            //释放流所占用的资源
            fs.Dispose();
        }
        /// <summary>
        /// 指定路径文件String读取
        /// 现阶段只对String处理
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="content">写入内容</param>
        public static string ReadByFileStream(string path)
        {
            string str = "";
            byte[] bytes;
            char[] chars;
            //创建(存在先删除在创建，不存在直接创建)
            FileStream fs = new FileStream(path, FileMode.Open);
            long shu = fs.Length;
            bytes = new byte[shu];
            chars = new char[shu];
            fs.Read(bytes, 0, (int)shu);
            str = Encoding.UTF8.GetString(bytes,0,(int)shu);
            fs.Close();
            fs.Dispose();
            return str;
        }
        #endregion
    }
}


