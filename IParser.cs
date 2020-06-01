using System.Collections.Generic;
using System.Threading.Tasks;

namespace parser
{
    public interface IParser
    {
        IEnumerable<string> getUrls(Page page);
        Task<Dictionary<string, Page>> crawlUrls(IEnumerable<string> urls);
    }
}