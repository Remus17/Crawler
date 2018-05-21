using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.Tools
{
  public class UrlFrontier
  {
    public static Queue<string> UrlsQueue { get; set; } = new Queue<string>();

    public static void Enqueue(Uri baseUrl,string anchor)
    {
      var url = GetUrlFromAnchor(baseUrl, anchor);
      if (CanBeSkipped(baseUrl, anchor, url)) return;

      if (UrlsQueue.Count > RunSettings.MaxQueuedUrlsSize)
      {
        //mecanism de asteptare?
        return;
      }
      UrlsQueue.Enqueue(url);
    }

  

    public static bool CanBeSkipped(Uri baseUrl, string anchor, string url)
    {
      if (!url.StartsWith("http") || baseUrl.AbsoluteUri.Equals(anchor, StringComparison.OrdinalIgnoreCase))
      {
        return true;
      }
      if (ApplicationCache.VisitedUrls.Contains(url) || UrlsQueue.Contains(url))
      {
        return true;
      }
      return false;
    }


    public static string GetUrlFromAnchor(Uri baseUrl, string anchor)
    {

      var absoluteUrl = IsAbsoluteUrl(anchor) ? anchor : 
        Uri.TryCreate(baseUrl, anchor, out Uri tempUri) ? tempUri.AbsoluteUri : 
        string.Empty;

      var hashtagIndex = absoluteUrl.IndexOf("#", StringComparison.Ordinal);

      return hashtagIndex >= 0 ? absoluteUrl.Substring(0, hashtagIndex) : absoluteUrl;
    }

    static bool IsAbsoluteUrl(string url)
    {
      return Uri.IsWellFormedUriString(url, UriKind.Absolute);
    }
  }
}
