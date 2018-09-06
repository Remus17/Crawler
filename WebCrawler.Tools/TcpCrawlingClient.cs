using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using WebCrawler.Tools.Dns;

namespace WebCrawler.Tools
{
  public class TcpCrawlingClient
  {
    public static TcpResponse Download(Uri uri)
    {
      ApplicationCache.Dns.TryGetValue(uri.Authority, out DnsResponse dns);
      if (dns == null)
      {
        dns = DnsResolver.GetDnsAddress(uri.Authority);
        if (dns == null) return null;
        ApplicationCache.Dns.Add(uri.Authority, dns);
      }
      else if (dns.ExpireDate < DateTime.Now)
      {
        ApplicationCache.Dns.Remove(uri.Authority);
        dns = DnsResolver.GetDnsAddress(uri.Authority);
        if (dns == null) return null;
        ApplicationCache.Dns.Add(uri.Authority, dns);
      }
      if (ApplicationCache.Dns.Count > RunSettings.MaxDnsSize)
      {
        ApplicationCache.TrimDnsCache();
      }

      var relativePath = uri.AbsolutePath;
      var requestMessage = new StringBuilder();
      requestMessage.Append($"GET {relativePath} HTTP/1.1\r\n");
      requestMessage.Append($"Host:{uri.Authority}\r\n"); //riweb.tibeica.com
      requestMessage.Append($"User-Agent:{RunSettings.UserAgent}\r\n");
      requestMessage.Append("Connection:close\r\n");
      //requestMessage.Append("Accept: text/html,application/xhtml+xml,application/xml;q=0.9\r\n");
      //requestMessage.Append("Accept-encoding: gzip, deflate\r\n");
      requestMessage.Append("\r\n");
      var client = GetTcpClient(uri, dns);
      if (client == null)
      {
        return null;
      }
      var sendBuffer = Encoding.ASCII.GetBytes(requestMessage.ToString());
      var stream = GetStream(client, uri);
      stream.Write(sendBuffer, 0, sendBuffer.Length);

      var response = Encoding.Default.GetString(ReadFully(stream));
      return BuildTcpResponse(response);
    }


    public static RobotsTextResponse GetRobotsResponse(Uri uri, int retries = 0, string cacheKey = null)
    {
      if (cacheKey == null)
      {
        //in case of redirects, keep same key
        cacheKey = uri.Authority;
      }

      if (retries == RunSettings.MaxRetries)
      {
        ApplicationCache.Robots.Add(cacheKey, null);
        return null;
      }
      retries++;
      var robots = new Uri($"{uri.Scheme}://{uri.Authority}/robots.txt");
      var page = Download(robots);
      if (page == null)
      {
        ApplicationCache.Robots.Add(cacheKey, null);
        return null;
      }
      if (page.StatusCode != HttpStatusCode.OK && page.StatusCode != HttpStatusCode.NotModified)
      {
        if ((page.StatusCode == HttpStatusCode.Moved || page.StatusCode == HttpStatusCode.Redirect
             && page.StatusCode == HttpStatusCode.TemporaryRedirect) && page.Headers.ContainsKey("Location"))
        {
          return GetRobotsResponse(new Uri(page.Headers["Location"]), retries, cacheKey);
        }
        ApplicationCache.Robots.Add(cacheKey, null);
        return null;
      }
      var parsedRobots = RobotsTextResponse.ParseText(page.Html);
      ApplicationCache.Robots.Add(cacheKey, parsedRobots);
      return parsedRobots;
    }

    private static TcpClient GetTcpClient(Uri uri, DnsResponse dns)
    {
      TcpClient client = null;
      var port = 80;
      if (uri.Scheme.Length == 5)
      {
        port = 443;
      }
      try
      {
        client = new TcpClient(dns.IpAddress, port);
      }
      catch (SocketException ex)
      {
        Console.WriteLine(
          $"Could not connect to lookedup dns {dns.IpAddress} for {uri.Authority}.{Environment.NewLine}Reason: {ex.Message}{Environment.NewLine}Skipping...");
        return client;
      }
      return client;
    }

    private static Stream GetStream(TcpClient client, Uri uri)
    {
      Stream stream = client.GetStream();
      if (uri.Scheme.Length == 5)
      {
        stream = new SslStream(stream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate));
        ((SslStream)stream).AuthenticateAsClient(uri.Authority);
      }
      return stream;
    }

    private static byte[] ReadFully(Stream stream, int initialLength = 0)
    {
      // If we've been passed an unhelpful initial length, just
      // use 32K.
      if (initialLength < 1)
      {
        initialLength = 32768;
      }

      byte[] buffer = new byte[initialLength];
      int read = 0;

      int chunk;
      while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
      {
        read += chunk;

        // If we've reached the end of our buffer, check to see if there's
        // any more information
        if (read == buffer.Length)
        {
          int nextByte = stream.ReadByte();

          // End of stream? If so, we're done
          if (nextByte == -1)
          {
            return buffer;
          }

          // Nope. Resize the buffer, put in the byte we've just
          // read, and continue
          byte[] newBuffer = new byte[buffer.Length * 2];
          Array.Copy(buffer, newBuffer, buffer.Length);
          newBuffer[read] = (byte)nextByte;
          buffer = newBuffer;
          read++;
        }
      }
      // Buffer is now too big. Shrink it.
      byte[] ret = new byte[read];
      Array.Copy(buffer, ret, read);
      return ret;
    }
    private static TcpResponse BuildTcpResponse(string downloaded)
    {
      var response = new TcpResponse();
      var headers = GetHeaders(downloaded, out int index);
      response.Html = index < downloaded.Length ?  downloaded.Substring(index, downloaded.Length - index) : string.Empty;
      response.StatusCode = (HttpStatusCode)Convert.ToInt32(headers[0].Substring(9, 3));
      for (int i = 1; i < headers.Count - 1; i++)
      {
        var items = headers[i].Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
        response.Headers[items[0]] = items[1];
      }
      return response;
    }


    private static List<string> GetHeaders(string downloaded, out int index)
    {
      var headers = new List<string>();
      var newLine = new StringBuilder();
      int i;
      for (i = 0; i < downloaded.Length; i++)
      {
        if (downloaded[i] == '\r' && downloaded[i + 1] == '\n')
        {
          if (i+3 < downloaded.Length && downloaded[i + 2] == '\r' && downloaded[i + 3] == '\n') break;
          headers.Add(newLine.ToString());
          newLine.Clear();
        }
        else if (downloaded[i] != '\n')
        {
          newLine.Append(downloaded[i]);
        }
      }
      index = i + 4;
      return headers;
    }

    private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
      return true;
    }
  }
}
