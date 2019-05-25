using EcoProIT.Chart.Utility;

namespace EcoProIT.Chart.Axis
{
    /// <summary>
    /// Linear YAxis
    /// </summary>
    public class LinearYAxis : YAxis
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinearYAxis"/> class.
        /// </summary>
        public LinearYAxis() :
            base()
        {
            this.Type = YType.Double;
            MinValue = 0;
            
        }
    }
}
