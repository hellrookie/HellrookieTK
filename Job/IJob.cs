using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hellrookie.ToolKit
{
    public delegate void JobMethod();
    public delegate void JobMethodWithParameter(object param);

    public interface IJob
    {
        bool IsInThread { get; set; }
        bool IsBackground { get; set; }
        void Run();
        void Abort();
        void SetParameter(object param);
        void SetRunMethod(JobMethod method);
        void SetRunMethod(JobMethodWithParameter method, object param);
    }
}
