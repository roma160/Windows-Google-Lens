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
using ModernWpf.Controls;
using Windows_Google_Lens.Utils;

namespace Windows_Google_Lens.Views
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : AcrylicWindow, IWindowWithClipboardManager
    {
        private ClipboardManager clipboardManager;
        public ClipboardManager ClipboardManager => clipboardManager;

        public DialogWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void AcrylicWindow_Loaded(object sender, RoutedEventArgs e)
        {
            clipboardManager = new ClipboardManager(this);
        }
    }
}
