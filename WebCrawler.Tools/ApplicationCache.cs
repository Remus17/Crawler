using System;
using System.Collections.Generic;
using System.Text;
using WebCrawler.Tools.Dns;

namespace WebCrawler.Tools
{
  public static class ApplicationCache
  {
    public static HashSet<string> VisitedUrls { get; set; } = new HashSet<string>();
    public static Dictionary<string,DnsResponse> Dns { get; set; } = new Dictionary<string, DnsResponse>();
    public static  Dictionary<string,RobotsTextResponse> Robots { get; set; }=new Dictionary<string, RobotsTextResponse>();
  }
}
