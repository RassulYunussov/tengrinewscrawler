using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Linq;
using System.Diagnostics;

namespace parser
{
    public class Parser : IParser
    {
        HttpClient _client = new HttpClient();
        Regex reg = new Regex("a href=(\"|')(https://tengrinews.kz)(.*?)(\"|')");
        ConcurrentDictionary<string,Page> _dic = new ConcurrentDictionary<string, Page>();
        public async Task<Dictionary<string, Page>> crawlUrls(IEnumerable<string> urls)
        {
            await crawl(urls);
            var dic = _dic;
            _dic = new ConcurrentDictionary<string, Page>();
            return dic.ToDictionary(kvp=>kvp.Key, kvp=>kvp.Value);
        }
       
        private async Task crawl(IEnumerable<string> urls)
        {
            List<Task<Page>> tasks= new List<Task<Page>>();
            foreach(var url in urls.Except(_dic.Keys)) 
            {
                tasks.Add(DownloadPage(url));
            }
            while(tasks.Count>0) {
                var t = await Task.WhenAny(tasks);
                tasks.Remove(t);
                var p = await t;
                if(_dic.TryAdd(p.Url,p))
                    await crawl(getUrls(p));
            }
        }

        public IEnumerable<string> getUrls(Page page)
        {
            var matches = reg.Matches(page.Content);
            List<string> output = new List<string>();
            foreach(var m in matches)
            {
                string str = m.ToString(); 
                string url = str.Substring(8,str.Length-9);
                output.Add(url);
            }
            return output;
        }
        private async Task<Page> DownloadPage(string url)
        {
            var result = await _client.GetStringAsync(url);
            return new Page() {Url = url, Content = result};
        }
    }
}