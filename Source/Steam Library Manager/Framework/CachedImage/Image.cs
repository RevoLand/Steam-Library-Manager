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
        static Image()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Image),
                new FrameworkPropertyMetadata(typeof(Image)));
        }

        public string ImageUrl
        {
            get => (string)GetValue(ImageUrlProperty); set => SetValue(ImageUrlProperty, value);
        }

        public BitmapCreateOptions CreateOptions
        {
            get => ((BitmapCreateOptions)(GetValue(Image.CreateOptionsProperty))); set => SetValue(Image.CreateOptionsProperty, value);
        }

        private static async void ImageUrlPropertyChangedAsync(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            string url = e.NewValue as string;

            if (string.IsNullOrEmpty(url))
                return;

            try
            {
                var cachedImage = (Image)obj;
                var bitmapImage = new BitmapImage();
                var memoryStream = await FileCache.HitAsync(new Uri(url));

                if (memoryStream == null || memoryStream.Length == 0)
                    return;

                bitmapImage.BeginInit();
                bitmapImage.CreateOptions = cachedImage.CreateOptions;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();

                cachedImage.Source = bitmapImage;
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