using System;
using System.Globalization;
using Windows.UI.Xaml.Data;
namespace HelpClasses
{
    public class BooleanToOpacity : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var flag = true;
            if (value is bool)
            {
                flag = (bool)value;
            }
            if (flag)
            {
                return 0.4;
            }
            return 1;
           
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var i = 1.0;
            if (value is double)
            {
                i = (double) value ;
            }
            return i;
        }
    }

    public class BooleanToOpacityHidden : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var flag = true;
            if (value is bool)
            {
                flag = (bool)value;
            }
            if (flag)
            {
                return 1.0;
            }
            return 0.0;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var i =1.0;
            if (value is double)
            {
                i = (double)value;
            }
            return i;
        }

        

    }
}
