using System;
using System.Windows;
using System.Windows.Media.Imaging;

// Thank you, Jeroen van Langen - http://stackoverflow.com/a/5175424/218882 and Ivan Leonenko - http://stackoverflow.com/a/12638859/218882

namespace Steam_Library_Manager.Framework.CachedImage
{
    /// <summary>
    ///     Represents a control that is a wrapper on System.Windows.Controls.Image for enabling filesystem-based caching
    /// </summary>
    public class Image : System.Windows.Controls.Image
    {
        static Image() => DefaultStyleKeyProperty.OverrideMetadata(typeof(Image), new FrameworkPropertyMetadata(typeof(Image)));

        public string ImageUrl
        {
            get => (string)GetValue(ImageUrlProperty); set => SetValue(ImageUrlProperty, value);
        }

        public BitmapCreateOptions CreateOptions
        {
            get => ((BitmapCreateOptions)(GetValue(CreateOptionsProperty))); set => SetValue(CreateOptionsProperty, value);
        }

        private static async void ImageUrlPropertyChangedAsync(DependencyObject Obj, DependencyPropertyChangedEventArgs e)
        {
            string Url = e.NewValue as string;

            if (string.IsNullOrEmpty(Url))
            {
                return;
            }

            try
            {
                var BitmapImage = new BitmapImage();
                var MemStream = await FileCache.HitAsync(new Uri(Url));

                if (MemStream == null || MemStream.Length == 0)
                {
                    return;
                }

                BitmapImage.BeginInit();
                BitmapImage.StreamSource = MemStream;
                BitmapImage.EndInit();

                ((Image)Obj).Source = BitmapImage;
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.ToString());
                //Debug.WriteLine(ex);
                // ignored, in case the downloaded file is a broken or not an image.
            }
        }

        public static readonly DependencyProperty ImageUrlProperty = DependencyProperty.Register("ImageUrl",
            typeof(string), typeof(Image), new PropertyMetadata("", ImageUrlPropertyChangedAsync));

        public static readonly DependencyProperty CreateOptionsProperty = DependencyProperty.Register("CreateOptions",
            typeof(BitmapCreateOptions), typeof(Image));
    }
}