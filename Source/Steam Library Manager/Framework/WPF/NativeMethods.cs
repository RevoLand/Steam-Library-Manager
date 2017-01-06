using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Xml;
using System.Xml.Serialization;

// https://blogs.msdn.microsoft.com/davidrickard/2010/03/08/saving-window-size-and-location-in-wpf-and-winforms/

namespace Steam_Library_Manager.Framework
{
    public class NativeMethods
    {
        public static class WindowPlacement
        {
            private static Encoding encoding = new UTF8Encoding();
            private static XmlSerializer serializer = new XmlSerializer(typeof(WINDOWPLACEMENT));

            [DllImport("user32.dll")]
            private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

            [DllImport("user32.dll")]
            private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

            private const int SW_SHOWNORMAL = 1;
            private const int SW_SHOWMINIMIZED = 2;

            // RECT structure required by WINDOWPLACEMENT structure
            [Serializable]
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;

                public RECT(int left, int top, int right, int bottom)
                {
                    this.Left = left;
                    this.Top = top;
                    this.Right = right;
                    this.Bottom = bottom;
                }
            }

            // POINT structure required by WINDOWPLACEMENT structure
            [Serializable]
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int X;
                public int Y;

                public POINT(int x, int y)
                {
                    this.X = x;
                    this.Y = y;
                }
            }

            // WINDOWPLACEMENT stores the position, size, and state of a window
            [Serializable]
            [StructLayout(LayoutKind.Sequential)]
            public struct WINDOWPLACEMENT
            {
                public int length;
                public int flags;
                public int showCmd;
                public POINT minPosition;
                public POINT maxPosition;
                public RECT normalPosition;
            }

            public static void SetPlacement(Window window, string placementXml)
            {
                if (string.IsNullOrEmpty(placementXml))
                {
                    return;
                }

                WINDOWPLACEMENT placement;
                IntPtr windowHandle = new WindowInteropHelper(window).Handle;
                byte[] xmlBytes = encoding.GetBytes(placementXml);

                try
                {
                    using (MemoryStream memoryStream = new MemoryStream(xmlBytes))
                    {
                        placement = (WINDOWPLACEMENT)serializer.Deserialize(memoryStream);
                    }

                    placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                    placement.flags = 0;
                    placement.showCmd = (placement.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : placement.showCmd);
                    SetWindowPlacement(windowHandle, ref placement);
                }
                catch (InvalidOperationException)
                {
                    // Parsing placement XML failed. Fail silently.
                }
            }

            public static string GetPlacement(Window window)
            {
                WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
                GetWindowPlacement(new WindowInteropHelper(window).Handle, out placement);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8))
                    {
                        serializer.Serialize(xmlTextWriter, placement);
                        byte[] xmlBytes = memoryStream.ToArray();
                        return encoding.GetString(xmlBytes);
                    }
                }
            }
        }
    }
}