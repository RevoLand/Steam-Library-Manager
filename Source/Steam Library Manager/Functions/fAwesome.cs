using FontAwesome.WPF;
using System.Windows.Controls;

namespace Steam_Library_Manager.Functions
{
    class fAwesome
    {

        public static Image getAwesomeIcon(FontAwesomeIcon fIcon, System.Windows.Media.Brush color)
        {
            Image icon = new Image()
            {
                Source = ImageAwesome.CreateImageSource(fIcon, color),
                Height = 16,
                Width = 16
            };
            return icon;
        }

    }
}
