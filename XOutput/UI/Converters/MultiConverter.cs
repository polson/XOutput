using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace XOutput.UI.Converters
{
    /// <summary>
    /// Multi converter class.
    /// </summary>
    public class MultiConverter : List<IValueConverter>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object returnValue = value;
            foreach (var converter in this)
            {
                returnValue = converter.Convert(returnValue, targetType, parameter, culture);
            }
            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
