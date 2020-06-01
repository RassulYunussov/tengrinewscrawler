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

        int _maxUrls;
        HttpClient _client = new HttpClient();
        Regex reg = new Regex(@"a\shref\s*=\s*(?:[""'](?<1>[^""']*)[""']|(?<1>\S+))");
        ConcurrentDictionary<string,Page> _dic = new ConcurrentDictionary<string, Page>();
        public Parser(int maxUrls = 50)
        {
            _maxUrls = maxUrls;
        }
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
                tasks.Add(DownloadPage(url));

            List<Task> subUrls = new List<Task>();
            while(tasks.Count>0) {
                if(_dic.Count>_maxUrls)
                    return;
                var t = await Task.WhenAny(tasks);
                tasks.Remove(t);
                var p = await t;
                if(p!=null&&_dic.TryAdd(p.Url,p))
                    subUrls.Add(crawl(getUrls(p)));
            }
            await Task.WhenAll(subUrls);
        }

        public IEnumerable<string> getUrls(Page page)
        {
            var matches = reg.Matches(page.Content);
            HashSet<string> output = new HashSet<string>();
            foreach(var m in matches)
            {
                string str = m.ToString(); 
                string url = str.Substring(8,str.Length-9);
                if(url.StartsWith("https://tengrinews.kz")||url.StartsWith("https://kaz.tengrinews.kz"))
                    output.Add(url);
                else 
                    output.Add("https://tengrinews.kz"+url);
            }
            return output;
        }
        private async Task<Page> DownloadPage(string url)
        {
            try {
                 var result = await _client.GetStringAsync(url);
                 return new Page() {Url = url, Content = result};
            } catch
            {
                return null;
            }
        }
    }
}