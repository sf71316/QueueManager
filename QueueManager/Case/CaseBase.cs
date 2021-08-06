using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager.Case
{
    public abstract class CaseBase
    {
        public abstract void ExecuteAsync(string cmd = "");
    }
}
