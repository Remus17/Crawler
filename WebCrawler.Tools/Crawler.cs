using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using HtmlAgilityPack;

namespace WebCrawler.Tools
{
  public class Crawler
  {

    public void StartFrom(string siteUrl)
    {
      ProcessUrl(siteUrl);
      var stopWatch = Stopwatch.StartNew();
      int processedUrls = 0;
      while (UrlFrontier.UrlsQueue.Count!=0)
      {
        var newExternalLink = UrlFrontier.UrlsQueue.Dequeue();
        ProcessUrl(newExternalLink);

        Console.WriteLine($"{Environment.NewLine}--Got url from url queue: {newExternalLink}--{Environment.NewLine}");

        while (UrlFrontier.CurrentWebsiteUrlsQueue.Count!=0)
        {
          var url = UrlFrontier.CurrentWebsiteUrlsQueue.Dequeue();
          Console.WriteLine(url);
          ProcessUrl(url);
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
    }

    private void ProcessUrl(string siteUrl,int retries=0)
    {
      if (retries == RunSettings.MaxRetries)
      {
        return;
      }
      retries++;

      //Console.WriteLine($"Crawling {siteUrl}");
      var baseUrl = new Uri(siteUrl);
      //get robots
      RobotsTextResponse robots = null;
      if (!ApplicationCache.Robots.TryGetValue(baseUrl.Authority, out robots))
      {
        robots = TcpCrawlingClient.GetRobotsResponse(baseUrl);
      }

      if (ApplicationCache.Robots.Count > RunSettings.MaxRobotsSize)
      {
        ApplicationCache.TrimRobots();
      }

      //check robots
      if (!RobotsTextResponse.IsAllowed(robots,baseUrl)) return;

      var page = TcpCrawlingClient.Download(baseUrl);
      if (ApplicationCache.VisitedUrls.Count > RunSettings.MaxVisitedUrls)
      {
        ApplicationCache.TrimVisitedUrls();
      }

      ApplicationCache.VisitedUrls.Add(siteUrl);

      if (page == null)
        return ;
      if ((page.StatusCode == HttpStatusCode.MovedPermanently || page.StatusCode==HttpStatusCode.TemporaryRedirect) && page.Headers.ContainsKey("Location"))
      {
        ProcessRedirectedLocation(page, baseUrl,retries);
        return;
      }

      if (page.StatusCode != HttpStatusCode.OK)
      {
        return;
      }

      var doc = new HtmlDocument();
      doc.LoadHtml(page.Html);
      //no index is returned, no follow means we index this but don't take anchors
      var metaTags = doc.DocumentNode.Descendants("meta").ToList();
      bool containsNoIndex = false, containsNoFollow=false;
      var robotsMeta = metaTags.FirstOrDefault(x => x.Attributes["name"]?.Value == "robots")?.Attributes["content"]?.Value;
      if (robotsMeta != null)
      {
        containsNoFollow = robotsMeta.Contains("nofollow");
        containsNoIndex = robotsMeta.Contains("noindex");
      }


      var anchors = doc.DocumentNode.Descendants("a");
      if (!containsNoIndex)
      {
        //we can save it on the disk so the indexer can do its job
        FileTreeWorker.CreateWebsiteTree(baseUrl, page.Html);
      }
      if (containsNoFollow)
      {
        //do not extract the links
        return;
      }

      foreach (var a in anchors)
      {
        var currentAnchor = a.Attributes["href"]?.Value;
        if (string.IsNullOrEmpty(currentAnchor)) continue;
        var rel = a.Attributes["rel"]?.Value;
        if (rel != null && rel == "nofollow")
        {
          continue;
        }
        UrlFrontier.Enqueue(baseUrl,currentAnchor);
      }
    }

    

    private void ProcessRedirectedLocation(TcpResponse page, Uri baseUrl,int retries)
    {
      var newLocation = page.Headers["Location"];
      var absoluteUrl = UrlFrontier.GetUrlFromAnchor(baseUrl,newLocation)?.AbsoluteUri ?? string.Empty;
      if (!UrlFrontier.CanBeSkipped(absoluteUrl))
      {
        Console.WriteLine($"Processing redirect");
        ProcessUrl(absoluteUrl,retries);
      }
    }

    
  }
}