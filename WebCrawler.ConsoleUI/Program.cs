using System;
using System.Collections.Generic;
using WebCrawler.Tools;
using WebCrawler.Tools.Dns;

namespace RIW2
{
  class Program
  {
    static void Main(string[] args)
    {
      RunSettings.Initialize();
      FileTreeWorker.CleanDirectory(RunSettings.SavingLocationPath);
      //var siteUrl = "http://riweb.tibeica.com";
      var siteUrl = "http://riweb.tibeica.com";


      var crawler = new Crawler();
      crawler.StartFrom(siteUrl);


      //var dns = DnsResolver.GetDnsAddress("www.tuiasi.ro");
      //var a = new Uri("http://riweb.tibeica.com/crawl");
      //var d = new Dictionary<string,object>();
      //d.Add("fas",null);
      //object o;
      //var ok = d.TryGetValue("ffas", out o);
      //Console.WriteLine($"is null {o==null} success :{ok} ");
    }

   
  }
}
