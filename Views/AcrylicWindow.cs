using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Windows.UI;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;

namespace Windows_Google_Lens.Views
{
    //https://github.com/riverar/sample-win32-acrylicblur/blob/master/MainWindow.xaml.cs
    public class AcrylicWindow : Window
    {
        #region ENUMS

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_INVALID_STATE = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public uint AccentFlags;
            public uint GradientColor;
            public uint AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }

        /// <summary>
        /// Options used by the DwmGetWindowAttribute and DwmSetWindowAttribute functions.
        /// <para><see href="https://github.com/electron/electron/issues/29937"/></para>
        /// </summary>
        [Flags]
        public enum DWMWINDOWATTRIBUTE
        {
            /// <summary>
            /// Is non-client rendering enabled/disabled
            /// </summary>
            DWMWA_NCRENDERING_ENABLED = 1,

            /// <summary>
            /// DWMNCRENDERINGPOLICY - Non-client rendering policy
            /// </summary>
            DWMWA_NCRENDERING_POLICY = 2,

            /// <summary>
            /// Potentially enable/forcibly disable transitions
            /// </summary>
            DWMWA_TRANSITIONS_FORCEDISABLED = 3,

            /// <summary>
            /// Enables content rendered in the non-client area to be visible on the frame drawn by DWM.
            /// </summary>
            DWMWA_ALLOW_NCPAINT = 4,

            /// <summary>
            /// Retrieves the bounds of the caption button area in the window-relative space.
            /// </summary>
            DWMWA_CAPTION_BUTTON_BOUNDS = 5,

            /// <summary>
            /// Is non-client content RTL mirrored
            /// </summary>
            DWMWA_NONCLIENT_RTL_LAYOUT = 6,

            /// <summary>
            /// Forces the window to display an iconic thumbnail or peek representation (a static bitmap), even if a live or snapshot representation of the window is available.
            /// </summary>
            DWMWA_FORCE_ICONIC_REPRESENTATION = 7,

            /// <summary>
            /// Designates how Flip3D will treat the window.
            /// </summary>
            DWMWA_FLIP3D_POLICY = 8,

            /// <summary>
            /// Gets the extended frame bounds rectangle in screen space
            /// </summary>
            DWMWA_EXTENDED_FRAME_BOUNDS = 9,

            /// <summary>
            /// Indicates an available bitmap when there is no better thumbnail representation.
            /// </summary>
            DWMWA_HAS_ICONIC_BITMAP = 10,

            /// <summary>
            /// Don't invoke Peek on the window.
            /// </summary>
            DWMWA_DISALLOW_PEEK = 11,

            /// <summary>
            /// LivePreview exclusion information
            /// </summary>
            DWMWA_EXCLUDED_FROM_PEEK = 12,

            /// <summary>
            /// Cloaks the window such that it is not visible to the user.
            /// </summary>
            DWMWA_CLOAK = 13,

            /// <summary>
            /// If the window is cloaked, provides one of the following values explaining why.
            /// </summary>
            DWMWA_CLOAKED = 14,

            /// <summary>
            /// Freeze the window's thumbnail image with its current visuals. Do no further live updates on the thumbnail image to match the window's contents.
            /// </summary>
            DWMWA_FREEZE_REPRESENTATION = 15,

            /// <summary>
            /// BOOL, Updates the window only when desktop composition runs for other reasons
            /// </summary>
            DWMWA_PASSIVE_UPDATE_MODE = 16,

            /// <summary>
            /// BOOL, Allows the use of host backdrop brushes for the window.
            /// </summary>
            DWMWA_USE_HOSTBACKDROPBRUSH = 17,

            /// <summary>
            /// Allows a window to either use the accent color, or dark, according to the user Color Mode preferences.
            /// </summary>
            DMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19,

            /// <summary>
            /// Allows a window to either use the accent color, or dark, according to the user Color Mode preferences.
            /// </summary>
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,

            /// <summary>
            /// Controls the policy that rounds top-level window corners.
            /// <para>Windows 11 and above.</para>
            /// </summary>
            DWMWA_WINDOW_CORNER_PREFERENCE = 33,

            /// <summary>
            /// The color of the thin border around a top-level window.
            /// </summary>
            DWMWA_BORDER_COLOR = 34,

            /// <summary>
            /// The color of the caption.
            /// <para>Windows 11 and above.</para>
            /// </summary>
            DWMWA_CAPTION_COLOR = 35,

            /// <summary>
            /// The color of the caption text.
            /// <para>Windows 11 and above.</para>
            /// </summary>
            DWMWA_TEXT_COLOR = 36,

            /// <summary>
            /// Width of the visible border around a thick frame window.
            /// <para>Windows 11 and above.</para>
            /// </summary>
            DWMWA_VISIBLE_FRAME_BORDER_THICKNESS = 37,

            /// <summary>
            /// Allows to enter a value from 0 to 4 deciding on the imposed backdrop effect.
            /// </summary>
            DWMWA_SYSTEMBACKDROP_TYPE = 38,

            /// <summary>
            /// Indicates whether the window should use the Mica effect.
            /// <para>Windows 11 and above.</para>
            /// </summary>
            DWMWA_MICA_EFFECT = 1029
        }

        /// <summary>
        /// Flags used by the DwmSetWindowAttribute function to specify the rounded corner preference for a window.
        /// </summary>
        [Flags]
        public enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DEFAULT = 0,
            DONOTROUND = 1,
            ROUND = 2,
            ROUNDSMALL = 3
        }

        #endregion

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        /// <summary>
        /// Sets the value of Desktop Window Manager (DWM) non-client rendering attributes for a window.
        /// </summary>
        /// <param name="hWnd">The handle to the window for which the attribute value is to be set.</param>
        /// <param name="dwAttribute">A flag describing which value to set, specified as a value of the DWMWINDOWATTRIBUTE enumeration.</param>
        /// <param name="pvAttribute">A pointer to an object containing the attribute value to set.</param>
        /// <param name="cbAttribute">The size, in bytes, of the attribute value being set via the <c>pvAttribute</c> parameter.</param>
        /// <returns>If the function succeeds, it returns <c>S_OK</c>. Otherwise, it returns an <c>HRESULT</c> error code.</returns>
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute([In] IntPtr hWnd, [In] int dwAttribute,
            [In] ref int pvAttribute,
            [In] int cbAttribute);

        private uint _blurOpacity = 0x20;
        public double BlurOpacity
        {
            get => _blurOpacity;
            set { _blurOpacity = (uint)value; EnableBlur(); }
        }

        private Color _backgroundColor = Colors.White;
        public new Color Background
        {
            get => _backgroundColor;
            set { _backgroundColor = value; EnableBlur(); }
        }

        public AcrylicWindow() : base()
        {
            ResizeMode = ResizeMode.NoResize;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;

            SetValue(WindowHelper.UseModernWindowStyleProperty, true);
            SetValue(TitleBar.ExtendViewIntoTitleBarProperty, true);

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();
        }

        internal void EnableBlur()
        {
            // https://github.com/riverar/sample-win32-acrylicblur
            var windowHelper = new WindowInteropHelper(this);

            Color color = _backgroundColor;
            AccentPolicy accent = new AccentPolicy
            {
                AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                GradientColor = (_blurOpacity << 24) | (uint) ((color.B << 16) | (color.G << 8) | color.R)
            };

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);


            // https://github.com/lepoco/wpfui/blob/main/src/Wpf.Ui/Interop/UnsafeNativeMethods.cs
            int pvAttribute = (int) DWM_WINDOW_CORNER_PREFERENCE.ROUND;

            // TODO: Validate HRESULT
            DwmSetWindowAttribute(
                windowHelper.Handle,
                (int)DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
                ref pvAttribute,
                Marshal.SizeOf(typeof(int)));

            Marshal.FreeHGlobal(accentPtr);
        }
    }
}
