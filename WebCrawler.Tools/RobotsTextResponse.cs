using System;
using System.Collections.Generic;
using System.Text;

namespace WebCrawler.Tools
{
  public class RobotsTextResponse
  {
    public HashSet<string> DisallowedUrls { get; set; } = new HashSet<string>();
    public HashSet<string> AllowedUrls { get; set; } = new HashSet<string>();
    public bool AppUserAgentAllowed { get; set; } 

    public static RobotsTextResponse ParseText(string text)
    {
      var response = new RobotsTextResponse();
      //redundant but makes more undersandable
      response.AppUserAgentAllowed = false; 

      int index = 0;
      while (index<text.Length)
      {
        var line = GetLine(text, ref index);
        if (line.StartsWith("User-agent:") && line.Length > 12 && (line[12] == '*' || IsMatchingUserAgent(line)))
        {
          response.AppUserAgentAllowed = true;
        }
        else if (line.StartsWith("Disallow:") && line.Length>10)
        {
          response.DisallowedUrls.Add(line.Substring(10));
        }
        else if (line.StartsWith("Allow:") && line.Length>7)
        {
          response.AllowedUrls.Add(line.Substring(7));
        }
      }
      return response;
    }

    private static bool IsMatchingUserAgent(string line)
    {
      var isMatch = true;
      for (int i = 12,j=0; i < line.Length && j<RunSettings.UserAgent.Length; i++,j++)
      {
        if (line[i] != RunSettings.UserAgent[j])
        {
          isMatch = false;
          break;
        }
      }
      return isMatch;
    }

    private static string GetLine(string text, ref int index)
    {
      var sb = new StringBuilder();
      for (int i = index; i < text.Length; i++)
      {
        if (text[i] != '\n')
        {
          sb.Append(text[i]);
        }
        else
        {
          index = i;
          return sb.ToString();
        }
      }
      index = text.Length;
      return sb.ToString();
    }
  }
}
