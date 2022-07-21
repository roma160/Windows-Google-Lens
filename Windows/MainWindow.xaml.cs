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
using Windows_Google_Lens.Lens;

namespace Windows_Google_Lens
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Worker worker;

        public MainWindow()
        {
            InitializeComponent();

            worker = new Worker();
        }

        private void scrennshotSearch_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                if (!await ScreenshotUtils.CaptureScreenshot() ||
                    !await ScreenshotUtils.ClipboardHasImage())
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
            
        }
    }
}
