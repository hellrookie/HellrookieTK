using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hellrookie.ToolKit
{
    /// <summary>
    /// 控制台输出进度条
    /// </summary>
    public class ProgressPrinter
    {
        private ConsoleColor colorBack;
        private ConsoleColor colorFore;
        private int currentProgress;
        private int curesorTop;
        private int curesorLeft;

        /// <summary>
        /// 构造ProgressPrinter实例
        /// </summary>
        public ProgressPrinter()
        {
            ProgressInitial();
        }

        private void ProgressInitial()
        {
            colorBack = Console.BackgroundColor;
            colorFore = Console.ForegroundColor;
            Console.WriteLine("Backup now working...");
            Console.BackgroundColor = ConsoleColor.Green;
            for (int i = 0; i < 25; ++i)
            {
                Console.Write(" ");
            }
            Console.WriteLine("");
            Console.BackgroundColor = colorBack;
            Console.WriteLine("0%");

            curesorTop = Console.CursorTop;
            curesorLeft = Console.CursorLeft;
            currentProgress = 0;
        }

        /// <summary>
        /// 打印当前的进度
        /// </summary>
        /// <param name="progressValue">进度值</param>
        public void Progress(int progressValue)
        {
            if (currentProgress > 100)
            {
                currentProgress = 100;
            }
            if (currentProgress >= progressValue)
            {
                return;
            }
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.SetCursorPosition(currentProgress / 4, curesorTop - 2);
            for (int i = 0; i < progressValue / 4 - currentProgress / 4; ++i)
            {
                Console.Write(" ");
            }
            currentProgress = progressValue;
            Console.BackgroundColor = colorBack;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(0, curesorTop - 1);
            Console.Write("{0}%", currentProgress);
            Console.ForegroundColor = colorFore;
            if (currentProgress == 100)
            {
                Console.SetCursorPosition(0, curesorTop);
                Console.WriteLine("finished.");
            }
        }
    }
}
