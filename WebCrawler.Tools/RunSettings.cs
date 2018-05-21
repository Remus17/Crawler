using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.Tools
{
  public static class RunSettings
  {
    public static int MaxVisitedUrls { get; private set; }
    public static int MaxQueuedUrlsSize { get; private set; }
    public static int MaxRobotsSize { get; private set; }
    public static string DnsServer{get;private set;}
    public static string UserAgent {get;private set; }
    public static  int ParallelismDegree { get; private set; }
    public static void Initialize()
    {
      UserAgent = "Remus_Balauca_1406A";
      MaxQueuedUrlsSize = 100;
      MaxVisitedUrls = 500;
      MaxRobotsSize = 50;
      DnsServer = "1.1.1.1"; //"192.168.100.1";
      //"5.254.96.195" - bucuresti , "8.8.8.8" - google, "1.1.1.1" , "81.180.223.1" - tuiasi
      ParallelismDegree = 1;
    }
  }
}
