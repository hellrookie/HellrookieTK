using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hellrookie.ToolKit
{
    public class JobFactory
    {
        public static IJob CreateJob()
        {
            return new Job();
        }
    }
}
