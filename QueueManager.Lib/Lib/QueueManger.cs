﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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
        static Lazy<ConcurrentBag<string>> _PublicPoolTask =
            new Lazy<ConcurrentBag<string>>();
        public bool EnableAddQueueAutoProcess { get; set; } = true;
        public void StartQueue(string queueKey)
        {
            //當queuetask 不存在時建立一個task 並執行
            //用lock 確保每個queuekey開會task 只會有一個
            bool lockTaken = Monitor.TryEnter(_PublicPoolTask.Value, 5000);
            if (lockTaken)
            {
                try
                {
                    if (!_PublicPoolTask.Value.Contains(queueKey))
                    {
                        _PublicPoolTask.Value.Add(queueKey);
                        //ProcessQueueByThread(queueKey);
                        //  ProcessQueueByTask(queueKey);
                        ProcessQueueByHosting(queueKey);
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(_PublicPoolTask.Value);
                    }
                }
            }
            else
            {
                Debug.WriteLine("not get queue task lock....................");
            }

        }
        private void ProcessQueueByThread(string queueKey)
        {

            var bw = new BackgroundWorker();
            bw.DoWork += (sender, e) =>
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
                            Debug.WriteLine($"Process queue key [{queueKey}] task id {Thread.CurrentThread.ManagedThreadId}");
                            _task.Start();
                            _task.Wait();
                            Debug.WriteLine($"Queue[{queueKey}] count:{_queue.Count}");
                        }
                    }
                    if (!_PublicPoolTask.Value.TryTake(out queueKey))
                    {

                        Debug.WriteLine($"移除QueueTask [{queueKey}] 失敗");
                    }
                    else
                    {
                        Debug.WriteLine($"移除QueueTask [{queueKey}] 成功");
                    }
                }
            };
            bw.RunWorkerAsync();
            //ThreadPool.QueueUserWorkItem(new WaitCallback(p =>
            //{
            //    //執行Queue 裡面的task
            //    ConcurrentQueue<Task<ITaskResult>> _queue;
            //    if (_PublicQueue.Value.TryGetValue(queueKey, out _queue))
            //    {
            //        while (!_queue.IsEmpty)
            //        {
            //            Task<ITaskResult> _task;
            //            if (_queue.TryDequeue(out _task))
            //            {
            //                Debug.ForegroundColor = DebugColor.Blue;
            //                Debug.WriteLine($"Process queue key [{queueKey}] task id {Thread.CurrentThread.ManagedThreadId}");
            //                _task.Start();
            //                _task.Wait();
            //                Debug.WriteLine($"Queue[{queueKey}] count:{_queue.Count}");
            //            }
            //        }
            //        if (!_PublicPoolTask.Value.TryTake(out queueKey))
            //        {
            //            Debug.ForegroundColor = DebugColor.Blue;

            //            Debug.WriteLine($"移除QueueTask [{queueKey}] 失敗");
            //        }
            //        else
            //        {
            //            Debug.ForegroundColor = DebugColor.Blue;
            //            Debug.WriteLine($"移除QueueTask [{queueKey}] 成功");
            //        }
            //    }
            //}));

        }
        private void ProcessQueueByHosting(string queueKey)
        {
            HostingEnvironment.QueueBackgroundWorkItem(ct =>
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
                            Debug.WriteLine($"Process queue key [{queueKey}] task id {Task.CurrentId}");
                            _task.Start();
                            _task.Wait();
                            Debug.WriteLine($"Queue[{queueKey}] count:{_queue.Count}");
                        }
                    }
                    if (!_PublicPoolTask.Value.TryTake(out queueKey))
                    {

                        Debug.WriteLine($"移除QueueTask [{queueKey}] 失敗");
                    }
                    else
                    {
                        Debug.WriteLine($"移除QueueTask [{queueKey}] 成功");
                    }
                }
            });

        }
        private void ProcessQueueByTask(string queueKey)
        {
            Task.Factory.StartNew(() =>
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
                            Debug.WriteLine($"Process queue key [{queueKey}] task id {Task.CurrentId}");
                            _task.Start();
                            _task.Wait();
                            Debug.WriteLine($"Queue[{queueKey}] count:{_queue.Count}");
                        }
                    }
                    if (!_PublicPoolTask.Value.TryTake(out queueKey))
                    {

                        Debug.WriteLine($"移除QueueTask [{queueKey}] 失敗");
                    }
                    else
                    {
                        Debug.WriteLine($"移除QueueTask [{queueKey}] 成功");
                    }
                }

            }, TaskCreationOptions.LongRunning);//.ConfigureAwait(false);

        }
        //public void StartAllQueueByTask()
        //{
        //    Task.Factory.StartNew(() =>
        //    {
        //        while (true)
        //        {
        //            //執行Queue 裡面的task
        //            ConcurrentQueue<Task<ITaskResult>> _queue;
        //            if (_PublicQueue.Value.TryGetValue(queueKey, out _queue))
        //            {
        //                while (!_queue.IsEmpty)
        //                {
        //                    Task<ITaskResult> _task;
        //                    if (_queue.TryDequeue(out _task))
        //                    {
        //                        Debug.ForegroundColor = DebugColor.Blue;
        //                        Debug.WriteLine($"Process queue key [{queueKey}] task id {Task.CurrentId}");
        //                        _task.Start();
        //                        _task.Wait();
        //                        Debug.WriteLine($"Queue[{queueKey}] count:{_queue.Count}");
        //                    }
        //                }
        //                if (!_PublicPoolTask.Value.TryTake(out queueKey))
        //                {
        //                    Debug.ForegroundColor = DebugColor.Blue;

        //                    Debug.WriteLine($"移除QueueTask [{queueKey}] 失敗");
        //                }
        //                else
        //                {
        //                    Debug.ForegroundColor = DebugColor.Blue;
        //                    Debug.WriteLine($"移除QueueTask [{queueKey}] 成功");
        //                }
        //            }
        //        }
        //    }, TaskCreationOptions.LongRunning);//.ConfigureAwait(false);

        //}
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
