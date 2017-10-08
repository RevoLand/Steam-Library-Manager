using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace FontAwesome.WPF.Converters
{
    /// <summary>
    /// Converts a FontAwesomIcon to its description.
    /// </summary>
    public class DescriptionConverter
        : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is FontAwesomeIcon))
            {
                return null;
            }

            var icon = (FontAwesomeIcon) value;

            var memInfo = typeof(FontAwesomeIcon).GetMember(icon.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length == 0)
            {
                return null; // alias
            }

            return ((DescriptionAttribute)attributes[0]).Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
