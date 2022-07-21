using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
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
using Windows.System;

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

        private async Task<bool> CaptureScreenshot()
        {
            bool result = await Windows.System.Launcher.LaunchUriAsync(
                new Uri("ms-screenclip:edit?delayInSeconds=0&clippingMode=true"));
            if (!result) return false;

            Process clippingProcess = Process.GetProcessesByName("ScreenClippingHost")[0];
            clippingProcess.WaitForExit();
            return true;
        }

        private byte[] GetImageFromClipboard()
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = 100;

            byte[] res;
            using (MemoryStream stream = new MemoryStream())
            {
                var image = Clipboard.GetImage();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
                res = stream.ToArray();
            }

            return res;
        }

        private async void launchGoogleLens(byte[] imageBytes)
        {
            MultipartFormDataContent formData = new MultipartFormDataContent();
            ByteArrayContent encoded_image = new ByteArrayContent(imageBytes);
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

        private async void scrennshotSearch_Click(object sender, RoutedEventArgs e)
        {
            if(!await CaptureScreenshot() || !Clipboard.ContainsImage()) return;
            
            launchGoogleLens(GetImageFromClipboard());
        }
    }
}
