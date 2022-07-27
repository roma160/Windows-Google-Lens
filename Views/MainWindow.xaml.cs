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
        private readonly Worker worker;

        private ClipboardManager clipboardManager;
        public ClipboardManager ClipboardManager => clipboardManager;

        private OpenFileDialog fileDialog;

        public MainWindow()
        {
            InitializeComponent();

            worker = new Worker();

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

                worker.LaunchGoogleLens(
                    ScreenshotUtils.GetImageFromClipboard());
            });
        }

        private void clipboardSearch_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                if (!await ScreenshotUtils.ClipboardHasImage()) return;

                worker.LaunchGoogleLens(
                    ScreenshotUtils.GetImageFromClipboard());
            });
        }

        private void fileSearch_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                bool? fileDialogResult = Dispatcher.Invoke(() => fileDialog.ShowDialog(this));
                if(!fileDialogResult.GetValueOrDefault(false)) return;

                String filePath = fileDialog.FileName;
                Task<byte[]> contentsTask = Task.Run(() => File.ReadAllBytes(filePath));

                worker.LaunchGoogleLens(contentsTask);
            });
        }

        private void MainUIWindow_Loaded(object sender, RoutedEventArgs e)
        {
            clipboardManager = new ClipboardManager(this);
        }
    }
}
