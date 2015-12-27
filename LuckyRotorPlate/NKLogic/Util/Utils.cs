using System;
using System.Windows;
using System.Text;

namespace NKLogic.Util
{
    public static class Utils
    {

        /// <summary>
        /// 计算根据角度与半径旋转后的点坐标
        /// </summary>
        /// <param name="angle">角度</param>
        /// <param name="radius">半径</param>
        /// <returns>点坐标</returns>
        public static Point ComputeCartesianCoordinate(double angle, double radius)
        {
            // 转化半径值
            double angleRad = (Math.PI / 180.0) * (angle - 90);

            double x = radius * Math.Cos(angleRad);
            double y = radius * Math.Sin(angleRad);

            return new Point(x, y);
        }
        
        /// <summary>
        /// 获取字符串长度(区分中英文,一个汉字算两个字节)
        /// </summary>
        /// <param name="str">待获取的字符串</param>
        /// <returns>字符串字节长度</returns>
        public static int GetLength(string str)
        {
            int len = 0;
            for (int i = 0; i < str.Length; i++)
            {
                byte[] byte_len = Encoding.Default.GetBytes(str.Substring(i, 1));
                if (byte_len.Length > 1)
                    len += 2; //如果长度大于1，是中文，占两个字节，+2
                else
                    len += 1;  //如果长度等于1，是英文，占一个字节，+1
            }
            return len;
        }   
    }
}
