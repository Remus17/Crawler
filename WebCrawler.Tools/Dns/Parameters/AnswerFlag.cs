using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.Tools.Dns.Parameters
{
  public static class AnswerFlag
  {
    public static byte NonAuthoritative => 0x00;
    public static byte Authoritative => 0x04;
  }
}
