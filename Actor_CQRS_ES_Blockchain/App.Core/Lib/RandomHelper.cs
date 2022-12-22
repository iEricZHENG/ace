using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Lib
{
    public class RandomHelper
    {
        static System.Random numRandom = new System.Random();
        /// <summary>
        /// 生成随机数字
        /// </summary>
        /// <param name="Length">生成长度</param>
        /// <param name="Sleep">是否要在生成前将当前线程阻止以避免重复</param>
        /// <returns></returns>
        public static int CreateNum(int Length)
        {
            return numRandom.Next(Convert.ToInt32(Math.Pow(10, Length)));
        }
        static char[] Pattern = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'm', 'n', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '2', '3', '4', '5', '6', '7', '8', '9' }; //, 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        /// <summary>
        /// 生成随机字母与数字
        /// </summary>
        /// <param name="Length">生成长度</param>
        /// <param name="Sleep">是否要在生成前将当前线程阻止以避免重复</param>
        /// <returns></returns>
        public static string CreateString(int Length)
        {
            string result = string.Empty;
            int n = Pattern.Length;
            System.Random random = new System.Random(~unchecked((int)System.DateTime.UtcNow.Ticks));
            for (int i = 0; i < Length; i++)
            {
                int rnd = random.Next(0, n);
                result += Pattern[rnd];
            }
            return result;
        }
        //定义一个字符串数组储存汉字编码的组成元素 
        static string[] rBase = new String[16] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f" };
        /// <summary>
        /// 中文
        /// </summary>
        /// <param name="strlength"></param>
        /// <returns></returns>
        public static string CreateChinaCode(int strlength)
        {
            System.Random rand = new System.Random(unchecked((int)System.DateTime.UtcNow.Ticks));

            //定义一个object数组用来 
            List<byte[]> result = new List<byte[]>(strlength);

            /**/
            /* 每循环一次产生一个含两个元素的十六进制字节数组，并将其放入bject数组中 
               每个汉字有四个区位码组成 
               区位码第1位和区位码第2位作为字节数组第一个元素 
               区位码第3位和区位码第4位作为字节数组第二个元素 
            */
            for (int i = 0; i < strlength; i++)
            {
                //区位码第1位 
                int r1 = rand.Next(11, 14);
                string str_r1 = rBase[r1].Trim();

                //区位码第2位 
                int r2;
                if (r1 == 13)
                    r2 = rand.Next(0, 7);
                else
                    r2 = rand.Next(0, 16);
                string str_r2 = rBase[r2].Trim();

                //区位码第3位 
                int r3 = rand.Next(10, 16);
                string str_r3 = rBase[r3].Trim();

                //区位码第4位 
                int r4;
                switch (r3)
                {
                    case 10: r4 = rand.Next(1, 16); break;
                    case 15: r4 = rand.Next(0, 15); break;
                    default: r4 = rand.Next(0, 16); break;
                }
                string str_r4 = rBase[r4].Trim();

                //定义两个字节变量存储产生的随机汉字区位码 
                byte byte1 = Convert.ToByte(str_r1 + str_r2, 16);
                byte byte2 = Convert.ToByte(str_r3 + str_r4, 16);
                //将两个字节变量存储在字节数组中 
                byte[] str_r = new byte[] { byte1, byte2 };
                //将产生的一个汉字的字节数组放入object数组中 
                if (!result.Contains(str_r))
                    result.Add(str_r);
            }

            //获取GB2312编码页（表） 
            Encoding gb = Encoding.GetEncoding("gb2312");

            //根据汉字编码的字节数组解码出中文汉字 
            string strResult = string.Empty;
            for (int i = 0; i < strlength; i++)
            {
                strResult += gb.GetString(result[i]);
            }
            return strResult;
        }
    }
}
