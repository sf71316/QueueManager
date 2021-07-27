using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager.Lib
{
    public class QueueManger
    {
        //QueueGroup 的Queue
        static Lazy<ConcurrentDictionary<string, ConcurrentQueue<Task<ITaskResult>>>> _PublicQueue =
            new Lazy<ConcurrentDictionary<string, ConcurrentQueue<Task<ITaskResult>>>>();
        //執行QueueGroup 的Task
        static Lazy<ConcurrentDictionary<string, Task>> _PublicPoolTask =
            new Lazy<ConcurrentDictionary<string, Task>>();
        public bool EnableAddQueueProcess { get; set; } = true;
        public void StartQueue(string queueKey)
        {
            //當queuetask 不存在時建立一個task 並執行
            //當queuetask 存在時且Task status 非WaitingForActivation,WaitingToRun,Running 需啟動Task
            //用lock 確保每個queuekey開會task 只會有一個
            lock (_PublicPoolTask)
            {
                if (_PublicPoolTask.Value.ContainsKey(queueKey))
                {
                    Task queueTask;
                    if (_PublicPoolTask.Value.TryGetValue(queueKey, out queueTask))
                    {
                        //當QueueTask 未啟動時，則啟動task
                        if (new TaskStatus[] { TaskStatus.Created }
                        .Any(p => p == queueTask.Status))
                        {
                            queueTask.Start();
                        }
                        //當QueueTask 為以下狀態時則取代問前Task
                        else if (new TaskStatus[] { TaskStatus.Canceled,TaskStatus.Faulted,
                        TaskStatus.RanToCompletion,TaskStatus.WaitingForActivation
                    ,}
                        .Any(p => p == queueTask.Status))
                        {
                            if (_PublicPoolTask.Value.TryRemove(queueKey, out queueTask))
                            {
                                var t = GetQueueTask(queueKey);
                                _PublicPoolTask.Value.AddOrUpdate(queueKey, t, (s, t1) =>
                                {
                                    return t;
                                });

                                t.Start();
                            }
                            else
                            {
                                this.OnNotify("處理佇列排程移除失敗");
                            }
                        }
                    }
                    else
                    {
                        this.OnNotify("加入處理佇列排程取得失敗");
                    }

                }
                else
                {

                    var t = GetQueueTask(queueKey);
                    _PublicPoolTask.Value.AddOrUpdate(queueKey, t, (s, t1) =>
                    {
                        return t;
                    });
                    t.Start();
                }
            }

        }
        private Task GetQueueTask(string queueKey)
        {
            var queueTask = new Task(() =>
            {
                System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.AboveNormal;
                //執行Queue 裡面的task
                ConcurrentQueue<Task<ITaskResult>> _queue;
                if (_PublicQueue.Value.TryGetValue(queueKey, out _queue))
                {
                    while (!_queue.IsEmpty)
                    {
                        Task<ITaskResult> _task;
                        if (_queue.TryDequeue(out _task))
                        {
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
            if (EnableAddQueueProcess)
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
