using EcoProIT.Chart.Utility;

namespace EcoProIT.Chart.Axis
{
    /// <summary>
    /// DateTime YAxis
    /// </summary>
    public class DateTimeYAxis : YAxis
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeYAxis"/> class.
        /// </summary>
        public DateTimeYAxis() :
            base()
        {
            this.Type = YType.DateTime;
        }
    }
}
