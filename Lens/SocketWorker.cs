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

        //TODO: configure proper exceptions namings to catch all possible network situations
        public class SocketException : Exception
        {
            public enum Type
            {
                InConstruction,
                InConstructionBecNetwork,
                InRequestAuthentication,
                InRequestResponse
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

        public const String LineSep = "\r\n";
        private static readonly Regex ContentLengthRegex = new Regex(
            @"Content-Length:\s*(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ResponseStatusCodeRegex = new Regex(
            @"HTTP/1\.1\s+(\d+)", RegexOptions.Compiled);

        public SocketWorker(String hostname, bool isHttps = true, int timeout = 3000)
        {
            CurrentStatus = Status.NotConstructed;

            Hostname = hostname;
            IsHttps = isHttps;
            if (!IsHttps) throw new NotImplementedException(
                "HTTP isn't implemented, and not going to be soon...");
            Port = isHttps ? 443 : 80;

            try
            {
                TcpClient = new TcpClient(Hostname, Port);
            }
            catch (System.Net.Sockets.SocketException)
            {
                throw new SocketException(SocketException.Type.InConstructionBecNetwork,
                    "Probably that is because you don't have an internet connection.");
            }

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
            {
                TcpClient.Close();
                throw new SocketException(
                    SocketException.Type.InConstruction,
                    "Your instance of worker wasn't properly constructed, check out this part of its lifetime!");
            }

            try { SslStream.AuthenticateAsClient(Hostname); }
            catch (AuthenticationException e)
            {
                TcpClient.Close();
                throw new SocketException(
                    SocketException.Type.InRequestAuthentication,
                    "Wasn't able to authenticate. God knows how to fix this.");
            }
            
            // Making the POST request
            switch (provider.EncodingType)
            {
                case PUploadGResultProvider.ImageEncodingType.Raw:
                    break;
                case PUploadGResultProvider.ImageEncodingType.Base64:
                    imageBytes = Encoding.UTF8.GetBytes(Convert.ToBase64String(imageBytes));
                    break;
            }
            Tuple<String, String> postContents = await provider.GetPostContents(imageBytes.Length);

            SslStream.Write(Encoding.UTF8.GetBytes(postContents.Item1));
            SslStream.Write(imageBytes);
            SslStream.Write(Encoding.UTF8.GetBytes(postContents.Item2));
            SslStream.Flush();


            // Reading the answer
            Tuple<String, String> postResponse = ReadResponse();

            TcpClient.Close();

            if (postResponse == null)
                throw new SocketException(
                    SocketException.Type.InRequestResponse,
                    "Problem is inside SocketWorker class, the request altering is needed!");

            // Forming the results
            return new Response
            {
                Head = postResponse.Item1,
                Body = postResponse.Item2,
                StatusCode = int.Parse(ResponseStatusCodeRegex.Match(
                    postResponse.Item1).Groups[1].Value)
            };
        });

        private Tuple<String, String> ReadResponse()
        {
            byte[] buffer = new byte[1024];
            int newBytesNumber;

            long bodyLengthToReceive = -1, bodyStartIndex;
            StringBuilder responseHeadBuilder = new StringBuilder(),
                responseBodyBuilder = new StringBuilder();
            try
            {
                do
                {
                    newBytesNumber = SslStream.Read(buffer, 0, buffer.Length);
                    if ((bodyStartIndex = ContainsEndOfHead(buffer)) != -1)
                    {
                        responseBodyBuilder.Append(Encoding.UTF8.GetChars(
                            SubArray(buffer, bodyStartIndex - 3, newBytesNumber - bodyStartIndex + 3)));
                        Array.Resize(ref buffer, (int)bodyStartIndex);
                        responseHeadBuilder.Append(Encoding.UTF8.GetChars(buffer));
                        bodyLengthToReceive =
                            int.Parse(ContentLengthRegex.Match(responseHeadBuilder.ToString()).Groups[1].Value);
                        break;
                    }

                    responseHeadBuilder.Append(Encoding.UTF8.GetChars(buffer));
                } while (newBytesNumber > 1);

                if (bodyLengthToReceive > responseBodyBuilder.Length)
                {
                    byte[] restBytes = new byte[bodyLengthToReceive - responseBodyBuilder.Length];
                    while (bodyLengthToReceive > responseBodyBuilder.Length)
                    {
                        SslStream.Read(restBytes, 0, restBytes.Length);
                        responseBodyBuilder.Append(Encoding.UTF8.GetChars(restBytes));
                    }
                }
            }
            catch (IOException)
            { return null; }
            catch (SocketException)
            { return null; }

            return new Tuple<String, String>(
                responseHeadBuilder.ToString(), responseBodyBuilder.ToString());
        }

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
