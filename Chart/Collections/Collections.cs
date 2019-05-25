using System.Collections.ObjectModel;
using EcoProIT.Chart.Axis;
using EcoProIT.Chart.Container;
using EcoProIT.Chart.Points;
using EcoProIT.Chart.Series;
using EcoProIT.Chart.SeriesParts;

namespace EcoProIT.Chart.Collections
{
    /// <summary>
    /// Series Parts Collection
    /// </summary>
    public class PartsCollection : ObservableCollection<SeriesPartBase>
    {
    }

    /// <summary>
    /// ChartPoint Collection
    /// </summary>
    public class PointsCollection : ObservableCollection<ChartPoint>
    {
    }

    /// <summary>
    /// Series Collection
    /// </summary>
    public class SeriesCollection : ObservableCollection<SeriesBase>
    {
    }


    /// <summary>
    /// Container Collection
    /// </summary>
    public class Containers : ObservableCollection<SeriesContainer>
    {
    }

    /// <summary>
    /// Axis Collection
    /// </summary>
    public class Axes : ObservableCollection<AxisBase>
    {
    }

}
