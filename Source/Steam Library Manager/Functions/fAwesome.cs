using FontAwesome.WPF;
using System.Windows.Controls;

namespace Steam_Library_Manager.Functions
{
    class fAwesome
    {

        public static Image getAwesomeIcon(FontAwesomeIcon fIcon, System.Windows.Media.Brush color)
        {
            Image icon = new Image();
            icon.Source = ImageAwesome.CreateImageSource(fIcon, color);
            icon.Height = 16;
            icon.Width = 16;

            return icon;
        }

    }
}
