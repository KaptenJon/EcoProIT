using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EcoProIT.Chart.SeriesParts
{
    public class BarErrorPart : LinePartBase
    {
        internal Point Point1;
        internal Point Point2;
        internal Point Point3;
        internal Point Point4;
        internal Point Point5;
        internal Point Point6;
        internal bool IsBearfill;
        Canvas _canvas;
        Line _line1;
        Line _line2;
        Line _line3;
        Line _line4;

        /// <summary>
        /// Initializes a new instance of the <see cref="BarErrorPart"/> class.
        /// </summary>
        public BarErrorPart()
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BarErrorPart"/> class.
        /// </summary>
        /// <param name="point1">The point1.</param>
        /// <param name="point2">The point2.</param>
        /// <param name="point3">The point3.</param>
        /// <param name="point4">The point4.</param>
        /// <param name="point5">The point5.</param>
        /// <param name="point6">The point6.</param>
        public BarErrorPart(Point point1, Point point2, Point point3, Point point4, Point point5, Point point6, double mean, double error, string name)
        {
            this.Point1 = point1;
            this.X1 = point1.X;
            this.Y1 = point1.Y;
            this.Point2 = point2;
            this.X2 = point2.X;
            this.Y2 = point2.Y;
            this.Point3 = point3;
            this.X3 = point3.X;
            this.Y3 = point3.Y;
            this.Point4 = point4;
            this.X4 = point4.X;
            this.Y4 = point4.Y;
            this.Point5 = point5;
            this.X5 = point5.X;
            this.Y5 = point5.Y;
            this.Point6 = point6;
            this.X6 = point6.X;
            this.Y6 = point6.Y;
            this.Mean = mean;
            this.Error = error;
            this.XName = name;

        }

        public string XName { get; set; }

        public double Error { get; set; }

        public double Mean { get; set; }

        private double _x3;
        /// <summary>
        /// Gets or sets the x3.
        /// </summary>
        /// <value>
        /// The x3.
        /// </value>
        public double X3
        {
            get { return _x3; }
            set { _x3 = value; }
        }

        private double _x4;
        /// <summary>
        /// Gets or sets the x4.
        /// </summary>
        /// <value>
        /// The x4.
        /// </value>
        public double X4
        {
            get { return _x4; }
            set { _x4 = value; }
        }

        private double _x5;
        /// <summary>
        /// Gets or sets the x5.
        /// </summary>
        /// <value>
        /// The x5.
        /// </value>
        public double X5
        {
            get { return _x5; }
            set { _x5 = value; }
        }

        private double _x6;
        /// <summary>
        /// Gets or sets the x6.
        /// </summary>
        /// <value>
        /// The x6.
        /// </value>
        public double X6
        {
            get { return _x6; }
            set { _x6 = value; }
        }

        private double _y3;
        /// <summary>
        /// Gets or sets the y3.
        /// </summary>
        /// <value>
        /// The y3.
        /// </value>
        public double Y3
        {
            get { return _y3; }
            set { _y3 = value; }
        }

        private double _y4;
        /// <summary>
        /// Gets or sets the y4.
        /// </summary>
        /// <value>
        /// The y4.
        /// </value>
        public double Y4
        {
            get { return _y4; }
            set { _y4 = value; }
        }
        private double _y5;
        /// <summary>
        /// Gets or sets the y5.
        /// </summary>
        /// <value>
        /// The y5.
        /// </value>
        public double Y5
        {
            get { return _y5; }
            set { _y5 = value; }
        }

        private double _y6;
   

        /// <summary>
        /// Gets or sets the y6.
        /// </summary>
        /// <value>
        /// The y6.
        /// </value>
        public double Y6
        {
            get { return _y6; }
            set { _y6 = value; }
        }

        /// <summary>
        /// Create a visual for single Series Part
        /// </summary>
        /// <returns>
        /// UIElement
        /// </returns>
        public override UIElement CreatePart()
        {
            _canvas = new Canvas();
            _line1 = new Line();
            _line2 = new Line();
            _line3 = new Line();
            _line4 = new Line();

            _line1.X1 = Point1.X;
            _line1.X2 = Point2.X;
            _line1.Y1 = Point1.Y;
            _line1.Y2 = Point2.Y;

            _line2.X1 = Point3.X;
            _line2.X2 = Point4.X;
            _line2.Y1 = Point3.Y;
            _line2.Y2 = Point4.Y;

            _line3.X1 = Point5.X;
            _line3.X2 = Point6.X;
            _line3.Y1 = Point5.Y;
            _line3.Y2 = Point6.Y;

            _line4.X1 = Point5.X + (Point6.X - Point5.X) / 2;
            _line4.X2 = Point5.X + (Point6.X - Point5.X) / 2;
            _line4.Y1 = Point4.Y;
            _line4.Y2 = Point6.Y;

            _canvas.Children.Add(_line1);
            _canvas.Children.Add(_line2);
            _canvas.Children.Add(_line3);
            _canvas.Children.Add(_line4);

            SetBindingForStrokeandStrokeThickness(_line1);
            _line2.StrokeThickness = 1;
            _line3.StrokeThickness = 1;
            _line4.StrokeThickness = 1;
            SetBindingForStroke(_line2);
            SetBindingForStroke(_line3);
            _line2.Stroke = new SolidColorBrush(Colors.Black);
            _line3.Stroke = new SolidColorBrush(Colors.Black);
            _line4.Stroke = new SolidColorBrush(Colors.Black);
            _canvas.ToolTip = string.Format("{2}:\n{0} std:{1}", Mean, Error,XName);
            return _canvas;
        }

        /// <summary>
        /// Refresh the Series Part
        /// </summary>
        public override void Refresh()
        {
            if (_line1 != null && _line2 != null && _line3 != null)
            {
                _line1.X1 = Point1.X;
                _line1.X2 = Point2.X;
                _line1.Y1 = Point1.Y;
                _line1.Y2 = Point2.Y;

                _line2.X1 = Point3.X;
                _line2.X2 = Point4.X;
                _line2.Y1 = Point3.Y;
                _line2.Y2 = Point4.Y;

                _line3.X1 = Point5.X;
                _line3.X2 = Point6.X;
                _line3.Y1 = Point5.Y;
                _line3.Y2 = Point6.Y;

                _line4.X1 = Point5.X + (Point6.X - Point5.X) / 2;
                _line4.X2 = Point5.X + (Point6.X - Point5.X) / 2;
                _line4.Y1 = Point4.Y;
                _line4.Y2 = Point6.Y;
                _canvas.ToolTip = string.Format("{2}:\n{0} std:{1}", Mean, Error, XName);
            }
            
        }
    }
}
