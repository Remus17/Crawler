﻿using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.Tools.Dns.Parameters
{
  public static class CheckingDisabled
  {
    public static byte False => 0x00;
    public static byte True => 0x10;
  }
}
