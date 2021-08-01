using Newtonsoft.Json;
using QueueManager.Lib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager.Case
{
    public class Case6 : CaseBase
    {
        public override async void ExecuteAsync()
        {
            //Console.WriteLine("Waiting");
            //System.Threading.Thread.Sleep(3 * 1000);
            string uri = "https://localhost:44369/api/Queue";
            ConcurrentBag<TaskResult> results = new ConcurrentBag<TaskResult>();
            ConcurrentBag<Task> tc = new ConcurrentBag<Task>();
            for (int i = 0; i< 20; i++)
            {
                Task t = new Task(( () =>
                 {
                     Stopwatch sw = new Stopwatch();
                     using (HttpClient client = new HttpClient())
                     {
                         try
                         {
                             Random rnd = new Random(Guid.NewGuid().GetHashCode());
                             var x = rnd.Next(1, 11);
                             var y = rnd.Next(12, 21);
                             QueueModel model = new QueueModel();
                             model.QueueKey = "test";
                             model.X = x;
                             model.Y = y;
                             Console.WriteLine($"TaskId:{Task.CurrentId} send request");
                             sw.Start();
                             client.Timeout = TimeSpan.FromSeconds(30);
                             // 將 data 轉為 json
                             string json = JsonConvert.SerializeObject(model);
                             // 將轉為 string 的 json 依編碼並指定 content type 存為 httpcontent
                             HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");
                             HttpResponseMessage response =  client.PostAsync(uri, contentPost).Result;
                             response.EnsureSuccessStatusCode();
                             string responseBody =  response.Content.ReadAsStringAsync().Result;
                             results.Add(JsonConvert.DeserializeObject<TaskResult>(responseBody));
                             sw.Stop();
                             Console.WriteLine(responseBody);
                             Console.WriteLine($"TaskId:{Task.CurrentId} receivied response {sw.ElapsedMilliseconds} ms");
                             sw.Reset();
                         }
                         catch (HttpRequestException e)
                         {
                             Console.WriteLine("\nException Caught!");
                             Console.WriteLine("Message :{0} ", e.Message);
                         }
                     }
                 }));
                tc.Add(t);
                t.Start();
            }
            if (Task.WaitAll(tc.ToArray(), 100000))
            {
                Console.WriteLine($"receivied {results.Count} result");
            }

        }
    }
}
