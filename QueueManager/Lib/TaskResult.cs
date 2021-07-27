using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager.Lib
{
    public class TaskResult : ITaskResult
    {
        public bool IsComplete { get; set; }
        public string message { get; set; }
    }
}
