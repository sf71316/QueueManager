using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager.Lib
{
    class QueueManger
    {
        //QueueGroup 的Queue
        static Lazy<ConcurrentDictionary<string, ConcurrentQueue<Task<ITaskResult>>>> _PublicQueue =
            new Lazy<ConcurrentDictionary<string, ConcurrentQueue<Task<ITaskResult>>>>();
        //執行QueueGroup 的Task
        static Lazy<ConcurrentDictionary<string, Task>> _PublicPoolTask =
            new Lazy<ConcurrentDictionary<string, Task>>();
        //static Lazy<QueueManger> _PoolManager = new Lazy<QueueManger>(() => new QueueManger());
        //public static QueueManger GetPoolManager()
        //{
        //    return _PoolManager.Value;
        //}
        public static async void StartQueue(string queueKey)
        {
            //當queuetask 不存在時建立一個task 並執行
            //當queuetask 存在時且Task status 非WaitingForActivation,WaitingToRun,Running 需啟動Task
            if (_PublicPoolTask.Value.ContainsKey(queueKey))
            {
                Task queueTask;
                if (_PublicPoolTask.Value.TryGetValue(queueKey, out queueTask))
                {
                    if (new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Created, TaskStatus.RanToCompletion }
                    .Any(p => p == queueTask.Status))
                    {
                        queueTask.Start();
                    }
                    else
                    {

                    }
                }

            }
            else
            {
                await _PublicPoolTask.Value.GetOrAdd(queueKey, Task.Run(() =>
                 {
                     //執行Queue 裡面的task
                     ConcurrentQueue<Task<ITaskResult>> _queue;
                     if (_PublicQueue.Value.TryGetValue(queueKey, out _queue))
                     {
                         while (_queue.Count > 0)
                         {
                             Task<ITaskResult> _task;
                             if (_queue.TryDequeue(out _task))
                             {
                                 _task.Start();
                                 
                             }
                         }
                     }
                 }));
            }

        }
        public void RegisterPool(string queueKey, Task<ITaskResult> task)
        {
            var _queue = _PublicQueue.Value.GetOrAdd(queueKey, new Func<string, ConcurrentQueue<Task<ITaskResult>>>(
                 p =>
                 {
                     return new ConcurrentQueue<Task<ITaskResult>>();
                 }));
            _queue.Enqueue(task);
            StartQueue(queueKey);
        }
        //public Queue<Task> GetPool(string queueKey)
        //{
        //    if (_PublicQueue.Value.ContainsKey(queueKey))
        //    {
        //        Queue<Task> _model = null;
        //        if (_PublicQueue.Value.TryGetValue(queueKey, out _model))
        //        {
        //            return _model;
        //        }
        //    }
        //    return null;
        //}

    }
}
