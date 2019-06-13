using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Hellrookie.ToolKit
{
    public class StaticLogger : Logger
    {
        private static object lockObj = new object();
        private static StaticLogger instance;

        private StaticLogger()
            : base(Process.GetCurrentProcess().ProcessName)
        {
        }

        public static StaticLogger Instance
        {
            get
            {
                if(instance == null)
                {
                    lock(lockObj)
                    {
                        if(instance == null)
                        {
                            instance = new StaticLogger();
                        }
                    }
                }
                return instance;
            }
        }
    }
}
