using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hellrookie.ToolKit
{
    /// <summary>
    /// 存在于一个AppDomain里面的Logger，方便给每次执行程序提供统一的Logger类。
    /// </summary>
    public class AppDomainLogger : ILogger, IDisposable
    {
        private static Logger internalLogger;
        private static AppDomainLogger logger;

        /// <summary>
        /// 否是要输出到控制台
        /// </summary>
        public bool WriteToConsole
        {
            get
            {
                return internalLogger.WriteToConsole;
            }

            set
            {
                internalLogger.WriteToConsole = value;
            }
        }

        private AppDomainLogger(string name)
        {
            internalLogger = new Logger(name);
        }

        /// <summary>
        /// 获取logger实例
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static AppDomainLogger CreateLogger(string name)
        {
            if (logger == null)
            {
                logger = new AppDomainLogger(name);
            }
            return logger;
        }

        /// <summary>
        /// 输出information
        /// </summary>
        /// <param name="msg"></param>
        public void Info(string msg)
        {
            internalLogger.Info(msg);
        }

        /// <summary>
        /// 输出information
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Info(string format, params object[] args)
        {
            internalLogger.Info(format, args);
        }

        /// <summary>
        /// 输出warning
        /// </summary>
        /// <param name="msg"></param>
        public void Warn(string msg)
        {
            internalLogger.Warn(msg);
        }

        /// <summary>
        /// 输出warning
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Warn(string format, params object[] args)
        {
            internalLogger.Warn(format, args);
        }

        /// <summary>
        /// 输出error
        /// </summary>
        /// <param name="msg"></param>
        public void Error(string msg)
        {
            internalLogger.Error(msg);
        }

        /// <summary>
        /// 输出error
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Error(string format, params object[] args)
        {
            internalLogger.Error(format, args);
        }

        /// <summary>
        /// 输出debug
        /// </summary>
        /// <param name="msg"></param>
        public void Debug(string msg)
        {
            internalLogger.Debug(msg);
        }

        /// <summary>
        /// 输出debug
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Debug(string format, params object[] args)
        {
            internalLogger.Debug(format, args);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            internalLogger.Dispose();
        }
    }
}
