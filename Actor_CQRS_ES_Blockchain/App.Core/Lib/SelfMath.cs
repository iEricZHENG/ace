using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Lib
{
    public static class SelfMath
    {
        public static int GetDigitalLength(decimal num)
        {
            var array = num.ToString().Split('.');
            if (array.Length == 2)
            {
                return array[1].TrimEnd().TrimEnd('0').Length;
            }
            return 0;
        }
        /// <summary>
        /// 将小数值按指定的小数位数截断
        /// </summary>
        /// <param name="d">要截断的小数</param>
        /// <param name="s">小数位数，s大于等于0，小于等于28</param>
        /// <returns></returns>
        public static decimal ToFixed(decimal d, int s)
        {
            decimal sp = Convert.ToDecimal(Math.Pow(10, s));
            var truncate = Math.Truncate(d);
            if (d < 0)
                return truncate + Math.Ceiling((d - truncate) * sp) / sp;
            else
                return truncate + Math.Floor((d - truncate) * sp) / sp;
        }
    }
}
