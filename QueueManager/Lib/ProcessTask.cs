using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager.Lib
{
    public class ProcessTask
    {
        public async Task<ITaskResult> Execute(int x, int y)
        {
            await Task.Delay(300);
            Console.WriteLine($"{x}+{y}={x + y}");
            TaskResult tr = new TaskResult();
            tr.IsComplete = true;
            tr.message = $"{x + y}";
            return tr;
        }
    }
}
