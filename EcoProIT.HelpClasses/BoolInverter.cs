﻿using System;
using System.Globalization;
using Windows.UI.Xaml.Data;


namespace HelpClasses
{
    public class BoolInverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
                return !(bool)value;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Convert(value, null, null, null);
        }
    }
}
