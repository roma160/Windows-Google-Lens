using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Windows_Google_Lens
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const String lensUrl = "https://lens.google.com/upload";
        private readonly HttpClient httpClient;
        private readonly Regex resultUrlRegex;

        public MainWindow()
        {
            InitializeComponent();

            httpClient = new HttpClient();
            resultUrlRegex = new Regex(
                @"https:\/\/lens\.google\.com\/search\?p=[^\""]+",
                RegexOptions.Compiled);
        }

        private async void debugButton_Click(object sender, RoutedEventArgs e)
        {
            MultipartFormDataContent formData = new MultipartFormDataContent();
            ByteArrayContent encoded_image = new ByteArrayContent(
                File.ReadAllBytes("C:/buff/img.jpg"));
            encoded_image.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            formData.Add(encoded_image, "encoded_image");

            String queryString = await new FormUrlEncodedContent(
                new Dictionary<String, String>{
                    { "ep", "ccm" },
                    { "s", "csp" },
                    { "st", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() }
                }
            ).ReadAsStringAsync();

            HttpResponseMessage response = await httpClient.PostAsync(
                $"{lensUrl}?{queryString}", formData);
            response.EnsureSuccessStatusCode();
            String response_text = await response.Content.ReadAsStringAsync();

            MatchCollection matches = resultUrlRegex.Matches(response_text);

            System.Diagnostics.Process.Start(matches[0].Value);
        }
    }
}
