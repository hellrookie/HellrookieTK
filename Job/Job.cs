using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Hellrookie.ToolKit
{
    class Job : IJob
    {
        private enum JobMethodType
        {
            Invalid,
            NoneParam,
            WithParam,
        }

        private JobMethod jobMethod;
        private JobMethodWithParameter jobMethodWithParam;
        private object param;
        private JobMethodType jobMethodType = JobMethodType.Invalid;

        private Thread methodThread;
        public Exception MethodException { get; private set; }
        public bool IsInThread { get; set; }
        public bool IsBackground { get; set; }

        public void SetParameter(object param)
        {
            this.param = param;
        }

        private void RunInternal()
        {
            try
            {
                switch (jobMethodType)
                {
                    case JobMethodType.NoneParam:
                        {
                            jobMethod();
                            break;
                        }
                    case JobMethodType.WithParam:
                        {
                            jobMethodWithParam(param);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            catch(Exception ex)
            {
                MethodException = ex;
            }
        }

        public virtual void Run()
        {
            MethodException = null;
            if (jobMethodType == JobMethodType.Invalid)
            {
                throw new Exception("Method has not been initialized.");
            }
            if (IsInThread)
            {
                methodThread = new Thread(RunInternal);
                methodThread.IsBackground = IsBackground;
                methodThread.Start();
            }
            else
            {
                RunInternal();
            }
        }

        public void Abort()
        {
            if(methodThread != null && methodThread.IsAlive)
            {
                methodThread.Abort();
            }
        }

        public void SetRunMethod(JobMethod method)
        {
            this.jobMethod = method;
            jobMethodType = JobMethodType.NoneParam;
        }

        public void SetRunMethod(JobMethodWithParameter method, object param)
        {
            this.jobMethodWithParam = method;
            this.param = param;
            jobMethodType = JobMethodType.WithParam;
        }
    }
}
