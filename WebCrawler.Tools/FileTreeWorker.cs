using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
      var websiteLocation = Path.Combine(RunSettings.SavingLocationPath, uri.Authority);
      CreateDirectory(websiteLocation);
      string currentLocation = string.Empty;
      if (uri.Segments.Length == 1)
      {
        currentLocation = Path.Combine(websiteLocation, "index");
        CreateDirectory(currentLocation);

      }

      foreach (var segment in uri.Segments.Skip(1))
      {
        if (segment == "/")
        {
          continue;
        }
        if (currentLocation.Length==0)
        {
          currentLocation = Path.Combine(websiteLocation, segment);
        }
        else
        {
          currentLocation = Path.Combine(currentLocation, segment);
        }
        CreateDirectory(currentLocation);

      }
      using (StreamWriter writeText = new StreamWriter($"{currentLocation}\\0.txt"))
      {
        writeText.Write(html);
      }
    }
  }
}
