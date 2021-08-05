using QueueManager.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace QueueMAnager.Service.Controllers
{
    /// <summary>
    /// test
    /// </summary>
    public class QueueController : ApiController
    {
        /// <summary>
        /// test
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public IHttpActionResult Post(QueueModel value)
        {
            QueueManger queueManger = new QueueManger();
            ProcessTaskG2 g2 = new ProcessTaskG2();
            g2.QueueKey = value.QueueKey;
            Debug.WriteLine($"Receivid client task:{value.TaskId}");
            var t = new Task<ITaskResult>(() =>
            {
                ITaskResult taskResult = g2.Execute(value.X, value.Y);
                return taskResult;
            }, TaskCreationOptions.DenyChildAttach);
            queueManger.AddInQueue(g2.QueueKey, t);
            t.Wait();
            if (t.IsCompleted)
            {
                return this.Ok(t.Result);
            }
            else
            {
                return this.Ok("failure");
            }
        }
        public string Get()
        {
            return $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")}";
        }
    }
}