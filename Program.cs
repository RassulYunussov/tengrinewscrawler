using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace parser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Parser p = new Parser();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var r = await p.crawlUrls(new string []{"https://tengrinews.kz"});
            sw.Stop();
            System.Console.WriteLine(r.Keys.Count);
            System.Console.WriteLine("Elapsed:"+sw.Elapsed.TotalSeconds);
        }
    }
}
