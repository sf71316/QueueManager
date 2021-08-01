using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager.Lib
{
    public interface ITaskResult
    {
        bool IsComplete { get; set; }
        string message { get; set; }
    }
}
