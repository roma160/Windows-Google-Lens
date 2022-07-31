using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Windows_Google_Lens.Lens
{
    public static class Worker
    {
        public struct Result
        {
            public enum Type
            {
                NetworkErrorOccurred,
                AuthenticationErrorOccurred,
                RequestErrorOccurred,
                ResponseErrorOccurred,
                AlgorithmError,
                LinkObtained,
                LaunchedSuccessfully,
                TaskCancelled
            }

            public Type ResultType;
            public String Data;

            public override string ToString() => $"Workers result ({ResultType}):\n{Data}";
        }

        public static async Task<Result> LaunchLens(
            Provider provider, Task<byte[]> imageBytes)
        {
            Task<Result> linkTask = null;
            switch (provider)
            {
                case PUploadGResultProvider pprovider:
                    linkTask = PUploadGResult(pprovider, imageBytes);
                    break;
            }

            Result result = await linkTask;
            if (result.ResultType != Result.Type.LinkObtained)
                return result;

            LaunchLinkInBrowser(result);
            return result;
        }

        private static void LaunchLinkInBrowser(Result result) => Process.Start(result.Data);

        private static async Task<Result> PUploadGResult(
            PUploadGResultProvider provider, Task<byte[]> imageBytes)
        {
            SocketWorker socketWorker;
            try
            {
                socketWorker = new SocketWorker(provider.PostDomain);
            }
            catch (SocketWorker.SocketException e)
            {
                return new Result
                {
                    ResultType = Result.Type.NetworkErrorOccurred,
                    Data = e.Message
                };
            }

            // Making Post request
            if (socketWorker.CurrentStatus != SocketWorker.Status.Ready)
                return new Result { ResultType = Result.Type.AuthenticationErrorOccurred };

            SocketWorker.Response response;
            try
            {
                response = await socketWorker.MakePUploadGResult(provider, await imageBytes);
            }
            catch (SocketWorker.SocketException e)
            {
                return new Result
                {
                    ResultType = Result.Type.RequestErrorOccurred,
                    Data = e.Message
                };
            }

            if(response.StatusCode != provider.PostSuccessStatusCode)
                return new Result
                { ResultType = Result.Type.ResponseErrorOccurred };

            String link = provider.GetGetLink(response.Body);
            if (link == null)
                return new Result
                {
                    ResultType = Result.Type.AlgorithmError
                };

            return new Result
            {
                ResultType = Result.Type.LinkObtained,
                Data = link
            };
        }
    }
}
