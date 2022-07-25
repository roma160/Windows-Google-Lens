using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Google_Lens.Utils
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public static class WinAPI
    {
        // https://github.com/microsoft/Windows.UI.Composition-Win32-Samples/issues/84

        /// <summary>
        /// The type of window background used
        /// </summary>
        public enum ACCENT_STATE
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_INVALID_STATE = 5
        }

        /// <summary>
        /// The data, that is being passed during the window background change
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ACCENT_POLICY
        {
            public ACCENT_STATE AccentState;
            public uint AccentFlags;
            public uint GradientColor;
            public uint AnimationId;
        }

        /// <summary>
        /// Types used by SetWindowCompositionAttribute and GetWindowCompositionAttribute
        /// </summary>
        public enum WINDOWCOMPOSITIONATTRIB
        {
            WCA_UNDEFINED = 0,
            WCA_NCRENDERING_ENABLED = 1,
            WCA_NCRENDERING_POLICY = 2,
            WCA_TRANSITIONS_FORCEDISABLED = 3,
            WCA_ALLOW_NCPAINT = 4,
            WCA_CAPTION_BUTTON_BOUNDS = 5,
            WCA_NONCLIENT_RTL_LAYOUT = 6,
            WCA_FORCE_ICONIC_REPRESENTATION = 7,
            WCA_EXTENDED_FRAME_BOUNDS = 8,
            WCA_HAS_ICONIC_BITMAP = 9,
            WCA_THEME_ATTRIBUTES = 10,
            WCA_NCRENDERING_EXILED = 11,
            WCA_NCADORNMENTINFO = 12,
            WCA_EXCLUDED_FROM_LIVEPREVIEW = 13,
            WCA_VIDEO_OVERLAY_ACTIVE = 14,
            WCA_FORCE_ACTIVEWINDOW_APPEARANCE = 15,
            WCA_DISALLOW_PEEK = 16,
            WCA_CLOAK = 17,
            WCA_CLOAKED = 18,
            WCA_ACCENT_POLICY = 19,
            WCA_FREEZE_REPRESENTATION = 20,
            WCA_EVER_UNCLOAKED = 21,
            WCA_VISUAL_OWNER = 22,
            WCA_HOLOGRAPHIC = 23,
            WCA_EXCLUDED_FROM_DDA = 24,
            WCA_PASSIVEUPDATEMODE = 25,
            WCA_LAST = 26
        }
        // http://undoc.airesoft.co.uk/user32.dll/GetWindowCompositionAttribute.php
        [StructLayout(LayoutKind.Sequential)]
        public struct WINCOMPATTRDATA
        {
            /// <summary>
            /// A flag describing which value to set, specified as a value of the <c>WINDOWCOMPOSITIONATTRIB</c> enumeration.
            /// </summary>
            public WINDOWCOMPOSITIONATTRIB attribute;
            /// <summary>
            ///  pointer to an object containing the attribute value to set. 
            /// </summary>
            public IntPtr pData;
            /// <summary>
            /// The size, in bytes, of the attribute value being set via the <c>pData</c> parameter.
            /// </summary>
            public int dataSize;
        }

        /// <summary>
        ///Sets various information regarding DWM (Composition) window attributes
        /// </summary>
        /// <remarks>
        /// As it turns out, despite the thing written on http://undoc.airesoft.co.uk/user32.dll/SetWindowCompositionAttribute.php,
        /// this function is different to the DwmSetWindowAttribute, and even gets its own params.
        /// </remarks>
        /// <param name="hwnd">The window whose information is to be changed</param>
        /// <param name="data">Pointer to a structure which both specifies and delivers the attribute data</param>
        /// <returns>If the function succeeds, it returns <c>S_OK</c>. Otherwise, it returns an <c>HRESULT</c> error code.</returns>
        [DllImport("user32.dll")]
        public static extern int SetWindowCompositionAttribute(
            IntPtr hwnd, ref WINCOMPATTRDATA data);


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

        /// <summary>
        /// Sets the value of Desktop Window Manager (DWM) non-client rendering attributes for a window.
        /// </summary>
        /// <param name="hWnd">The handle to the window for which the attribute value is to be set.</param>
        /// <param name="dwAttribute">A flag describing which value to set, specified as a value of the DWMWINDOWATTRIBUTE enumeration.</param>
        /// <param name="pvAttribute">A pointer to an object containing the attribute value to set.</param>
        /// <param name="cbAttribute">The size, in bytes, of the attribute value being set via the <c>pvAttribute</c> parameter.</param>
        /// <returns>If the function succeeds, it returns <c>S_OK</c>. Otherwise, it returns an <c>HRESULT</c> error code.</returns>
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(
            [In] IntPtr hWnd, [In] int dwAttribute, 
            [In] ref int pvAttribute, [In] int cbAttribute);
    }
}
