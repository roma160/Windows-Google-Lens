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
using Microsoft.Win32;
using ModernWpf.Controls;
using Windows_Google_Lens.Lens;
using Windows_Google_Lens.Utils;
using Windows_Google_Lens.Views.Custom;

namespace Windows_Google_Lens.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AcrylicWindow, IWindowWithClipboardManager
    {
        private Provider provider;

        private ClipboardManager clipboardManager;
        public ClipboardManager ClipboardManager => clipboardManager;

        private OpenFileDialog fileDialog;

        public MainWindow()
        {
            InitializeComponent();

            provider = Providers.GoogleLens;

            fileDialog = new OpenFileDialog
            {
                Multiselect = false,
                // https://stackoverflow.com/questions/2069048/setting-the-filter-to-an-openfiledialog-to-allow-the-typical-image-formats
                Filter = "Image Files (*.jpg, *.jpeg, *.png, *.gif, *.tif)|*.jpg;*.jpeg;*.png;*.gif;*.tif",
                Title = "Open file to search:",
                CheckPathExists = true
            };
        }

        private void screenshotSearch_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                if (!await ScreenshotUtils.CaptureScreenshot(this) ||
                    !await ScreenshotUtils.ClipboardHasImage(false))
                    return;

                Task<LoadingWindow> loadingWindow = LoadingWindow.OpenLoadingWindow(
                    "Your screenshot is being proceed.", this);

                Worker.Result result = await Worker.LaunchLens(
                    provider, ScreenshotUtils.GetImageFromClipboard());

                await LoadingWindow.CloseLoadingWindow(await loadingWindow, this);
            });
        }

        private void clipboardSearch_Click(object sender, RoutedEventArgs e)
        {
            //Task.Run(async () =>
            //{
            //    if (!await ScreenshotUtils.ClipboardHasImage()) return;

            //    Task<LoadingWindow> loadingWindow = LoadingWindow.OpenLoadingWindow(
            //        "Your photo is being proceed.", this);

            //    await worker.LaunchLens(
            //        ScreenshotUtils.GetImageFromClipboard());

            //    await LoadingWindow.CloseLoadingWindow(await loadingWindow, this);
            //});
        }

        private void fileSearch_Click(object sender, RoutedEventArgs e)
        {
            //Task.Run(async () =>
            //{
            //    bool? fileDialogResult = Dispatcher.Invoke(() => fileDialog.ShowDialog(this));
            //    if(!fileDialogResult.GetValueOrDefault(false)) return;

            //    Task<LoadingWindow> loadingWindow = LoadingWindow.OpenLoadingWindow(
            //        "Your file is being proceed.", this);

            //    String filePath = fileDialog.FileName;
            //    Task<byte[]> contentsTask = Task.Run(() => File.ReadAllBytes(filePath));

            //    await worker.LaunchLens(contentsTask);

            //    await LoadingWindow.CloseLoadingWindow(await loadingWindow, this);
            //});
        }

        private void MainUIWindow_Loaded(object sender, RoutedEventArgs e)
        {
            clipboardManager = new ClipboardManager(this);
        }

        private void debugButton_Click(object sender, RoutedEventArgs e)
        {
            //Window loadingWindow = null;
            //loadingWindow = new LoadingWindow(
            //    "Your screenshot is being processed.");
            //loadingWindow.Owner = this;
            //loadingWindow.Show();
        }
    }
}
