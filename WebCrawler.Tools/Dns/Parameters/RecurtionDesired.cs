using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.Tools.Dns.Parameters
{
  public static class RecurtionDesired
  {
    public static byte False => 0x00;//0
    public static byte True => 0x01;//1
  }
}
