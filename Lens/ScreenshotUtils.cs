using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Windows_Google_Lens.Lens
{
    public static class ScreenshotUtils
    {
        public static async Task<bool> CaptureScreenshot()
        {
            bool result = await Windows.System.Launcher.LaunchUriAsync(
                new Uri("ms-screenclip:edit?delayInSeconds=0&clippingMode=true"));
            if (!result) return false;

            Process clippingProcess = Process.GetProcessesByName("ScreenClippingHost")[0];
            clippingProcess.WaitForExit();
            return true;
        }

        private static byte[] clipboardImageWorker()
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
        public static async Task<byte[]> GetImageFromClipboard() =>
            await Application.Current.Dispatcher.InvokeAsync(clipboardImageWorker);

        public static async Task<bool> ClipboardHasImage(bool showErrorMessage = true)
        {
            if (Clipboard.ContainsImage()) return true;
            if (!showErrorMessage) return false;
            await Application.Current.Dispatcher.InvokeAsync(() =>
                MessageBox.Show(
                    "There was no images inside clipboard!", 
                    "Error", MessageBoxButton.OK,MessageBoxImage.Error)
                );
            return false;
        }
    }
}
