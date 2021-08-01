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
        static Lazy<ConcurrentDictionary<string, bool>> _PublicQueueTask =
            new Lazy<ConcurrentDictionary<string, bool>>();
        public bool EnableAddQueueAutoProcess { get; set; } = true;
        public void StartQueue(string queueKey)
        {
            //當queuetask 不存在時建立一個task 並執行
            //用lock 確保每個queuekey開會task 只會有一個
            bool lockTaken = Monitor.TryEnter(_PublicQueueTask.Value, 5000);
            if (lockTaken)
            {
                try
                {
                    if (!_PublicQueueTask.Value.ContainsKey(queueKey))
                    {
                        _PublicQueueTask.Value.TryAdd(queueKey, false);
                        //ProcessQueueByThread(queueKey);
                        ProcessQueueByTask(queueKey);
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(_PublicQueueTask.Value);
                    }
                }
            }
            else
            {
                Console.WriteLine("not get queue task lock....................");
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
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"Process queue key [{queueKey}] task id {Thread.CurrentThread.ManagedThreadId}");
                            _task.Start();
                            _task.Wait();
                            Console.WriteLine($"Queue[{queueKey}] count:{_queue.Count}");
                        }
                    }
                    var v = false;
                    if (!_PublicQueueTask.Value.TryRemove(queueKey, out v))
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;

                        Console.WriteLine($"移除QueueTask [{queueKey}] 失敗");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine($"移除QueueTask [{queueKey}] 成功");
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
            //                Console.ForegroundColor = ConsoleColor.Blue;
            //                Console.WriteLine($"Process queue key [{queueKey}] task id {Thread.CurrentThread.ManagedThreadId}");
            //                _task.Start();
            //                _task.Wait();
            //                Console.WriteLine($"Queue[{queueKey}] count:{_queue.Count}");
            //            }
            //        }
            //        if (!_PublicPoolTask.Value.TryTake(out queueKey))
            //        {
            //            Console.ForegroundColor = ConsoleColor.Blue;

            //            Console.WriteLine($"移除QueueTask [{queueKey}] 失敗");
            //        }
            //        else
            //        {
            //            Console.ForegroundColor = ConsoleColor.Blue;
            //            Console.WriteLine($"移除QueueTask [{queueKey}] 成功");
            //        }
            //    }
            //}));

        }

        private void ProcessQueueByTask(string queueKey)
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
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
                                Console.WriteLine($"Process queue key [{queueKey}] task id {Task.CurrentId}");
                                _task.Start();
                                _task.Wait();
                                Console.WriteLine($"Queue[{queueKey}] count:{_queue.Count}");
                            }
                        }
                        var v = false;
                        if (!_PublicQueueTask.Value.TryRemove(queueKey, out v))
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;

                            Console.WriteLine($"移除QueueTask [{queueKey}] 失敗");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"移除QueueTask [{queueKey}] 成功");
                        }
                    }
                }
            }, TaskCreationOptions.DenyChildAttach);//.ConfigureAwait(false);

        }
        public void StartAllQueueByTask()
        {
            bool lockTaken = Monitor.TryEnter(_PublicQueueTask.Value, 5000);
            if (lockTaken)
            {
                try
                {
                    Task.Factory.StartNew(() =>
                    {
                        while (true)
                        {
                            var readytoRunTask = _PublicQueueTask.Value.Where(p => !p.Value);
                            foreach (var qTask in readytoRunTask)
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    while (true)
                                    {
                                        //執行Queue 裡面的task
                                        ConcurrentQueue<Task<ITaskResult>> _queue;
                                        if (_PublicQueue.Value.TryGetValue(qTask.Key, out _queue))
                                        {
                                            while (!_queue.IsEmpty)
                                            {
                                                Task<ITaskResult> _task;
                                                if (_queue.TryDequeue(out _task))
                                                {
                                                    Console.ForegroundColor = ConsoleColor.Blue;
                                                    Console.WriteLine($"Process queue key [{qTask.Key}] task id {Task.CurrentId}");
                                                    _task.Start();
                                                    _task.Wait();
                                                    Console.WriteLine($"Queue[{qTask.Key}] count:{_queue.Count}");
                                                }
                                            }

                                        }
                                    }
                                }, TaskCreationOptions.LongRunning);//.ConfigureAwait(false);
                                _PublicQueueTask.Value.AddOrUpdate(qTask.Key, true, (p1, p2) => true);
                            }
                        }
                    });
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(_PublicQueueTask.Value);
                    }
                }
            }
            else
            {
                Console.WriteLine("not get queue task lock....................");
            }



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
