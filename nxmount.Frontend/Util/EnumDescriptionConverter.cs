using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nxmount.Frontend.Util
{
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType().IsEnum)
            {
                return ((Enum)value).ToDescription();
            }
            throw new ArgumentException("Convert:Value must be an enum.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is EnumDescription enumDescription)
            {
                return enumDescription.Value;
            }
            throw new ArgumentException("ConvertBack:EnumDescription must be an enum.");
        }
    }
}
