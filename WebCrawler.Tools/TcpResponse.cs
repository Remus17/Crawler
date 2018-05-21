using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace WebCrawler.Tools
{
  public class TcpResponse
  {
    public string Html { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
  }
}
