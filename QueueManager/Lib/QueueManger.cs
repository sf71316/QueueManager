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
        static Lazy<ConcurrentDictionary<string, Queue<Task>>> _PublicPool = new Lazy<ConcurrentDictionary<string, Queue<Task>>>();
        static Lazy<QueueManger> _PoolManager = new Lazy<QueueManger>(() => new QueueManger());
        public static QueueManger GetPoolManager()
        {
            return _PoolManager.Value;
        }
        public static async void Start()
        {
            await Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var allqueue = _PublicPool.Value.ToArray();
                    foreach (var queue in allqueue)
                    {
                        var task = queue.Value.Dequeue();
                        task.Start();

                    }
                    
                }
            });

        }
        public static void Stop()
        {

        }
        public Queue<Task> RegisterPool(string queueKey)
        {
            return _PublicPool.Value.GetOrAdd(queueKey, new Queue<Task>());
        }
        public bool IsCreated(string queueKey)
        {
            return _PublicPool.Value.ContainsKey(queueKey);
        }
        public Queue<Task> GetPool(string queueKey)
        {
            if (_PublicPool.Value.ContainsKey(queueKey))
            {
                Queue<Task> _model = null;
                if (_PublicPool.Value.TryGetValue(queueKey, out _model))
                {
                    return _model;
                }
            }
            return null;
        }

    }
}
