using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Windows_Google_Lens.Views.Custom;

namespace Windows_Google_Lens.Views
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : AcrylicWindow
    {
        public LoadingWindow(String loadingText)
        {
            InitializeComponent();

            textBlock.Text = loadingText;
        }

        public static async Task<LoadingWindow> OpenLoadingWindow(String title, Window parentWindow)
        {
            LoadingWindow loadingWindow = null;
            await parentWindow.Dispatcher.InvokeAsync(() =>
            {
                loadingWindow = new LoadingWindow(title);
                loadingWindow.Owner = parentWindow;
                loadingWindow.Show();
            });
            return loadingWindow;
        }

        public static async Task CloseLoadingWindow(LoadingWindow loadingWindow, Window parentWindow) =>
            await parentWindow.Dispatcher.InvokeAsync(loadingWindow.Close);
    }
}
