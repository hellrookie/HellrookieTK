using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Hellrookie.ToolKit
{
    /// <summary>
    /// 高精度计时器，tick级别。
    /// </summary>
    public class PerformanceTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(
            out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(
            out long lpFrequency);

        private long startTime, stopTime;
        private long freq;

        /// <summary>
        /// 构造PerformanceTimer实例
        /// </summary>
        public PerformanceTimer()
        {
            startTime = 0;
            stopTime = 0;

            if (QueryPerformanceFrequency(out freq) == false)
            {
                // 不支持高性能计数器
                throw new Exception();
            }
        }

        /// <summary>
        /// 开始计时器
        /// </summary>
        public void Start()
        {
            // 来让等待线程工作
            Thread.Sleep(0);

            QueryPerformanceCounter(out startTime);
        }

        /// <summary>
        /// 停止计时器
        /// </summary>
        public void Stop()
        {
            QueryPerformanceCounter(out stopTime);
        }

        /// <summary>
        /// 返回计时器经过时间(单位：秒)
        /// </summary>
        public double Duration
        {
            get
            {
                return (double)(stopTime - startTime) / (double)freq;
            }
        }
    }
}
