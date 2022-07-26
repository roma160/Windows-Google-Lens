using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Windows.UI;
using System.Windows.Media;
using Microsoft.Win32;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using Windows_Google_Lens.Utils;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;

namespace Windows_Google_Lens.Views
{
    //https://github.com/riverar/sample-win32-acrylicblur/blob/master/MainWindow.xaml.cs
    public class AcrylicWindow : Window
    {
        public static readonly DependencyProperty BlurColorProperty =
            DependencyProperty.Register("BlurColor", typeof(SolidColorBrush), typeof(AcrylicWindow),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)), (o, args) => ((AcrylicWindow)o).Enable()));
        public SolidColorBrush BlurColor
        {
            get => (SolidColorBrush) GetValue(BlurColorProperty);
            set => SetValue(BlurColorProperty, value);
        }

        public static readonly DependencyProperty BodyBlurColorProperty =
            DependencyProperty.Register("BodyBlurColor", typeof(SolidColorBrush), typeof(AcrylicWindow),
                new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));
        public SolidColorBrush BodyBlurColor { get; set; }

        public AcrylicWindow() => Loaded += OnLoaded;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Style = (Style) FindResource(typeof(AcrylicWindow));
            Enable();
        }

        internal void Enable()
        {
            // https://github.com/riverar/sample-win32-acrylicblur
            var windowHelper = new WindowInteropHelper(this);

            EnableAcrylicBackground(windowHelper);

            if(IsWindows11) EnableRoundBorders(windowHelper);
        }

        private void EnableAcrylicBackground(WindowInteropHelper windowHelper)
        {
            // Creating and initializing ACCENT_POLICY structure
            Color c = BlurColor.Color;
            WinAPI.ACCENT_POLICY accent = new WinAPI.ACCENT_POLICY
            {
                AccentState = WinAPI.ACCENT_STATE.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                GradientColor = (uint)((c.A << 24) | (c.B << 16) | (c.G << 8) | c.R)
            };
            int accentStructSize = Marshal.SizeOf(accent);
            IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            // Creating WINCOMPATTRDATA for the function call
            WinAPI.WINCOMPATTRDATA data = new WinAPI.WINCOMPATTRDATA
            {
                attribute = WinAPI.WINDOWCOMPOSITIONATTRIB.WCA_ACCENT_POLICY,
                dataSize = accentStructSize,
                pData = accentPtr
            };
            WinAPI.SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            // Finishing by freeing the pointers
            Marshal.FreeHGlobal(accentPtr);
        }

        private void EnableRoundBorders(WindowInteropHelper windowHelper)
        {
            // https://github.com/lepoco/wpfui/blob/main/src/Wpf.Ui/Interop/UnsafeNativeMethods.cs
            int pvAttribute = (int)WinAPI.DWM_WINDOW_CORNER_PREFERENCE.ROUND;

            // TODO: Validate HRESULT
            WinAPI.DwmSetWindowAttribute(
                windowHelper.Handle,
                (int)WinAPI.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
                ref pvAttribute,
                Marshal.SizeOf(typeof(int)));
        }

        internal static bool IsWindows11
        { get {
            RegistryKey reg = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            if (reg == null) return false;

            String currentBuildStr = reg.GetValue("CurrentBuild") as String;
            return
                int.TryParse(currentBuildStr, out int currentBuild) &&
                currentBuild >= 22000;
        }}
    }
}
