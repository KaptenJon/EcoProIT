﻿using EcoProIT.Chart.Utility;

namespace EcoProIT.Chart.Axis
{
    /// <summary>
    /// Linear XAxis
    /// </summary>
    public class LinearXAxis : XAxis
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinearXAxis"/> class.
        /// </summary>
        public LinearXAxis() :     
            base()
        {
            this.Type = XType.Double; 
        }
    }
}
