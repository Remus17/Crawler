using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WebCrawler.Tools
{
  public class FileTreeWorker
  {
    public static void CreateDirectory(string path)
    {
      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
      }
    }

    public static void CleanDirectory(string path)
    {
      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
      }
      DirectoryInfo di = new DirectoryInfo(path);

      foreach (FileInfo file in di.GetFiles())
      {
        file.Delete();
      }
      foreach (DirectoryInfo dir in di.GetDirectories())
      {
        dir.Delete(true);
      }
    }

    public static void CreateWebsiteTree(Uri uri,string html)
    {
      
    }
  }
}
