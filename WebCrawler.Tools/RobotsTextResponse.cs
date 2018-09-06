using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebCrawler.Tools
{

  public class UserAgent
  {
    public HashSet<string> AllowedUrls { get; set; } = new HashSet<string>();
    public HashSet<string> DisallowedUrls { get; set; } = new HashSet<string>();
    public string Name { get; set; }
  }

  public class RobotsTextResponse
  {
    public UserAgent AnyAgent { get; set; }
    public UserAgent MyAgent { get; set; }

    public static bool IsAllowed(RobotsTextResponse robots, Uri baseUrl)
    {
      if (robots == null)
      {
        return true;
      }

      var allowed = true;
      //check my agent first
      if (robots.MyAgent != null)
      {
        allowed = IsAllowedUserAgent(robots.MyAgent, baseUrl);
      }
      else if (robots.AnyAgent != null)
      {
        allowed = IsAllowedUserAgent(robots.AnyAgent, baseUrl);
      }

      return allowed;
    }

    private static bool IsAllowedUserAgent(UserAgent userAgent, Uri baseUrl)
    {
      var sb = new StringBuilder();
      var allowed = true;
      string dissallowedUrl = null;
      foreach (var segment in baseUrl.Segments)
      {
        sb.Append(segment);
        var currentString = sb.Length > 1 && sb[sb.Length - 1] == '/' ? sb.ToString(0, sb.Length - 1) : sb.ToString();
        dissallowedUrl = userAgent.DisallowedUrls.FirstOrDefault(x => x == currentString);
        if (dissallowedUrl != null)
        {
          allowed = false;
          break;
        }
      }
      if (!allowed)
      {
        var allowedUrls = userAgent.AllowedUrls.Where(x => x.StartsWith(dissallowedUrl)).ToList();
        foreach (var url in allowedUrls)
        {
          var segments = url.Substring(dissallowedUrl.Length);
          if (baseUrl.AbsolutePath.Contains(segments))
          {
            allowed = true;
            break;
          }
        }
      }
      return allowed;
    }

    public static RobotsTextResponse ParseText(string text)
    {
      var response = new RobotsTextResponse();
      var startedParsing = false;
      UserAgent currentAgent = null;
      int index = 0;
      while (index < text.Length)
      {
        var line = GetLine(text, ref index);
        if (line.Length == 0)
        {
          if (startedParsing)
          {
            startedParsing = false;
          }
          continue;
        }

        if (line.StartsWith("User-agent:") && line.Length > 12 && (line[12] == '*' || IsMatchingUserAgent(line)))
        {
          if (line[12] == '*')
          {
            response.AnyAgent = new UserAgent();
            currentAgent = response.AnyAgent;
            currentAgent.Name = "*";
            startedParsing = true;
          }
          else if (IsMatchingUserAgent(line))
          {
            response.MyAgent = new UserAgent();
            currentAgent = response.MyAgent;
            currentAgent.Name = line.Substring(12);
            startedParsing = true;
          }
          else
          {
            continue;
          }
        }
        if (!startedParsing)
        {
          continue;
        }


        if (line.StartsWith("Disallow:") && line.Length > 10)
        {
          currentAgent.DisallowedUrls.Add(line.Substring(10));
        }
        else if (line.StartsWith("Allow:") && line.Length > 7)
        {
          currentAgent.AllowedUrls.Add(line.Substring(7));
        }
      }
      return response;
    }

    private static bool IsMatchingUserAgent(string line)
    {
      var isMatch = true;
      for (int i = 12, j = 0; i < line.Length && j < RunSettings.UserAgent.Length; i++, j++)
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
          index = ++i;
          return sb.ToString();
        }
      }
      index = text.Length;
      return sb.ToString();
    }
  }
}
