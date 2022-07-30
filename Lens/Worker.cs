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
    public class Worker
    {
        public enum ResultType
        {
            NetworkErrorOccured,
            RequestErrorOccured,
            AlgorithmError,
            LinkObtained,
            LaunchedSuccessfully,
            TaskCancelled
        }
        public struct Result
        {
            public ResultType ResultType;
            public String Data;
        }

        private readonly HttpClient httpClient;

        public Worker() => httpClient = new HttpClient();

        public async Task<Result> LaunchLens(
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
            if (result.ResultType != ResultType.LinkObtained)
                return result;

            LaunchLinkInBrowser(result);
            return result;
        }

        private static void LaunchLinkInBrowser(Result result) => Process.Start(result.Data);

        private async Task<Result> PUploadGResult(
            PUploadGResultProvider provider, Task<byte[]> imageBytes)
        {
            // Preparing data to be sent
            Task<String> queryString = provider.GetQueryString();
            Task<MultipartFormDataContent> formData = GetFormData(provider, imageBytes);

            // Making Post request
            HttpResponseMessage response;
            try
            {
                response = await httpClient.PostAsync(
                    $"{provider.PostUrl}?{await queryString}", await formData);
            }
            catch (HttpRequestException e)
            {
                return new Result
                {
                    ResultType = ResultType.NetworkErrorOccured,
                    Data = e.Message
                };
            }

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                return new Result
                {
                    ResultType = ResultType.RequestErrorOccured,
                    Data = e.Message
                };
            }
            
            String link = provider.GetGetLink(await response.Content.ReadAsStringAsync());
            if (link == null)
                return new Result
                {
                    ResultType = ResultType.AlgorithmError
                };

            return new Result
            {
                ResultType = ResultType.LinkObtained,
                Data = link
            };
        }

        private static async Task<MultipartFormDataContent> GetFormData(
            PUploadGResultProvider provider, Task<byte[]> imageBytes)
        {
            HttpContent encodedImage = null;

            switch(provider.EncodingType){
                case PUploadGResultProvider.ImageEncodingType.Raw:
                    encodedImage = new ByteArrayContent(await imageBytes);
                    break;
                case PUploadGResultProvider.ImageEncodingType.Base64:
                    encodedImage = new StringContent(Convert.ToBase64String(await imageBytes));
                    break;
            }

            encodedImage.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            MultipartFormDataContent formData = new MultipartFormDataContent();
            formData.Add(encodedImage, provider.ImageEntryName);

            formData.
            return formData;
        }
    }
}
