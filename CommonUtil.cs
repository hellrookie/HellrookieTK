using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hellrookie.ToolKit
{
    public class CommonUtil
    {
        /// <summary>
        /// 控制台输入密码
        /// </summary>
        /// <returns></returns>
        public static string InputPasswordInConsole()
        {
            string password = string.Empty;
            char inputKey;
            do
            {
                inputKey = Console.ReadKey(true).KeyChar;
                if (inputKey == '\b')
                {
                    if (password.Length > 0)
                    {
                        password = password.Substring(0, password.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (inputKey != '\r')
                {
                    password += inputKey;
                    Console.Write("*");
                }
            }
            while (inputKey != '\r');
            return password;
        }
    }
}
