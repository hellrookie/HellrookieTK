using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hellrookie.ToolKit
{
    /// <summary>
    /// 心跳事件
    /// </summary>
    public delegate void HeartBeatEvent();
    /// <summary>
    /// 心跳
    /// </summary>
    public class HeartBeat
    {
        private HeartBeatEvent heartBeatDeadEvent { get; set; }
        private HeartBeatEvent heartBeatStartFailEvent { get; set; }

        /// <summary>
        /// 开始心跳
        /// </summary>
        public void Start()
        {
            
        }

        private void StartFail()
        {
            
        }

        private void HeartBeatDead(HeartBeatEvent handler)
        {
            if (heartBeatDeadEvent != null)
            {
                heartBeatDeadEvent();
            }
        }
    }
}
