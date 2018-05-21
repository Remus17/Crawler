using System;
using WebCrawler.Tools;
using WebCrawler.Tools.Dns;

namespace RIW2
{
  class Program
  {
    static void Main(string[] args)
    {
      RunSettings.Initialize();

      //MakeDnsRequest();
      System.Web.HttpUtility.HtmlDecode("&euro;");
      var siteUrl = "http://riweb.tibeica.com";
      int level = 5;
      var crawler = new Crawler();
      crawler.StartFrom(siteUrl, level);
      //var dns = DnsResolver.GetDnsAddress("www.tuiasi.ro");
      var a = new Uri("http://riweb.tibeica.com/crawl");
    }

   
  }
}
