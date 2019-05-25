using System.Windows.Media;

namespace Sparrow.Chart
{
    public class BarErrorContainer : SeriesContainer
    {
        override protected void DrawPath(SeriesBase series, System.Drawing.Pen pen)
        {
            if (series is BarErrorSeries)
            {
                var points = new PointCollection();
                var lowPoints = new PointCollection();
                var openPoints = new PointCollection();
                var closePoints = new PointCollection();
                var openOffPoints = new PointCollection();
                var closeOffPoints = new PointCollection();
                var pointCount = 0;
                PartsCollection partsCollection = new PartsCollection();
                if (series is BarErrorSeries)
                {
                    BarErrorSeries lineSeries = series as BarErrorSeries;
                    points = lineSeries.HighPoints;
                    lowPoints = lineSeries.LowPoints;
                    openPoints = lineSeries.OpenPoints;
                    closePoints = lineSeries.ClosePoints;
                    openOffPoints = lineSeries.OpenOffPoints;
                    closeOffPoints = lineSeries.CloseOffPoints;

                    pointCount = lineSeries.HighPoints.Count;
                    partsCollection = lineSeries.Parts;
                }
                if (RenderingMode == RenderingMode.Default)
                {
                    for (int i = 0; i < partsCollection.Count; i++)
                    {
                        var element = partsCollection[i].CreatePart();
                        if (element != null && !PartsCanvas.Children.Contains(element))
                            PartsCanvas.Children.Add(element);
                    }
                }
                else
                {
                    if (series is BarErrorSeries)
                    {
                        for (int i = 0; i < pointCount; i++)
                        {

                            switch (RenderingMode)
                            {
                                case RenderingMode.GDIRendering:
                                    GDIGraphics.DrawLine(pen, points[i].AsDrawingPointF(), lowPoints[i].AsDrawingPointF());
                                    GDIGraphics.DrawLine(pen, openOffPoints[i].AsDrawingPointF(), openPoints[i].AsDrawingPointF());
                                    GDIGraphics.DrawLine(pen, closeOffPoints[i].AsDrawingPointF(), closePoints[i].AsDrawingPointF());
                                    break;
                                case RenderingMode.Default:
                                    break;
                                case RenderingMode.WritableBitmap:
                                    this.WritableBitmap.Lock();
                                    WritableBitmapGraphics.DrawLine(pen, points[i].AsDrawingPointF(), lowPoints[i].AsDrawingPointF());
                                    WritableBitmapGraphics.DrawLine(pen, openOffPoints[i].AsDrawingPointF(), openPoints[i].AsDrawingPointF());
                                    WritableBitmapGraphics.DrawLine(pen, closeOffPoints[i].AsDrawingPointF(), closePoints[i].AsDrawingPointF());
                                    this.WritableBitmap.Unlock();
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    this.Collection.InvalidateBitmap();
                }
            }
        }

    }
}
