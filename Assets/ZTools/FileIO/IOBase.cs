using System.IO;
using System.Text;

namespace ZTools.FileIO
{
    /// <summary>
    /// IO基础方法
    /// </summary>
    public class IOBase 
    {
        //*****************************************本地文件********************************************************
        //----------------------------------------字符串类文件读写------------------------------------------
        #region File读写
        /// <summary>
        /// 指定路径文件读取返回String
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>返回文件的字符串</returns>
        public static string ReadFileText(string path)
        {
            if (!File.Exists(path))
                return null;
            return File.ReadAllText(path);
        }
        /// <summary>
        /// 指定路径文件String覆盖写入
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="content">写入内容</param>
        public static void WriteFileText(string path,string content)
        {
            if (!File.Exists(path))
            {
                FileStream fs = File.Create(path);
                fs.Close();
                fs.Dispose();
            }
            File.WriteAllText(path, content);
        }
        #endregion


        #region FileStream读写
        /// <summary>
        /// 指定路径文件String读取
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="content">写入内容</param>
        public static string ReadByFileStream(string path)
        {
            string str = "";
            byte[] bytes;
            //创建(存在先删除在创建，不存在直接创建)
            FileStream fs = new FileStream(path, FileMode.Open);
            long shu = fs.Length;
            bytes = new byte[shu];
            fs.Read(bytes, 0, (int)shu);
            str = Encoding.UTF8.GetString(bytes, 0, (int)shu);
            fs.Close();
            fs.Dispose();
            return str;
        }
        /// <summary>
        /// 指定路径文件String覆盖写入
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
        #endregion
        //----------------------------------------特殊格式类文件读写------------------------------------------

        #region byte[]读写
        /// <summary>
        /// 指定路径文件byte[]覆盖写入
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <param name="count"></param>
        public static byte[] ReadByBytes(string path)
        {
            byte[] content = null;
            FileStream fs = new FileStream(path, FileMode.Open);
            long shu = fs.Length;
            fs.Read(content, 0, (int)shu);
            return content;
        }
        /// <summary>
        /// 指定路径文件byte[]覆盖写入
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <param name="count"></param>
        public static void WriteByBytes(string path, byte[] content, int count)
        {
            //创建(存在先删除在创建，不存在直接创建)
            FileStream fs = new FileStream(path, FileMode.Create);
            fs.Write(content, 0, count);
            fs.Flush();
            fs.Close();
            fs.Dispose();
        }
        #endregion

        //*****************************************网络文件(异步)********************************************************

    }
}


