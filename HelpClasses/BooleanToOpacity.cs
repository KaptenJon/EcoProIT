using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HelpClasses
{
    public class BooleanToOpacity : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var i = 1.0;
            if (value is double)
            {
                i = (double) value ;
            }
            return i;
        }
    }
}
