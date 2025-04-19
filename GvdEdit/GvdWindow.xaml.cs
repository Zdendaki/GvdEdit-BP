using SkiaSharp.Views.Desktop;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace GvdEdit
{
    /// <summary>
    /// Interakční logika pro GvdWindow.xaml
    /// </summary>
    public partial class GvdWindow : Window
    {
        internal bool AllowClose = false;

        private readonly GvdPainter _painter;

        const uint MF_BYCOMMAND = 0x00000000;
        const uint MF_GRAYED = 0x00000001;
        const uint SC_CLOSE = 0xF060;

        [LibraryImport("user32.dll", EntryPoint = "GetSystemMenu")]
        private static partial nint GetSystemMenu(nint hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);

        [LibraryImport("user32.dll", EntryPoint = "EnableMenuItem")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool EnableMenuItem(nint hMenu, uint uIDEnableItem, uint uEnable);

        public GvdWindow()
        {
            InitializeComponent();
            _painter = new();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            nint hwnd = new WindowInteropHelper(this).Handle;

            if (hwnd == nint.Zero)
                throw new InvalidOperationException("Window handle is not available.");

            nint hmenu = GetSystemMenu(hwnd, false);

            if (hmenu == nint.Zero)
                return;

            EnableMenuItem(hmenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = !AllowClose;

            if (AllowClose)
                _painter.Dispose();

            base.OnClosing(e);
        }

        private void SKGLElement_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            _painter.DrawGvd(e.Surface.Canvas);
        }

        private void GvdCanvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GvdCanvas.InvalidateVisual();
        }
    }
}
