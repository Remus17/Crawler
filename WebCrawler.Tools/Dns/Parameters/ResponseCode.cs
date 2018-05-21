using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.Tools.Dns.Parameters
{
  public  static class ResponseCode
  {
    public static byte NoError => 0x00;
    public static byte FormatError => 0x01;
    public static byte ServerFailure => 0x02;
    public static byte NameError => 0x04;
    public static byte Refused => 0x05;
    public static byte YxDomain=> 0x06;
    public static byte YxRrSet=> 0x07;
    public static byte NxRrSet=> 0x08;
    public static byte NotAuth => 0x09;
    public static byte NotZone => 0x0A;



  }
}
