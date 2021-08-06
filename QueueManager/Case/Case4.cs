using QueueManager.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueManager.Case
{
    class Case4 : CaseBase
    {
        public override void ExecuteAsync(string cmd="")
        {
            //ThreadPool.SetMaxThreads(1000, 1000);
            QueueManger queueManger = new QueueManger();
            queueManger.Notify += QueueManger_Notify;
            //string queuekey = "test";
            int processCount = 10;
            int queueCount = 5;
            //queueManger.EnableAddQueueAutoProcess = false;
            List<Task> tc = new List<Task>();
            //queueManger.StartAllQueueByTask();
            for (int j = 0; j < queueCount; j++)
            {
                var qk = j.ToString();
                for (int i = 0; i < processCount; i++)
                {
                    tc.Add(
                        new Task(() =>
                        {
                            Stopwatch sw = new Stopwatch();
                            Random rnd = new Random(Guid.NewGuid().GetHashCode());
                            var x = rnd.Next(1, 11);
                            var y = rnd.Next(12, 21);
                            var processTask = new ProcessTaskG2();
                            var t = new Task<ITaskResult>(() =>
                            {
                                ITaskResult taskResult = processTask.Execute(x, y);
                                return taskResult;
                            }, TaskCreationOptions.DenyChildAttach);
                            processTask.QueueKey = qk;
                            queueManger.AddInQueue(processTask.QueueKey, t);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Task id:{Task.CurrentId} queue task:{processTask.QueueKey} waiting...");
                            sw.Start();
                            t.Wait();

                            sw.Stop();
                            if (t.IsCompleted)
                            {

                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Task id:{Task.CurrentId} queue task:{processTask.QueueKey} receivied result {t.Result.message} elapsed:{sw.ElapsedMilliseconds}ms");

                            }
                            else
                            {
                                Console.WriteLine($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                            }
                            sw.Reset();
                        },TaskCreationOptions.DenyChildAttach)
                        );
                }
            }
            //queueManger.StartQueue(queuekey);
            Parallel.ForEach(tc, task =>
            {
                task.Start();
                // System.Threading.Thread.Sleep(6);
            });
            //foreach (var item in tc)
            //{
            //    item.Start();
            //}
        }
        private void QueueManger_Notify(object sender, MessageArg e)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(e.Message);
        }
    }
}
