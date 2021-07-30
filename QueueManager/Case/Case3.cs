using QueueManager.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager.Case
{

    /// <summary>
    /// 模擬2個Queue Task 每個queue 處理時間不同
    /// Queue key G1 : process time 100~200ms
    /// Queue key G2 : process time 10~20ms
    /// 預估結果 G2 會比G1 早完成
    /// </summary>
    class Case3
    {
        public void Execute()
        {
            QueueManger queueManger = new QueueManger();
            queueManger.Notify += QueueManger_Notify;
            //string queuekey = "test";
            int processCount = 50;
            int processCount2 = 50;
            //queueManger.EnableAddQueueAutoProcess = false;

            for (int i = 0; i < processCount; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    Stopwatch sw = new Stopwatch();
                    Random rnd = new Random(Guid.NewGuid().GetHashCode());
                    var x = rnd.Next(1, 11);
                    var y = rnd.Next(12, 21);
                    ProcessTaskG1 processTask = new ProcessTaskG1();
                    var t = new Task<ITaskResult>(() =>
                    {
                        ITaskResult taskResult = processTask.Execute(x, y);
                        return taskResult;
                    }, TaskCreationOptions.None);
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
                });

            }
            for (int i = 0; i < processCount2; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    Stopwatch sw2 = new Stopwatch();
                    Random rnd = new Random(Guid.NewGuid().GetHashCode());
                    var x = rnd.Next(1, 11);
                    var y = rnd.Next(12, 21);
                    ProcessTaskG2 processTask = new ProcessTaskG2();
                    var t = new Task<ITaskResult>(() =>
                    {
                        ITaskResult taskResult = processTask.Execute(x, y);
                        return taskResult;
                    }, TaskCreationOptions.AttachedToParent);
                    queueManger.AddInQueue(processTask.QueueKey, t);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Task id:{Task.CurrentId} queue task:{processTask.QueueKey} waiting...");
                    sw2.Start();
                    t.Wait();
                    sw2.Stop();
                    if (t.IsCompleted)
                    {

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Task id:{Task.CurrentId} queue task:{processTask.QueueKey} receivied result {t.Result.message} elapsed:{sw2.ElapsedMilliseconds}ms");

                    }
                    else
                    {
                        Console.WriteLine($"^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
                    }
                    sw2.Reset();
                });

            }
            //queueManger.StartQueue(queuekey);


        }
        private void QueueManger_Notify(object sender, MessageArg e)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(e.Message);
        }
    }
}
