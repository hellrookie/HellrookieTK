using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Hellrookie.ToolKit
{
    /// <summary>
    /// 超时事件
    /// </summary>
    /// <param name="param"></param>
    public delegate void TimeOutProcess(object param);

    /// <summary>
    /// 定时器
    /// </summary>
    public class Timer
    {
        private SortedList<DateTime, List<TimeOutBlock>> timeOutBlocks;
        private DateTime nextCheckTime;
        private object syncObj = new object();
        private Thread mainThread;
        /// <summary>
        /// 构造定时器
        /// </summary>
        public Timer()
        {
            timeOutBlocks = new SortedList<DateTime, List<TimeOutBlock>>();
            // default set a time enough long.
            nextCheckTime = DateTime.Now.AddYears(1);
        }

        /// <summary>
        /// 启动定时器
        /// </summary>
        public void Run()
        {
            mainThread = new Thread(MainThread);
            mainThread.IsBackground = true;
            mainThread.Name = "Zjq.Utility.Timer_MainThread";
            mainThread.Priority = ThreadPriority.AboveNormal;
            mainThread.Start();
        }

        /// <summary>
        /// 停止定时器
        /// </summary>
        public void Stop()
        {
            mainThread.Abort();
        }

        #region AddTimeEvent
        /// <summary>
        /// Add time arrive event run infinite.
        /// </summary>
        /// <param name="process">Process to do when time arrive.</param>
        /// <param name="processParam">Parameter to pass to the Process.</param>
        /// <param name="startTime">First arrive time.</param>
        /// <param name="interval">Interval(Milliseconds) to reach another time arrive event.</param>
        public void AddTimeEvent(TimeOutProcess process, object processParam, DateTime startTime, double interval)
        {
            AddTimeEvent(process, processParam, startTime, DateTime.MaxValue, interval, -1);
        }

        /// <summary>
        /// Add time arrive event with the number of time arrive event repetitions.
        /// </summary>
        /// <param name="process">Process to do when time arrive.</param>
        /// <param name="processParam">Parameter to pass to the Process.</param>
        /// <param name="startTime">First arrive time.</param>
        /// <param name="interval">Interval(Milliseconds) to reach another time arrive event.</param>
        /// <param name="excuteTimes">The number of time arrive event repetitions.</param>
        public void AddTimeEvent(TimeOutProcess process, object processParam, DateTime startTime, double interval, int excuteTimes)
        {
            AddTimeEvent(process, processParam, startTime, DateTime.MaxValue, interval, excuteTimes);
        }

        /// <summary>
        /// Add time arrive event with end time.
        /// </summary>
        /// <param name="process">Process to do when time arrive.</param>
        /// <param name="processParam">Parameter to pass to the Process.</param>
        /// <param name="startTime">First arrive time.</param>
        /// <param name="endTime">The end time of time arrive event</param>
        /// <param name="interval">Interval(Milliseconds) to reach another time arrive event.</param>
        public void AddTimeEvent(TimeOutProcess process, object processParam, DateTime startTime, DateTime endTime, double interval)
        {
            AddTimeEvent(process, processParam, startTime, endTime, interval, -1);
        }

        private void AddTimeEvent(TimeOutProcess process, object processParam, DateTime startTime, DateTime endTime, double interval, int excuteTimes)
        {
            if (!timeOutBlocks.ContainsKey(startTime))
            {
                timeOutBlocks.Add(startTime, new List<TimeOutBlock>());
            }
            var blocks = timeOutBlocks[startTime];
            lock (syncObj)
            {
                blocks.Add(new TimeOutBlock(process, processParam, startTime, endTime, interval, excuteTimes));
                if (startTime < nextCheckTime)
                {
                    nextCheckTime = startTime;
                }
            }
        }
        #endregion

        private void MainThread()
        {
            while (true)
            {
                // wait until first time arrive event.
                while (timeOutBlocks.Count <= 0 || DateTime.Now < timeOutBlocks.First().Key)
                {
                    Thread.Sleep(1);
                }
                var blocks = timeOutBlocks.First();
                Thread th = new Thread(DoProcess);
                th.Start(blocks);
                lock (syncObj)
                {
                    timeOutBlocks.Remove(timeOutBlocks.First().Key);
                }
            }
        }

        private void DoProcess(object obj)
        {
            KeyValuePair<DateTime, List<TimeOutBlock>> blocks = (KeyValuePair<DateTime, List<TimeOutBlock>>)obj;
            var list = blocks.Value;
            var time = blocks.Key;
            foreach (var block in list)
            {
                var result = block.DoProcess(time);
                if (result > time)
                {
                    if (!timeOutBlocks.ContainsKey(result))
                    {
                        timeOutBlocks.Add(result, new List<TimeOutBlock>());
                    }
                    timeOutBlocks[result].Add(block);
                }
            }
        }
    }

    class TimeOutBlock
    {
        private TimeOutProcess process;
        private object param;
        public double Interval{ get; set; }
        public int excuteTimes;
        private int currentExcuteTime;
        private DateTime endTime;
        private DateTime nextTimeOutTime;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="process"></param>
        /// <param name="processParam"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="interval">Milliseconds</param>
        /// <param name="excuteTimes"></param>
        public TimeOutBlock(TimeOutProcess process, object processParam, DateTime startTime, DateTime endTime, double interval, int excuteTimes)
        {
            this.process = process;
            this.param = processParam;
            this.Interval = interval;
            this.excuteTimes = excuteTimes;
            this.currentExcuteTime = 0;
            this.endTime = endTime;
            if (startTime > DateTime.Now)
            {
                this.nextTimeOutTime = startTime;
            }
            else
            {
                this.nextTimeOutTime = DateTime.Now.AddMilliseconds(interval);
            }
        }

        public DateTime DoProcess(DateTime time)
        {
            // Do custom process.
            process(param);
            ++currentExcuteTime;
            if(excuteTimes > currentExcuteTime || excuteTimes == -1)
            {
                nextTimeOutTime = time.AddMilliseconds(Interval);
                if(nextTimeOutTime < endTime)
                {
                    return nextTimeOutTime;
                }
            }
            return time;
        }
    }
}
