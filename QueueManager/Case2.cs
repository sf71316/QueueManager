using QueueManager.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager
{
    public class Case2
    {
        public void Execute()
        {
            QueueManger queueManger = new QueueManger();
            queueManger.Notify += QueueManger_Notify;
            string queuekey = "test";
            int processCount = 20;
            //queueManger.EnableAddQueueProcess = false;

            ProcessTask processTask = new ProcessTask();
            for (int i = 0; i < processCount; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    Random rnd = new Random(Guid.NewGuid().GetHashCode());
                    var x = rnd.Next(1, 11);
                    var y = rnd.Next(12, 21);
                    var t = new Task<ITaskResult>(() =>
                    {
                        ITaskResult taskResult = processTask.Execute(x, y).Result;
                        return taskResult;
                    }, TaskCreationOptions.PreferFairness);
                    queueManger.AddInQueue(queuekey, t);
                    Console.WriteLine($"Task id:{Task.CurrentId} waiting...");
                    t.Wait();
                    if (t.IsCompleted)
                    {
                        Console.WriteLine($"receivied result {t.Result.message}");
                    }
                });
            }
            // queueManger.StartQueue(queuekey);


        }
        private void QueueManger_Notify(object sender, MessageArg e)
        {
            Console.WriteLine(e.Message);
        }
    }

}
