using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using Windows.ApplicationModel.Store.Preview.InstallControl;

namespace Windows_Google_Lens.Lens
{
    public class SocketWorker
    {
        public class Response
        {
            public int StatusCode;
            public String Head;
            public String Body;

            public override string ToString() => Head + Body;
        }

        public enum Status
        {
            NotConstructed, Ready, UserCertificationError
        }

        public class SocketException : Exception
        {
            public enum Type
            {
                InConstruction,
                InRequestAuthentication
            }

            public readonly Type ExceptionType;

            public SocketException(Type type, String message = null) : base(message) =>
                ExceptionType = type;
        }

        public readonly String Hostname;
        public readonly int Port;
        public readonly bool IsHttps;
        public Status CurrentStatus { get; private set; }

        public int Timeout
        {
            get => SslStream.ReadTimeout;
            set => SslStream.ReadTimeout = value;
        }

        private TcpClient TcpClient;
        private SslStream SslStream;

        private const String LineSep = "\r\n";
        private static readonly Regex ContentLengthRegex = new Regex(
            @"Content-Length:\s*(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ResponseStatusCodeRegex = new Regex(
            @"HTTP/1\.1\s+(\d+)", RegexOptions.Compiled);

        public SocketWorker(String hostname, bool isHttps = true, int timeout = 1000)
        {
            CurrentStatus = Status.NotConstructed;

            Hostname = hostname;
            IsHttps = isHttps;
            if (!IsHttps) throw new NotImplementedException(
                "HTTP isn't implemented, and not going to be soon...");
            Port = isHttps ? 443 : 80;

            TcpClient = new TcpClient(Hostname, Port);
            SslStream = new SslStream(TcpClient.GetStream(), false,
                UserCertificateValidationCallback, null);
            SslStream.ReadTimeout = timeout;

            if (CurrentStatus == Status.NotConstructed)
                CurrentStatus = Status.Ready;
        }

        private bool UserCertificateValidationCallback(
            object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            if (sslpolicyerrors == SslPolicyErrors.None) return true;
            CurrentStatus = Status.UserCertificationError;
            return false;
        }

        public Task<Response> MakePUploadGResult(PUploadGResultProvider provider,
            byte[] imageBytes) => Task.Run(async () =>
        {
            // Setting up the security stuff
            if (CurrentStatus != Status.Ready)
                throw new SocketException(
                    SocketException.Type.InConstruction,
                    "Your instance of worker wasn't properly constructed, check out this part of its lifetime!");

            try { SslStream.AuthenticateAsClient(Hostname); }
            catch (AuthenticationException e)
            {
                TcpClient.Close();
                throw new SocketException(
                    SocketException.Type.InRequestAuthentication,
                    "Wasn't able to authenticate. God knows how to fix this.");
            }
            
            // Making the POST request
            Task<String> queryString = provider.GetQueryString();

            String requestBorder = GenerateBoundary();
            int bodySize = 0;
            String requestBodyBeginning = $"--{requestBorder}{LineSep}Content-Disposition: form-data";
            if (provider.ImageEntryName != null)
                requestBodyBeginning += $"; name=\"{provider.ImageEntryName}\"";
            requestBodyBeginning += $"{LineSep}Content-Type: image/jpeg{LineSep}{LineSep}";

            String requestBodyEnding = $"{LineSep}--{requestBorder}--";

            bodySize += Encoding.UTF8.GetByteCount(requestBodyBeginning) +
                        Encoding.UTF8.GetByteCount(requestBodyEnding);

            switch (provider.EncodingType)
            {
                case PUploadGResultProvider.ImageEncodingType.Raw:
                    break;
                case PUploadGResultProvider.ImageEncodingType.Base64:
                    imageBytes = Encoding.UTF8.GetBytes(Convert.ToBase64String(imageBytes));
                    break;
            }
            bodySize += imageBytes.Length;

            String requestHeader = $"POST {provider.PostPath}?{await queryString} HTTP/1.1{LineSep}" +
                                   $"Host: {provider.PostDomain}{LineSep}" +
                                   $"Content-Length: {bodySize}{LineSep}" +
                                   $"Content-Type: multipart/form-data; boundary={requestBorder}{LineSep}{LineSep}";

            SslStream.Write(Encoding.UTF8.GetBytes(requestHeader));
            SslStream.Write(Encoding.UTF8.GetBytes(requestBodyBeginning));
            SslStream.Write(imageBytes);
            SslStream.Write(Encoding.UTF8.GetBytes(requestBodyEnding));
            SslStream.Flush();


            // Reading the answer
            int bytesReceived;
            byte[] buffer = new byte[1024];

            long bodyLength = -1, bodyIndex;
            StringBuilder responseHeadBuilder = new StringBuilder(),
                responseBodyBuilder = new StringBuilder();

            do {
                bytesReceived = SslStream.Read(buffer, 0, buffer.Length);
                if ((bodyIndex = ContainsEndOfHead(buffer)) != -1)
                {
                    responseBodyBuilder.Append(Encoding.UTF8.GetChars(
                        SubArray(buffer, bodyIndex - 3, bytesReceived - bodyIndex + 3)));
                    Array.Resize(ref buffer, (int) bodyIndex);
                    responseHeadBuilder.Append(Encoding.UTF8.GetChars(buffer));
                    bodyLength = int.Parse(ContentLengthRegex.Match(responseHeadBuilder.ToString()).Groups[1].Value);
                    break;
                }
                responseHeadBuilder.Append(Encoding.UTF8.GetChars(buffer));
            } while(bytesReceived > 1);

            if (bodyLength > responseBodyBuilder.Length)
            {
                byte[] restBytes = new byte[bodyLength - responseBodyBuilder.Length];
                while (bodyLength > responseBodyBuilder.Length)
                {
                    SslStream.Read(restBytes, 0, restBytes.Length);
                    responseBodyBuilder.Append(Encoding.UTF8.GetChars(restBytes));
                }
            }

            TcpClient.Close();

            // Forming the results
            Response response = new Response
            {
                Head = responseHeadBuilder.ToString(),
                Body = responseBodyBuilder.ToString()
            };
            response.StatusCode = int.Parse(ResponseStatusCodeRegex.Match(response.Head).Groups[1].Value);
            return response;
        });

        //TODO: implement real random border generation
        private static String GenerateBoundary(int length = 24) => "e2c09e68db44f08a3a1eead4";

        private static long ContainsEndOfHead(byte[] bytes)
        {
            byte[] bytesToFind = { 0x0d, 0x0a, 0x0d, 0x0a };
            long foundInd = -1;
            for (int i = 0; i < bytes.Length && foundInd == -1; i++)
            {
                if (bytes[i] != bytesToFind[0]) continue;

                int j = 1;
                while (j < bytesToFind.Length &&
                       bytes[i + j] == bytesToFind[j]) j++;
                if (j == bytesToFind.Length)
                    foundInd = i + bytesToFind.Length;
            }

            return foundInd;
        }

        private static T[] SubArray<T>(T[] data, long index, long length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
