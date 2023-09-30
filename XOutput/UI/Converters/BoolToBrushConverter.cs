﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace XOutput.UI.Converters;

/// <summary>
///     Converts a boolean value to brush.
///     Cannot be used backwards.
/// </summary>
public class BoolToBrushConverter : IValueConverter
{
    /// <summary>
    ///     Converts the bool value. Returns light green(true) or light gray(false).
    /// </summary>
    /// <param name="value">Boolean value to convert</param>
    /// <param name="targetType">Ignored</param>
    /// <param name="parameter">Ignored</param>
    /// <param name="culture">Ignored</param>
    /// <returns></returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var b = value as bool?;
        if (b == null) throw new ArgumentException(nameof(value));

        if (b == true)
            return new SolidColorBrush(Color.FromRgb(100, 255, 100));
        return new SolidColorBrush(Color.FromRgb(200, 200, 200));
    }

    /// <summary>
    ///     Intentionally not implemented.
    /// </summary>
    /// <param name="value">Ignored</param>
    /// <param name="targetType">Ignored</param>
    /// <param name="parameter">Ignored</param>
    /// <param name="culture">Ignored</param>
    /// <returns></returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}