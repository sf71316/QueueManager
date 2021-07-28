using QueueManager.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            //queueManger.EnableAddQueueAutoProcess = false;
            CustomTaskScheduler cts = new CustomTaskScheduler(int.MaxValue);
            ProcessTask processTask = new ProcessTask();

            Parallel.For(0, processCount, i =>
            {
                Stopwatch sw = new Stopwatch();

                Random rnd = new Random(Guid.NewGuid().GetHashCode());
                var x = rnd.Next(1, 11);
                var y = rnd.Next(12, 21);
                var t = new Task<ITaskResult>(() =>
                {
                    ITaskResult taskResult = processTask.Execute(x, y).Result;
                    return taskResult;
                }, TaskCreationOptions.PreferFairness);
                queueManger.AddInQueue(queuekey, t);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Task id:{Task.CurrentId} waiting...");
                sw.Start();
                t.Wait();
                if (t.IsCompleted)
                {
                    sw.Stop();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Task id:{Task.CurrentId} receivied result {t.Result.message} elapsed:{sw.ElapsedMilliseconds}ms");
                }
            });
            //queueManger.StartQueue(queuekey);


        }
        private void QueueManger_Notify(object sender, MessageArg e)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(e.Message);
        }
    }

}
