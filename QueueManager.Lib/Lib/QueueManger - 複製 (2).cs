using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace QueueManager.Lib
{
    public class QueueManger
    {
        //QueueGroup 的Queue
        static Lazy<ConcurrentDictionary<string, ConcurrentQueue<Task<ITaskResult>>>> _PublicQueue =
            new Lazy<ConcurrentDictionary<string, ConcurrentQueue<Task<ITaskResult>>>>();
        //執行QueueGroup 的Task
        static Lazy<ConcurrentDictionary<string, QueueTaskInfo>> _PublicPoolTask =
            new Lazy<ConcurrentDictionary<string, QueueTaskInfo>>();
        public bool EnableAddQueueAutoProcess { get; set; } = true;
        public void StartQueue(string queueKey)
        {
            //當queuetask 不存在時建立一個task 並執行
            //當queuetask 存在時且Task status 非WaitingForActivation,WaitingToRun,Running 需啟動Task
            //用lock 確保每個queuekey開會task 只會有一個


            var qTaskInfo = _PublicPoolTask.Value.GetOrAdd(queueKey, new QueueTaskInfo());
            lock (qTaskInfo)
            {
                if (!qTaskInfo.IsRunning)
                {
                    try
                    {
                        qTaskInfo.IsRunning = true;
                        _PublicPoolTask.Value.AddOrUpdate(queueKey, qTaskInfo, (p1, p2) =>
                        {
                            return qTaskInfo;
                        });

                        ProcessQueue(queueKey);
                    }
                    finally
                    {
                        //if (hasLock)
                        //{
                        //    Monitor.Exit(qk);
                        //}
                    }
                }
            }

        }
        private void ProcessQueue(string queueKey)
        {

            ThreadPool.QueueUserWorkItem(new WaitCallback(p =>
            {
                //執行Queue 裡面的task
                ConcurrentQueue<Task<ITaskResult>> _queue;
                if (_PublicQueue.Value.TryGetValue(queueKey, out _queue))
                {
                    while (!_queue.IsEmpty)
                    {
                        Task<ITaskResult> _task;
                        if (_queue.TryDequeue(out _task))
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"Process queue key [{queueKey}] task id {Thread.CurrentThread.ManagedThreadId}");
                            _task.Start();
                            _task.Wait();
                            _task.Dispose();
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"Queue count:{_queue.Count}");
                        }
                    }
                    var qTaskInfo = _PublicPoolTask.Value.GetOrAdd(queueKey, new QueueTaskInfo());
                    if (!_PublicPoolTask.Value.TryRemove(queueKey, out qTaskInfo))
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"移除QueueTask 失敗");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"移除QueueTask 成功");
                    }
                }
            }));

        }
        private Task GetQueueTask(string queueKey)
        {
            var queueTask = new Task(() =>
            {
                //執行Queue 裡面的task
                ConcurrentQueue<Task<ITaskResult>> _queue;
                if (_PublicQueue.Value.TryGetValue(queueKey, out _queue))
                {
                    while (!_queue.IsEmpty)
                    {
                        Task<ITaskResult> _task;
                        if (_queue.TryDequeue(out _task))
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"Queue key [{queueKey}] task id {Task.CurrentId}");
                            _task.Start();
                            _task.Wait();

                        }
                    }
                }
            });
            return queueTask;
        }
        public void AddInQueue(string queueKey, Task<ITaskResult> task)
        {
            var _queue = _PublicQueue.Value.GetOrAdd(queueKey, new Func<string, ConcurrentQueue<Task<ITaskResult>>>(
                 p =>
                 {
                     return new ConcurrentQueue<Task<ITaskResult>>();
                 }));
            _queue.Enqueue(task);
            if (EnableAddQueueAutoProcess)
                StartQueue(queueKey);
        }

        private void OnNotify(string message)
        {
            if (this.Notify != null)
            {
                this.Notify(this, new MessageArg
                {
                    Message = message
                });
            }
        }
        public event EventHandler<MessageArg> Notify;
    }
    public class MessageArg : EventArgs
    {
        public string Message { get; set; }
    }
}
