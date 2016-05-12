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
            get { return (string)GetValue(ImageUrlProperty); }
            set { SetValue(ImageUrlProperty, value); }
        }

        public BitmapCreateOptions CreateOptions
        {
            get { return ((BitmapCreateOptions)(base.GetValue(Image.CreateOptionsProperty))); }
            set { base.SetValue(Image.CreateOptionsProperty, value); }
        }

        private static async void ImageUrlPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var url = new Uri(e.NewValue as string);

            var cachedImage = (Image)obj;
            var bitmapImage = new BitmapImage();
            try
            {
                var memoryStream = await FileCache.HitAsync(url);
                if (memoryStream == null)
                    return;

                bitmapImage.BeginInit();
                bitmapImage.CreateOptions = cachedImage.CreateOptions;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                cachedImage.Source = bitmapImage;
            }
            catch (Exception)
            {
                // ignored, in case the downloaded file is a broken or not an image.
            }
        }

        public static readonly DependencyProperty ImageUrlProperty = DependencyProperty.Register("ImageUrl",
            typeof(string), typeof(Image), new PropertyMetadata("", ImageUrlPropertyChanged));

        public static readonly DependencyProperty CreateOptionsProperty = DependencyProperty.Register("CreateOptions",
            typeof(BitmapCreateOptions), typeof(Image));
    }
}