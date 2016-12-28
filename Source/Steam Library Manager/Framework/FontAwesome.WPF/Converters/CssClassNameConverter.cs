using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace FontAwesome.WPF.Converters
{
    /// <summary>
    /// Converts the CSS class name to a FontAwesomIcon and vice-versa.
    /// </summary>
    public class CssClassNameConverter
        : MarkupExtension, IValueConverter
    {
        private static readonly IDictionary<string, FontAwesomeIcon> ClassNameLookup = new Dictionary<string, FontAwesomeIcon>();
        private static readonly IDictionary<FontAwesomeIcon, string> IconLookup = new Dictionary<FontAwesomeIcon, string>();

        static CssClassNameConverter()
        {
            foreach (var value in Enum.GetValues(typeof(FontAwesomeIcon)))
            {
                var memInfo = typeof(FontAwesomeIcon).GetMember(value.ToString());
                var attributes = memInfo[0].GetCustomAttributes(typeof(IconIdAttribute), false);

                if (attributes.Length == 0) continue; // alias

                var id = ((IconIdAttribute)attributes[0]).Id;

                if (ClassNameLookup.ContainsKey(id)) continue;

                ClassNameLookup.Add(id, (FontAwesomeIcon)value);
                IconLookup.Add((FontAwesomeIcon)value, id);
            }
        }

        /// <summary>
        /// Gets or sets the mode of the converter
        /// </summary>
        public CssClassConverterMode Mode { get; set; }

        private static FontAwesomeIcon FromStringToIcon(object value)
        {
            var icon = value as string;

            if (string.IsNullOrEmpty(icon)) return FontAwesomeIcon.None;
            if (!ClassNameLookup.TryGetValue(icon, out var rValue))
            {
                rValue = FontAwesomeIcon.None;
            }

            return rValue;
        }

        private static string FromIconToString(object value)
        {
            if (!(value is FontAwesomeIcon)) return null;
            IconLookup.TryGetValue((FontAwesomeIcon) value, out string rValue);
            
            return rValue;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Mode == CssClassConverterMode.FromStringToIcon)
                return FromStringToIcon(value);
            
            return FromIconToString(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Mode == CssClassConverterMode.FromStringToIcon)
                return FromIconToString(value);

            return FromStringToIcon(value);
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    /// <summary>
    /// Defines the CssClassNameConverter mode. 
    /// </summary>
    public enum CssClassConverterMode
    {
        /// <summary>
        /// Default mode. Expects a string and converts to a FontAwesomeIcon.
        /// </summary>
        FromStringToIcon = 0,
        /// <summary>
        /// Expects a FontAwesomeIcon and converts it to a string.
        /// </summary>
        FromIconToString
    }
}
