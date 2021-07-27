using QueueManager.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager
{
    class Case1
    {
        public void Execute()
        {
            Lib.QueueManger queueManger = new QueueManger();
            queueManger.Notify += QueueManger_Notify;
            int taskCount = 20;
            var queuekeys = new string[] { "test", "test2" };
            for (int i = 0; i < taskCount; i++)
            {
                Random rnd2 = new Random(Guid.NewGuid().GetHashCode());
                var queuekey = rnd2.Next(0, 2);
                var t = new Task<ITaskResult>(() =>
                {
                    Random rnd = new Random(Guid.NewGuid().GetHashCode());
                    var timesleep = rnd.Next(1, 5) * 100;
                    TaskResult tr = new TaskResult();
                    Console.WriteLine($"Process taskid:{Task.CurrentId} ");
                    Console.WriteLine($"Process.....{timesleep} ms");
                    System.Threading.Thread.Sleep(timesleep);
                    Console.WriteLine($"Process id:{Task.CurrentId}.....{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")} End");
                    return tr;
                });
                //Console.WriteLine($"{queuekey}");
                queueManger.AddInQueue(queuekeys[queuekey], t);

            }
            //queueManger.StartQueue("test");
        }

        private void QueueManger_Notify(object sender, MessageArg e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
