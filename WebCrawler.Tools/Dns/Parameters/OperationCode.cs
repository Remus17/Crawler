using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.Tools.Dns.Parameters
{
  public static class OperationCode
  {
    public static byte Query => 0x00; //0
    public static byte InverseQuery => 0x08; //1
    public static byte Status => 0x10;//2
    public static byte Notify => 0x20;//4
    public static byte Update => 0x28;//5
  }
}
