using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebCrawler.Tools.Dns;

namespace WebCrawler.Tools
{
  public static class ApplicationCache
  {
    public static HashSet<string> VisitedUrls { get; set; } = new HashSet<string>();
    public static Dictionary<string,DnsResponse> Dns { get; set; } = new Dictionary<string, DnsResponse>();
    public static  Dictionary<string,RobotsTextResponse> Robots { get; set; }=new Dictionary<string, RobotsTextResponse>();

    public static void TrimVisitedUrls()
    {
      var quarter = VisitedUrls.Count / 4;
      foreach (var item in VisitedUrls.Take(quarter).ToList())
      {
        VisitedUrls.Remove(item);
      }
      Console.WriteLine("Trimming cached visited urls to 3 quarters");
    }
    public static void TrimRobots()
    {
      var quarter = Robots.Count / 4;
      foreach (var item in Robots.Take(quarter).ToList())
      {
        Robots.Remove(item.Key);
      }
      Console.WriteLine("Trimming robots to 3 quarters");
    }

    public static void TrimDnsCache()
    {
      var countBeforeTrim = Dns.Count;
      var tenth = countBeforeTrim / 10;

      //remove the ones with expired dates first
      foreach (var item in Dns.Where(x=>x.Value.ExpireDate < DateTime.Now).ToList())
      {
        Dns.Remove(item.Key);
      }
      if (Dns.Count != countBeforeTrim)
      {
        Console.WriteLine($"Removed {countBeforeTrim - Dns.Count} expired dns");
      }

      //if cache is still quite full, then trim a little more
      var rest = tenth - (countBeforeTrim - Dns.Count);
      if (rest < 0)
      {
        return;
      }
      foreach (var item in Dns.Take(rest).ToList())
      {
        Dns.Remove(item.Key);
      }
      Console.WriteLine("Trimming DNS to 9 tenths");
    }

  }
}
