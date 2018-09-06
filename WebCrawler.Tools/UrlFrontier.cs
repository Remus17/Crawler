using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebCrawler.Tools
{
  public class UrlFrontier
  {
    public static String[] AvailableFormats { get; set; } = {"html", "htm", "php", "asp", "aspx", "jsp"};

    public static Queue<string> CurrentWebsiteUrlsQueue { get; set; } = new Queue<string>();

    public static Queue<string> UrlsQueue { get; set; } = new Queue<string>();

    public static void Enqueue(Uri baseUrl,string anchor)
    {
      var hashTagIndex = anchor.IndexOf('#');
      if (hashTagIndex >= 0)
      {
        anchor = anchor.Substring(0, hashTagIndex);
      }
     
      if (IsAbsoluteUrl(anchor))
      {
        if (UrlsQueue.Count > RunSettings.MaxQueuedUrlsSize || CanBeSkipped(anchor) || !AvailableFormat(new Uri(anchor).Segments.Last()))
        {
          return;
        }
        
        if (anchor.Contains(baseUrl.Authority))
        {
          CurrentWebsiteUrlsQueue.Enqueue(anchor);
        }
        else
        {
          var newUri= new Uri(anchor);
          var absoluteUri = $"{newUri.Scheme}://{newUri.Authority}";
          if (!CanBeSkipped(absoluteUri))
          {
            //enqueue only home page, the crawler will download only what they have exposed on their website
            UrlsQueue.Enqueue(absoluteUri);
          }
        }
        return;
      }

      if (!AvailableFormat(anchor))
      {
        return;
      }
      var url = GetUrlFromAnchor(baseUrl, anchor);
      if (url == null)
      {
        return;
      }
      var absoluteUrl = url.AbsoluteUri;

      if (CanBeSkipped(absoluteUrl) || !AvailableFormat(url.Segments.Last())) return;

      //if (CurrentWebsiteUrlsQueue.Count > RunSettings.MaxQueuedUrlsSize)
      //{
      //  return;
      //}
      CurrentWebsiteUrlsQueue.Enqueue(absoluteUrl);
    }

    private static bool AvailableFormat(string anchor)
    {
      var dotIndex = anchor.IndexOf('.');
      if (dotIndex < 0)
      {
        return true;
      }
      return AvailableFormats.Any(format => anchor.EndsWith(format));
    }


    public static bool CanBeSkipped(string url)
    {
      if (!url.StartsWith("http"))
      {
        return true;
      }
      if (ApplicationCache.VisitedUrls.Contains(url) || CurrentWebsiteUrlsQueue.Contains(url) || UrlsQueue.Contains(url) )
      {
        return true;
      }
      return false;
    }


    public static Uri GetUrlFromAnchor(Uri baseUrl, string anchor)
    {

      Uri.TryCreate(baseUrl, anchor, out Uri tempUri);
      return tempUri;
    }

    static bool IsAbsoluteUrl(string url)
    {
      return Uri.IsWellFormedUriString(url, UriKind.Absolute);
    }

  }

}
