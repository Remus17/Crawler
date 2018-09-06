using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebCrawler.Tools
{
  public static class RunSettings
  {
    public static int MaxVisitedUrls { get; private set; } = 500;
    public static int MaxQueuedUrlsSize { get; private set; } = 100;
    public static int MaxRobotsSize { get; private set; } = 50;
    public static int MaxDnsSize { get; private set; } = 50;
    public static string DnsServer { get; private set; } = "1.1.1.1";
    public static string UserAgent { get; private set; } = "Remus_Balauca_1406A";
    public static string SavingLocationPath { get; private set; } = Path.Combine(Environment.CurrentDirectory,"crawled");
    public static int ParallelismDegree { get; private set; } = 1;
    public static int MaxRetries { get; private set; } = 3;
    public static void Initialize()
    {
      UserAgent = "RIWEB_CRAWLER";
      MaxQueuedUrlsSize = 200;
      MaxVisitedUrls = 1000;
      MaxRobotsSize = 200;
      MaxDnsSize = 200;
      DnsServer = "1.1.1.1"; //"192.168.100.1";
      //"5.254.96.195" - bucuresti , "8.8.8.8" - google, "1.1.1.1" , "81.180.223.1" - tuiasi
      ParallelismDegree = 1;
    }
  }
}
