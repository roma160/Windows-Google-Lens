using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Windows_Google_Lens.Lens
{
    public class SocketWorker
    {
        public SocketWorker()
        {
            
        }

        public static Task<String> GetResponce() => Task.Run(() =>
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            Socket ListenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            byte[] fileBytes = File.ReadAllBytes("C:/buff/img.jpg");
            String dataString = $@"--e2c09e68db44f08a3a1eead4
Content-Disposition: form-data; name=""imageBin""
Content-Type: image/jpeg

{Convert.ToBase64String(fileBytes)}
--e2c09e68db44f08a3a1eead4--";
            int contentLength = dataString.Length;
            String requestString = $@"POST /images/detail/search?iss=sbiupload&FORM=ANCMS1 HTTP/1.1
Host: www.bing.com
Content-Length: {contentLength}
Content-Type: multipart/form-data; boundary=e2c09e68db44f08a3a1eead4

{dataString}
";


            ListenerSocket.ReceiveTimeout = 1000;
            ListenerSocket.Connect(new DnsEndPoint("www.bing.com", 443));
            ListenerSocket.Send(Encoding.UTF8.GetBytes(requestString));

            int delta = 0;
            String result = "";
            int counter = 0;
            byte[] buffer = new byte[1024];
            String strBuffer = "";
            do
            {
                try
                {
                    ListenerSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                }
                catch(SocketException e)
                {
                    break;
                }
                strBuffer = Encoding.UTF8.GetString(buffer);
                result += strBuffer;
                delta = strBuffer.Length;
                counter++;
            } while (delta > 0 && counter < 4);

            Regex r = new Regex(@"Location:\s*([^\r\n]+)");
            Process.Start("https://www.bing.com" + r.Matches(result)[0].Groups[1].Value);

            return result;
        });
    }
}
