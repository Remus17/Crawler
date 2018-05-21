using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace WebCrawler.Tools.Dns
{
  public class DnsResponse
  {
    public string IpAddress { get; set; }
    public DateTime ExpireDate { get; set; }
  }
}
