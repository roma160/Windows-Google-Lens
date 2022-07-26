using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows_Google_Lens.Utils;

namespace Windows_Google_Lens.Lens
{
    public static class ScreenshotUtils
    {
        private static bool theScreenshotIsCaptured = false;
        public static async Task<bool> CaptureScreenshot(IWindowWithClipboardManager window)
        {
            if (theScreenshotIsCaptured) return false;
            theScreenshotIsCaptured = true;

            bool clipboardHaveChanged = false;
            EventHandler onClipboardChange = (sender, args) => clipboardHaveChanged = true;
            window.ClipboardManager.ClipboardChanged += onClipboardChange;

            void OnCaptureFinish()
            {
                theScreenshotIsCaptured = false;
                window.ClipboardManager.ClipboardChanged -= onClipboardChange;
            }

            // https://github.com/MicrosoftDocs/windows-uwp/blob/docs/windows-apps-src/launch-resume/launch-screen-snipping.md
            bool clipperLaunchResult = await Windows.System.Launcher.LaunchUriAsync(
                new Uri("ms-screenclip:edit?delayInSeconds=0&clippingMode=true"));
            if (!clipperLaunchResult)
            {
                OnCaptureFinish();
                return false;
            }

            Process[] clippingProcesses = Process.GetProcessesByName("ScreenClippingHost");
            if (clippingProcesses.Length == 0)
            {
                OnCaptureFinish();
                return false;
            }
            clippingProcesses[0].WaitForExit();

            OnCaptureFinish();
            return clipboardHaveChanged;
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
