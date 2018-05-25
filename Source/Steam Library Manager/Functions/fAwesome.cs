using FontAwesome.WPF;
using System.Windows.Controls;

namespace Steam_Library_Manager.Functions
{
    internal static class FAwesome
    {
        public static Image GetAwesomeIcon(FontAwesomeIcon fIcon, System.Windows.Media.Brush color) => new Image()
        {
            Source = ImageAwesome.CreateImageSource(fIcon, color),
            Height = 16,
            Width = 16
        };
    }
}