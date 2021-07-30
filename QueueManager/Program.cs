using QueueManager.Case;
using QueueManager.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueManager
{
    class Program
    {
        static void Main(string[] args)
        {
            
            //var c = new Case5();
            var c = new Case4();
            // var c = new Case3();
            //var c = new Case2();
            //var c = new Case1();
            c.Execute();

            var ss = Console.ReadLine();
            //while ( true)
            //{
            //    var ss = Console.ReadLine();
            //    if (ss == "q")
            //        break;
            //    c.Execute();

            //}
        }

    }
}
