using QueueManager.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager
{
    /// <summary>
    /// 測試Queue 是否照安排依序執行
    /// </summary>
    public class Case1
    {
        public void Execute()
        {
            QueueManger queueManger = new QueueManger();
            queueManger.Notify += QueueManger_Notify;
            string[] queuekey = new string[] { "test", "222" };
            int processCount = 20;
            queueManger.EnableAddQueueProcess = false;
            for (int i = 0; i < processCount; i++)
            {
                var task = new Task<ITaskResult>(() =>
                {
                    Random rnd = new Random(Guid.NewGuid().GetHashCode());
                    var tr = new TaskResult();
                    int pt = rnd.Next(1, 11);
                    Console.WriteLine($"[TaskID:{Task.CurrentId}] Process task ");
                    Console.WriteLine($"processing......{pt * 100}ms");
                    System.Threading.Thread.Sleep(pt * 100);
                    Console.WriteLine($"[TaskID:{Task.CurrentId}] Process task finish");
                    return tr;
                });
                var qk = "";
                if (i < 10)
                    qk = queuekey[0];
                else
                    qk = queuekey[1];
                queueManger.AddInQueue(qk, task);
                Console.WriteLine($"add task in queue [{qk}]");
            }

            queueManger.StartQueue(queuekey[0]);
            queueManger.StartQueue(queuekey[1]);

        }

        private void QueueManger_Notify(object sender, MessageArg e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
