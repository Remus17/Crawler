using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.Tools.Dns.Parameters
{
  public static class QrFlag
  {
    public static byte Request => 0x00;

    public static byte Response => 0x80; //1000 0000
  }
}
