using System.Windows;
using System.Windows.Media;
#if WINRT
using Windows.UI.Xaml.Media;
#else
#endif
using EcoProIT.Chart.Extensions;
using EcoProIT.Chart.Series;

namespace EcoProIT.Chart.Container
{
    /// <summary>
    /// SplineSeries Container
    /// </summary>
    public class SplineContainer : SeriesContainer
    {
#if WPF
        protected override void DrawPath(SeriesBase series, System.Drawing.Pen pen)
        {
            if (series is SplineSeries)
            {
                SplineSeries splineSeries = series as SplineSeries;
                var points = splineSeries.SplinePoints;
                var pointCount = splineSeries.SplinePoints.Count;
                if (RenderingMode == Utility.RenderingMode.Default)
                {
                    PartsCanvas.Children.Clear();
                    for (int i = 0; i < splineSeries.Parts.Count; i++)
                    {
                        var element = splineSeries.Parts[i].CreatePart();
                        if (element != null && !PartsCanvas.Children.Contains(element))
                            PartsCanvas.Children.Add(element);
                    }
                }
                else
                {
                    for (int i = 0; i < pointCount - 1; i++)
                    {
                        switch (RenderingMode)
                        {
                            case Utility.RenderingMode.GDIRendering:
                                GDIGraphics.DrawBezier(pen, points[i].AsDrawingPointF(), splineSeries.FirstControlPoints[i].AsDrawingPointF(), splineSeries.SecondControlPoints[i].AsDrawingPointF(), points[i + 1].AsDrawingPointF());
                                break;
                            case Utility.RenderingMode.Default:
                                break;
                            case Utility.RenderingMode.WritableBitmap:
                                this.WritableBitmap.Lock();
                                WritableBitmapGraphics.DrawBezier(pen, points[i].AsDrawingPointF(), splineSeries.FirstControlPoints[i].AsDrawingPointF(), splineSeries.SecondControlPoints[i].AsDrawingPointF(), points[i + 1].AsDrawingPointF());
                                this.WritableBitmap.AddDirtyRect(new Int32Rect(0, 0, WritableBitmap.PixelWidth, WritableBitmap.PixelHeight));
                                this.WritableBitmap.Unlock();
                                break;
                            default:
                                break;
                        }
                    }
                    this.Collection.InvalidateBitmap();
                }
            }
        }
#else
        protected override void DrawPath(SeriesBase series, Brush brush, double strokeThickness)
        {
            if (series is SplineSeries)
            {
                SplineSeries splineSeries = series as SplineSeries;
                PartsCanvas.Children.Clear();
                for (int i = 0; i < splineSeries.Parts.Count; i++)
                {
                    PartsCanvas.Children.Add(splineSeries.Parts[i].CreatePart());
                }
            }
        }
#endif
    }
}
