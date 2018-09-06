using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using WebCrawler.Tools.Dns.Parameters;

namespace WebCrawler.Tools.Dns
{
  public class DnsResolver
  {

    public static DnsResponse GetDnsAddress(string host)
    {
      var requestSize = 12 + host.Length + 2 + 4; // header + url size +2 + qtype & qclass
      var requestBytes = new byte[requestSize];

      //ID - 2 bytes
      requestBytes[0] = 0x17;
      requestBytes[1] = 0x04;
      //flags
      requestBytes[2] = (byte)(QrFlag.Request | OperationCode.Query | AnswerFlag.NonAuthoritative | TruncationFlag.False | RecurtionDesired.True);
      requestBytes[3] = (byte)(RecurtionAvailable.False | Zero.Reserved | AuthenticationData.False | CheckingDisabled.False | ResponseCode.NoError);
      //total questions count
      requestBytes[4] = 0x00;
      requestBytes[5] = 0x01;
      //total answers count
      requestBytes[6] = 0x00;
      requestBytes[7] = 0x00;
      //total authority record count
      requestBytes[8] = 0x00;
      requestBytes[9] = 0x00;
      //total additional record count
      requestBytes[10] = 0x00;
      requestBytes[11] = 0x00;

      var idx = 12;

      foreach (var item in host.Split('.'))
      {
        requestBytes[idx++] = (byte)item.Length;
        for (int i = 0; i < item.Length; i++)
        {
          requestBytes[idx++] = (byte)item[i];
        }
      }
      requestBytes[idx++] = 0x00; //end of hostname
      //qtype
      requestBytes[idx++] = 0x00;
      requestBytes[idx++] = 0x01; //only ANAME
      //qclass
      requestBytes[idx++] = 0x00;
      requestBytes[idx] = 0x01;


      var address = new IPEndPoint(IPAddress.Parse(RunSettings.DnsServer), 53);
      var responseBytes = new byte[512];

      var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
      socket.SendTo(requestBytes, address);
      socket.Receive(responseBytes);

      bool isSuccessful = (responseBytes[3] & 0x0F) == ResponseCode.NoError;
      if (!isSuccessful)
        return null;
      var answerCount = (responseBytes[6] << 8 ) | responseBytes[7];
      if (answerCount < 1)
      {
        return null;
      }
      idx = requestSize; //after request data we have the response
      //we extract the ip address so we don't need the offset
      while (idx<responseBytes.Length)
      {
        var offset = ((responseBytes[idx]<<8) | responseBytes[idx + 1]) & 0x3FFF;
        bool skip = false;
        for (int i = offset; requestBytes.Length>i && requestBytes[i] != 0; i++)
        {
          if (requestBytes[i] != responseBytes[i])
          {
            skip = true;
            break;
          }
        }
        idx += 2;
        var type = (responseBytes[idx] << 8) | responseBytes[idx + 1];
        if (type != 1)
        {
          skip = true;
          //Console.WriteLine("Non ipv4 address");
        }
        idx += 4; //skip name, type and class 2+2+2


        var ttl = (responseBytes[idx] << 24) | (responseBytes[idx + 1] << 16) | (responseBytes[idx + 2] << 8) | responseBytes[idx + 3];
        var expireDate = DateTime.Now.AddSeconds(ttl);
        idx += 4;

        //since we extract IPV4 addresses, the length is 4 bytes
        var rdLength = (responseBytes[idx] << 8) | responseBytes[idx + 1];
        idx += 2;
        if (!skip)
        {
          var firstAname = new StringBuilder();
          for (int i = idx, end = idx + 4; i < end; i++)
          {
            var segment = (int)responseBytes[i];
            firstAname.Append($"{segment}.");
          }
          return new DnsResponse()
          {
            ExpireDate = expireDate,
            IpAddress = firstAname.ToString(0, firstAname.Length - 1) //remove last dot
          };
        }
        idx += rdLength;

      }

      return null;
    }

  }
}
