using Newtonsoft.Json;
using QueueManager.Lib;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueManager.Case
{
    public class Case6 : CaseBase
    {
        string uri = "https://localhost:44369/api/Queue";
        ConcurrentBag<Task> tc = new ConcurrentBag<Task>();
        int tindex = 1;
        ConcurrentBag<TaskResult> results = new ConcurrentBag<TaskResult>();
        ConcurrentExclusiveSchedulerPair taskSchedulerPair = new ConcurrentExclusiveSchedulerPair();
        public override void ExecuteAsync(string cmd = "")
        {
            tindex = 1;
            results = new ConcurrentBag<TaskResult>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < 1000; i++)
            {
                #region Restsharp
                if (cmd == "r")
                    CallByRestsharp();
                #endregion

                #region Httpclient 
                if (cmd == "h")
                    CallByHttpClient();

                #endregion
            }

            //foreach (var item in tc)
            //{
            //    item.Start();
            //}
            if (Task.WaitAll(tc.ToArray(), 100000))
            {
                sw.Stop();
                //Console.WriteLine($"Total elapsed {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"receivied {results.Count} result  {sw.ElapsedMilliseconds} ms");
            }

        }

        private void CallByRestsharp()
        {
            Task t = new Task((() =>
           {
               //Stopwatch sw = new Stopwatch();
               RestClient client = new RestClient();
               client.BaseUrl = new Uri(uri);
               try
               {
                   Random rnd = new Random(Guid.NewGuid().GetHashCode());
                   var x = rnd.Next(1, 11);
                   var y = rnd.Next(12, 21);
                   QueueModel model = new QueueModel();

                   model.ProcessId = $"Processid:{tindex++}";
                   model.QueueKey = "test";
                   model.X = x;
                   model.Y = y;
                   Console.WriteLine($"ProcessId:{ model.ProcessId} send request");
                   //sw.Start();
                   //client.Timeout = TimeSpan.FromSeconds(30);
                   // 將 data 轉為 json
                   var request = new RestRequest("", Method.POST);
                   request.AddJsonBody(model);
                   // 將轉為 string 的 json 依編碼並指定 content type 存為 httpcontent
                   //HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");
                   //HttpResponseMessage response = client.PostAsync(uri, contentPost).Result;
                   //response.EnsureSuccessStatusCode();
                   var responseBody = client.Post(request);

                   results.Add(JsonConvert.DeserializeObject<TaskResult>(responseBody.Content));
                   //sw.Stop();
                   //Console.WriteLine(responseBody);
                   Console.WriteLine($"{model.ProcessId} receivied result:{responseBody.Content}");
                   //sw.Reset();
               }
               catch (HttpRequestException e)
               {
                   Console.WriteLine("\nException Caught!");
                   Console.WriteLine("Message :{0} ", e.Message);
               }

           }));
            tc.Add(t);
            t.Start();
        }

        private void CallByHttpClient()
        {
            Task t = new Task((async () =>
           {
               //Stopwatch sw = new Stopwatch();
               //HttpClientHandler httpClientHandler = new HttpClientHandler();
               //httpClientHandler.MaxAutomaticRedirections = int.MaxValue;
               //httpClientHandler.MaxRequestContentBufferSize = int.MaxValue;
               //httpClientHandler.MaxConnectionsPerServer = int.MaxValue;

               using (HttpClient client = new HttpClient())
               {
                   try
                   {
                       Random rnd = new Random(Guid.NewGuid().GetHashCode());
                       var x = rnd.Next(1, 11);
                       var y = rnd.Next(12, 21);
                       QueueModel model = new QueueModel();
                       model.ProcessId = $"Processid:{tindex++}";
                       model.QueueKey = "test";
                       model.X = x;
                       model.Y = y;
                       Console.WriteLine($"ProcessId:{model.ProcessId} send request");
                       //sw.Start();
                       // 將 data 轉為 json
                       string json = JsonConvert.SerializeObject(model);
                       // 將轉為 string 的 json 依編碼並指定 content type 存為 httpcontent
                       HttpContent contentPost = new StringContent(json, Encoding.UTF8, "application/json");
                       var response = await client.PostAsync(uri, contentPost);
                       //var response = client.PostAsync(uri, contentPost).Result;

                       //response.EnsureSuccessStatusCode();
                       string responseBody = await response.Content.ReadAsStringAsync();
                       //string responseBody = response.Content.ReadAsStringAsync().Result;
                       results.Add(JsonConvert.DeserializeObject<TaskResult>(responseBody));
                       //sw.Stop();
                       //Console.WriteLine(responseBody);
                       Console.WriteLine($"{model.ProcessId} receivied result:{responseBody}");
                       //sw.Reset();
                   }
                   catch (HttpRequestException e)
                   {
                       Console.WriteLine("\nException Caught!");
                       Console.WriteLine("Message :{0} ", e.Message);
                   }

               }
           }));
            tc.Add(t);
            t.Start(taskSchedulerPair.ConcurrentScheduler);
        }
    }
}
