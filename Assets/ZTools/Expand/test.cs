using UnityEngine;

namespace ZTools.Expand
{
    public class test : MonoBehaviour
    {
        public string shu;
        // Use this for initialization
        void Start()
        {
            gameObject.ZLog("ok");
        }
        [ContextMenu("go")]
        private void Go()
        {
            Debug.Log(NumberToChinese2(shu));
        }

        /// <summary>
        /// 12 十二
        /// 102 一百零二
        /// 2344 两千三百四十四
        /// 5002 五千零二
        /// 6010 六千零一十
        /// </summary>
        /// <param name="numberStr"></param>
        /// <returns></returns>
        public string NumberToChinese(string numberStr)
        {
            string numStr = "0123456789";
            //string chineseStr = "零一二三四五六七八九";
            //string unit = "十百千万十百千亿";

            string[] chineseStr = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
            string[] unit = { "十", "百", "千", "万", "十", "百", "千", "亿", "十", "百", "千" };
            string result = "";
            for (int i = 0;i < numberStr.Length;i++)
            {
                int index = numStr.IndexOf(numberStr[i]);
                if (i != numberStr.Length -1 && index != 0)
                {
                    result += chineseStr[index] + unit[numberStr.Length - 2 - i];
                }else
                {
                    result += chineseStr[index];
                }   
            }
            for (int i = 0;i < result.Length;i++)
            {

            }
            numStr = null;
            chineseStr = null;
            return result;
        }

        public string NumberToChinese2(string numberStr)
        {
            string numStr = "0123456789";
            string chineseStr = "零一二三四五六七八九";
            char[] c = numberStr.ToCharArray();
            for (int i = 0;i < c.Length;i++)
            {
                int index = numStr.IndexOf(c[i]);
                if (index != -1)
                    c[i] = chineseStr.ToCharArray()[index];
            }
            string temp1 = c.ToString();
            string temp2 = new char[2] { '一', '零' }.ToString();
            if (new string(c) == new string(new char[2] { '一', '零' }))
            {
                c = new char[] { '十' };
            }
            if (c.Length == 2)
            {
                if (c[0] == '一')
                {
                    c = new char[2] { '十', c[1] };
                }else
                {
                    c = new char[3] { c[0], '十', c[1] };
                }   
            }
            numStr = null;
            chineseStr = null;
            return new string(c);
        }
    }
}

