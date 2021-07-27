using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager
{
    public interface ITaskResult
    {
        bool IsComplete { get; set; }
        int Message { get; set; }
    }
}
