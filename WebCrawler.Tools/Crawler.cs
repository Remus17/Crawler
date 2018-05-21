using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HtmlAgilityPack;

namespace WebCrawler.Tools
{
  public class Crawler
  {

    public void StartFrom(string siteUrl, int level = 2)
    {
      ProcessUrl(siteUrl);
      var stopWatch = Stopwatch.StartNew();
      int processedUrls = 0;
      while (UrlFrontier.UrlsQueue.Count!=0)
      {
        ProcessUrl(UrlFrontier.UrlsQueue.Dequeue());
        processedUrls++;
        if (processedUrls == 100)
        {
          stopWatch.Stop();
          Console.WriteLine($"Downloaded 100 pages in {stopWatch.Elapsed.TotalSeconds} seconds.{Environment.NewLine}");
          processedUrls = 0;
          stopWatch = Stopwatch.StartNew();
        }
      }
    }

    private void ProcessUrl(string siteUrl)
    {
      Console.WriteLine($"Crawling {siteUrl}");
      var baseUrl = new Uri(siteUrl);
      var robots = TcpCrawlingClient.GetRobotsResponse(baseUrl);
      var page = TcpCrawlingClient.Download(baseUrl);
      if (ApplicationCache.VisitedUrls.Count > RunSettings.MaxVisitedUrls)
      {
        TrimCache();
      }

      ApplicationCache.VisitedUrls.Add(siteUrl);

      if (page == null)
        return ;
      if (page.StatusCode == HttpStatusCode.MovedPermanently && page.Headers.ContainsKey("Location"))
      {
        ProcessRedirectedLocation(page, baseUrl);
        return;
      }

      if (page.StatusCode != HttpStatusCode.OK)
      {
        return;
      }

      var doc = new HtmlDocument();
      doc.LoadHtml(page.Html);
      var anchors = doc.DocumentNode.Descendants("a");
      FileTreeWorker.CreateWebsiteTree(baseUrl, page.Html);

      foreach (var a in anchors)
      {
        var currentAnchor = a.Attributes["href"]?.Value;
        if (string.IsNullOrEmpty(currentAnchor)) continue;
        UrlFrontier.Enqueue(baseUrl,currentAnchor);
      }
    }

    private void ProcessRedirectedLocation(TcpResponse page, Uri baseUrl)
    {
      var newLocation = page.Headers["Location"];
      var absoluteUrl = UrlFrontier.GetUrlFromAnchor(baseUrl,newLocation);
      if (!UrlFrontier.CanBeSkipped(baseUrl, newLocation, absoluteUrl))
      {
        Console.WriteLine($"Processing a 301 Redirect");
        ProcessUrl(absoluteUrl);
      }
    }

    private static void TrimCache()
    {
      var half = ApplicationCache.VisitedUrls.Count / 2;
      foreach (var item in ApplicationCache.VisitedUrls.Take(half).ToList())
      {
        ApplicationCache.VisitedUrls.Remove(item);
      }
      Console.WriteLine("Trimming cached visited urls to half");
    }
  }
}
