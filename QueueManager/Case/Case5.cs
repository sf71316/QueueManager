using QueueManager.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager.Case
{
    public class Case5
    {
        public void Execute()
        {
            ProcessQueue<int> processQueue = new ProcessQueue<int>();
            processQueue.ProcessExceptionEvent += ProcessQueue_ProcessExceptionEvent;
            processQueue.ProcessItemEvent += ProcessQueue_ProcessItemEvent;

            processQueue.Enqueue(1);
            processQueue.Enqueue(2);
            processQueue.Enqueue(3);
        }
        /// <summary>
        /// 該方法對入隊的每個元素進行處理
        /// </summary>
        /// <param name="value"></param>
        private void ProcessQueue_ProcessItemEvent(int value)
        {
            Console.WriteLine(value);
        }

        /// <summary>
        ///  處理異常
        /// </summary>
        /// <param name="obj">佇列例項</param>
        /// <param name="ex">異常物件</param>
        /// <param name="value">出錯的資料</param>
        private void ProcessQueue_ProcessExceptionEvent(dynamic obj, Exception ex, int value)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
