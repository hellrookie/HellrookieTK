using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Hellrookie.ToolKit
{
    /// <summary>
    /// logger接口
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 否是要输出到控制台
        /// </summary>
        bool WriteToConsole { get; set; }
        /// <summary>
        /// information
        /// </summary>
        /// <param name="msg"></param>
        void Info(string msg);
        /// <summary>
        /// information
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void Info(string format, params object[] args);
        /// <summary>
        /// warn
        /// </summary>
        /// <param name="msg"></param>
        void Warn(string msg);
        /// <summary>
        /// warn
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void Warn(string format, params object[] args);
        /// <summary>
        /// error
        /// </summary>
        /// <param name="msg"></param>
        void Error(string msg);
        /// <summary>
        /// error
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void Error(string format, params object[] args);
        /// <summary>
        /// debug
        /// </summary>
        /// <param name="msg"></param>
        void Debug(string msg);
        /// <summary>
        /// debug
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        void Debug(string format, params object[] args);
    }

    /// <summary>
    /// log的类型
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// information
        /// </summary>
        Info,
        /// <summary>
        /// warning
        /// </summary>
        Warn,
        /// <summary>
        /// error
        /// </summary>
        Error,
        /// <summary>
        /// debug
        /// </summary>
        Debug,
    }

    /// <summary>
    /// 书写log。
    /// 自带内存缓存，可以使用FlushTime控制写文件的时间。
    /// WriteToConsole控制是否同时在控制台打印。
    /// 支持Info，Warn，Error和Debug四种log。
    /// </summary>
    public class Logger : ILogger, IDisposable
    {
        private MemoryStream stream;
        private string name;
        private bool closing = false;
        private object syncObj = new object();
        /// <summary>
        /// 缓存写文件的时间
        /// </summary>
        public int FlushTime { get; set; }
        /// <summary>
        /// 否是要输出到控制台
        /// </summary>
        public bool WriteToConsole { get; set; }

        /// <summary>
        /// 在执行文件的目录创建Logs文件夹，在该文件夹中创建新的log文件写log。
        /// </summary>
        /// <param name="name">文件的名称，不包括后缀</param>
        public Logger(string name)
        {
            var parentPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName, "Logs");

            if (!Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }
            lock (syncObj)
            {
                // 防止在多线程中创建重复的名称
                do
                {
                    Thread.Sleep(1000);
                    this.name = string.Format("{0}\\{1}_{2}.log", parentPath, name, DateTime.Now.ToString("yyyyMMddHHmmss"));
                }
                while (File.Exists(this.name));
            }
            if(!string.IsNullOrEmpty(this.name))
            {
                stream = new MemoryStream();
                Thread th = new Thread(WriteToFile);
                th.IsBackground = true;
                th.Start();
            }
        }

        /// <summary>
        /// 获取log文件的名称
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        #region 把msg写到缓存中
        /// <summary>
        /// 输出information
        /// </summary>
        /// <param name="msg"></param>
        public void Info(string msg)
        {
            Info(msg, new object[]{});
        }
        /// <summary>
        /// 输出information
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Info(string format, params object[] args)
        {
            MessageProcess(LogType.Info, format, args);
        }
        /// <summary>
        /// 输出warning
        /// </summary>
        /// <param name="msg"></param>
        public void Warn(string msg)
        {
            Warn(msg, new object[] { });
        }
        /// <summary>
        /// 输出warning
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Warn(string format, params object[] args)
        {
            MessageProcess(LogType.Warn, format, args);
        }
        /// <summary>
        /// 输出error
        /// </summary>
        /// <param name="msg"></param>
        public void Error(string msg)
        {
            Error(msg, new object[] { });
        }
        /// <summary>
        /// 输出error
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Error(string format, params object[] args)
        {
            MessageProcess(LogType.Error, format, args);
        }
        /// <summary>
        /// 输出debug
        /// </summary>
        /// <param name="msg"></param>
        public void Debug(string msg)
        {
#if DEBUG
            Debug(msg, new object[] { });
#endif
        }
        /// <summary>
        /// 输出debug
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Debug(string format, params object[] args)
        {
#if DEBUG
            MessageProcess(LogType.Debug, format, args);
#endif
        }
        private void MessageProcess(LogType logType, string format, params object[] args)
        {
            var msg = string.Format(format, args);
            if(WriteToConsole)
            {
                Console.WriteLine(msg);
            }
            string writenMsg = string.Empty;
            switch(logType)
            {
                case LogType.Info:
                    {
                        writenMsg = string.Format("[INFO {0}] {1}\r\n", DateTime.Now.ToString(), msg);
                        break;
                    }
                case LogType.Warn:
                    {
                        writenMsg = string.Format("[WARN {0}] {1}\r\n", DateTime.Now.ToString(), msg);
                        break;
                    }
                case LogType.Error:
                    {
                        writenMsg = string.Format("[ERROR {0}] {1}\r\n", DateTime.Now.ToString(), msg);
                        break;
                    }
                case LogType.Debug:
                    {
#if DEBUG
                        writenMsg = string.Format("[DEBUG {0}] {1}\r\n", DateTime.Now.ToString(), msg);
#endif
                        break;
                    }
            }
            if (stream != null)
            {
                WriteToMemory(writenMsg);
            }
            else
            {
                Console.WriteLine("Cannot write log. Stream create failed.");
            }
        }
        private void WriteToMemory(string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            lock (syncObj)
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
        #endregion

        private void WriteToFile()
        {
            while (!closing)
            {
                Thread.Sleep(FlushTime);
                lock (syncObj)
                {
                    if (stream.Length > 0)
                    {
                        byte[] data;
                        data = new byte[stream.Length];
                        stream.Position = 0;
                        stream.Read(data, 0, (int)stream.Length);
                        stream.SetLength(0);
                        stream.Position = 0;
                        using (FileStream file = new FileStream(name, FileMode.Append))
                        {
                            file.Write(data, 0, data.Length);
                            file.Flush();
                            file.Close();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            closing = true;
            Thread.Sleep(FlushTime);
            lock (syncObj)
            {
                stream.Close();
            }
        }
    }
}
