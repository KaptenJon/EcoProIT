using System.Windows;
#if WINRT
using Windows.UI.Xaml;
#endif
using EcoProIT.Chart.Collections;

namespace EcoProIT.Chart.Axis
{

    /// <summary>
    /// IAxis
    /// </summary>
    public interface IAxis
    {        

        object MinValue
        {
            get;
            set;
        }

        object MaxValue
        {
            get;
            set;
        }

        object Interval
        {
            get;
            set;
        }

        Style AxisLineStyle
        {
            get;
            set;
        }

        SeriesCollection Series
        {
            get;
            set;
        }

        
    }
}
