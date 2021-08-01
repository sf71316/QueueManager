using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueManager.Lib
{
    public class ProcessQueue<T>
    {
        private BlockingCollection<T> _queue;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellToken;
        //內部執行緒池
        private List<Thread> _threadCollection;

        //佇列是否正在處理資料
        private int _isProcessing;
        //有執行緒正在處理資料
        private const int Processing = 1;
        //沒有執行緒處理資料
        private const int UnProcessing = 0;
        //佇列是否可用
        private volatile bool _enabled = true;
        //內部處理執行緒數量
        private int _internalThreadCount;

        public event Action<T> ProcessItemEvent;
        //處理異常，需要三個引數，當前佇列例項，異常，當時處理的資料
        public event Action<dynamic, Exception, T> ProcessExceptionEvent;

        public ProcessQueue()
        {
            _queue = new BlockingCollection<T>();
            _cancellationTokenSource = new CancellationTokenSource();
            _internalThreadCount = 1;
            _cancellToken = _cancellationTokenSource.Token;
            _threadCollection = new List<Thread>();
        }

        public ProcessQueue(int internalThreadCount) : this()
        {
            this._internalThreadCount = internalThreadCount;
        }

        /// <summary>
        /// 佇列內部元素的數量 
        /// </summary>
        public int GetInternalItemCount()
        {
            return _queue.Count;
        }

        public void Enqueue(T items)
        {
            if (items == null)
            {
                throw new ArgumentException("items");
            }

            _queue.Add(items);
            DataAdded();
        }

        public void Flush()
        {
            StopProcess();

            while (_queue.Count != 0)
            {
                T item = default(T);
                if (_queue.TryTake(out item))
                {
                    try
                    {
                        ProcessItemEvent(item);
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(ex, item);
                    }
                }
            }
        }

        private void DataAdded()
        {
            if (_enabled)
            {
                if (!IsProcessingItem())
                {
                    ProcessRangeItem();
                    StartProcess();
                }
            }
        }

        //判斷是否佇列有執行緒正在處理 
        private bool IsProcessingItem()
        {
            return !(Interlocked.CompareExchange(ref _isProcessing, Processing, UnProcessing) == UnProcessing);
        }

        private void ProcessRangeItem()
        {
            for (int i = 0; i < this._internalThreadCount; i++)
            {
                ProcessItem();
            }
        }

        private void ProcessItem()
        {
            Thread currentThread = new Thread((state) =>
            {
                T item = default(T);
                while (_enabled)
                {
                    try
                    {
                        try
                        {
                            item = _queue.Take(_cancellToken);
                            ProcessItemEvent(item);
                        }
                        catch (OperationCanceledException ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                    }
                    catch (Exception ex)
                    {
                        OnProcessException(ex, item);
                    }
                }

            });

            _threadCollection.Add(currentThread);
        }

        private void StartProcess()
        {
            foreach (var thread in _threadCollection)
            {
                thread.Start();
            }
        }

        private void StopProcess()
        {
            this._enabled = false;
            foreach (var thread in _threadCollection)
            {
                if (thread.IsAlive)
                {
                    thread.Join();
                }
            }
            _threadCollection.Clear();
        }

        private void OnProcessException(Exception ex, T item)
        {
            var tempException = ProcessExceptionEvent;
            Interlocked.CompareExchange(ref ProcessExceptionEvent, null, null);

            if (tempException != null)
            {
                ProcessExceptionEvent(this, ex, item);
            }
        }

    }
}
