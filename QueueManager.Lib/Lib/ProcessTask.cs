using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager.Lib
{
    public class ProcessTask
    {
        public string QueueKey { get; set; }
        public int MinSleepValue { get; set; }
        public int MaxSleepValue { get; set; }
        public virtual ITaskResult Execute(int x, int y)
        {
            //Random rnd = new Random(Guid.NewGuid().GetHashCode());
            //System.Threading.Thread.Sleep(rnd.Next(MinSleepValue, this.MaxSleepValue));
            // Console.WriteLine($"{x}+{y}={x + y}");
            TaskResult tr = new TaskResult();
            tr.IsComplete = true;
            tr.message = $"{x + y}";
            return tr;
        }
    }
    public class ProcessTaskG1 : ProcessTask
    {
        public ProcessTaskG1()
        {
            this.QueueKey = "G1";
            this.MinSleepValue = 100;
            this.MaxSleepValue = 200;
        }
    }
    public class ProcessTaskG2 : ProcessTask
    {
        public ProcessTaskG2()
        {
            this.QueueKey = "G2";
            //this.MinSleepValue = 10;
            //this.MaxSleepValue = 20;
            this.MinSleepValue = 0;
            this.MaxSleepValue = 0;
        }
    }
}
