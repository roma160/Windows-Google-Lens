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
        private const String lensUrl = "https://lens.google.com/upload";
        private readonly HttpClient httpClient;
        private readonly Regex resultUrlRegex;

        public Worker()
        {
            httpClient = new HttpClient();
            resultUrlRegex = new Regex(
                @"https:\/\/lens\.google\.com\/search\?p=[^\""]+",
                RegexOptions.Compiled);
        }

        public async Task LaunchGoogleLens(Task<byte[]> imageBytes)
        {
            MultipartFormDataContent formData = new MultipartFormDataContent();

            String queryString = await new FormUrlEncodedContent(
                new Dictionary<String, String>{
                    { "ep", "ccm" },
                    { "s", "csp" },
                    { "st", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() }
                }
            ).ReadAsStringAsync();

            ByteArrayContent encoded_image = new ByteArrayContent(await imageBytes);
            encoded_image.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            formData.Add(encoded_image, "encoded_image");

            HttpResponseMessage response = await httpClient.PostAsync(
                $"{lensUrl}?{queryString}", formData);
            response.EnsureSuccessStatusCode();
            String response_text = await response.Content.ReadAsStringAsync();

            MatchCollection matches = resultUrlRegex.Matches(response_text);

            System.Diagnostics.Process.Start(matches[0].Value);
        }
    }
}
