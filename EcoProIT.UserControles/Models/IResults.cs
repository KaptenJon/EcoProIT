using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Deployment.Application;
using System.Linq;
using System.Runtime.CompilerServices;
using EcoProIT.DataLayer;
using EcoProIT.UserControles.ViewModel;
using HelpClasses;
using HelpClasses.Annotations;

namespace EcoProIT.UserControles.Models
{
    public abstract class IResults:INotifyPropertyChanged
    {
        private string _resultId;
        private Dictionary<Consumable, decimal> _consumables = new Dictionary<Consumable, decimal>();
        private Statistic _totalProducedStats;

        public Dictionary<Consumable, decimal> Consumables
        { get { return _consumables; } protected set { _consumables = value; OnPropertyChanged("Consumables"); } }

        public static string SelectedIndex { get; set; }

        public string ResultId
        {
            get { return _resultId; }
            set
            {
                _resultId = value;
                OnPropertyChanged("ResultId");
            }

        }
        public static void UpdateSourceData(ISet<Pair<IEnumerable<ProductResult>, IEnumerable<MachineState>>> modelResultSet )
        {
            SourceSimulationModelSet = new HashSet<Pair<ParallelQuery<ProductResult>,ParallelQuery<MachineState>>>();
            foreach (var pair in modelResultSet)
            {
                var sourceProductResultsOrdered = (from i in pair.Key orderby i.Start select i).ToList().AsParallel();
                var sourceMachineStates = pair.Value.AsParallel();
                SourceSimulationModelSet.Add(new Pair<ParallelQuery<ProductResult>, ParallelQuery<MachineState>>(sourceProductResultsOrdered, sourceMachineStates));
            }
            SourceMachineStates = modelResultSet.FirstOrDefault().Value.ToList().AsParallel();
            SourceTableOrdered  = modelResultSet.FirstOrDefault().Key.ToList().AsParallel();
            TotalTime = (from i in modelResultSet.FirstOrDefault().Value where i.Machine == "BaseModel" && i.State == "BaseTime" select i.Total).FirstOrDefault();
            
        }

        public Statistic TotalProducedStats
        {
            get { return _totalProducedStats; }
            set
            {
                _totalProducedStats = value;
                OnPropertyChanged();
            }
        }

        protected static ParallelQuery<ProductResult> SourceTableOrdered;
        protected static ParallelQuery<MachineState> SourceMachineStates;
        protected static HashSet<Pair<ParallelQuery<ProductResult>, ParallelQuery<MachineState>>> SourceSimulationModelSet { get; private set; }
        protected static Dictionary<string, decimal> _nodeIndicatorBase = new Dictionary<string, decimal>();
        private static decimal _totalTime;

        public decimal NodeIndicator
        {
            get
            {
                if(_nodeIndicatorBase.Count == 0)
                    return 0;
                decimal baseforIndex = _nodeIndicatorBase.Max(t => t.Value);
                if (_nodeIndicatorBase.ContainsKey(ResultId) && baseforIndex != 0)
                    return _nodeIndicatorBase[ResultId]/baseforIndex;
                return 0;
            }
        }

        public static decimal TotalTime
        {
            get { return _totalTime; }
            private set { _totalTime = value;}
        }


        public void NotifyIndicatiorBase()
        {
            OnPropertyChanged("NodeIndicator");
        }

        public abstract void UpdateBaseIndicator();
        public abstract void UpdateConsumptions();



        public abstract Dictionary<string, decimal> PerTime(ulong timespan, string index);

        public abstract decimal CalculateForOneProduct(Product results, string selectedIndex);

        protected abstract ulong TotalProduced(ulong start, ulong end, string productName, ParallelQuery<ProductResult> productResults);

        public abstract Dictionary<string, decimal> PerProduct(IEnumerable<Product> products, string index);

        protected static ulong ReduceToInterval(ulong start, ulong end, ParallelQuery<SimulationResult> d)
        {
            ulong reducedtime = 0UL;
            var takenResults = new HashSet<SimulationResult>();
            foreach (var m in d.OrderBy(t => t.Start))
            {
                if (m.Start < start)
                {
                    reducedtime += start - m.Start;
                    takenResults.Add(m);
                }
                if (m.End > end)
                {
                    reducedtime += (m.End - end);
                    takenResults.Add(m);
                }
                if (m.Start > start && m.End < end)
                    break;
            }
            foreach (var m in d.OrderByDescending(t => t.Start))
            {
                if (m.Start < start && !takenResults.Contains(m))
                    reducedtime += start - m.Start;
                if (m.End > end && !takenResults.Contains(m))
                    reducedtime += (m.End - end);
                if (m.Start > start && m.End < end)
                    break;
            }
            return d.Sum(t => t.Total, new object()) - reducedtime;
        }

        /// <summary>
        /// New Version
        /// </summary>
        /// <param name="interval">Interval size 0 equals no intervals</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="product"></param>
        /// <param name="simulationModelSet"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public Statistic TotalProduced(ulong start, ulong end,string productName )
        {
            var simresult = SourceSimulationModelSet.Select(model => (decimal) TotalProduced(start, end, productName, model.Key)).ToList();
            return new Statistic(simresult.Mean(), simresult.StandardDeviation());
           
        }

        protected static ParallelQuery<ProductResult> ProductResults(List<ProductResult> tableOrdered)
        {
            //fix overlapping times
            for (int i = 0; i < tableOrdered.Count - 1; i++)
            {
                if ((tableOrdered[i].Start + tableOrdered[i].Total) > tableOrdered[i + 1].Start)
                    tableOrdered[i].Total = tableOrdered[i + 1].Start - tableOrdered[i].Start;
            }
            var f = tableOrdered.AsParallel();
            return f;
        }
        protected static ParallelQuery<MachineState> ProductResults(List<MachineState> tableOrdered)
        {
            //fix overlapping times
            for (int i = 0; i < tableOrdered.Count - 1; i++)
            {
                if ((tableOrdered[i].Start + tableOrdered[i].Total) > tableOrdered[i + 1].Start)
                    tableOrdered[i].Total = tableOrdered[i + 1].Start - tableOrdered[i].Start;
            }
            var f = tableOrdered.AsParallel();
            return f;
        }

        /**
        /// <summary>
        /// New Version
        /// </summary>
        /// <param name="interval">Interval size 0 equals no intervals</param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="product"></param>
        /// <param name="simulationModelSet"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public List<Pair<string, Statistic>> CalculateStatisticsInterval(ulong interval, ulong start, ulong end, string product,
            IEnumerable<Pair<ParallelQuery<ProductResult>, ParallelQuery<MachineState>>> SimulationModelSet, string index)
        {

            var Simresult = new List<Pair<string, decimal>>();
            foreach (var model in SimulationModelSet)
            {
                if (interval > 0)
                    Simresult.Add(CalculateIntervalTime(interval, start, end, product, model.Key, model.Value, index));
                else
                {
                    Simresult.Add(CalculateTime(start, end, product, model.Key, model.Value, index));
                }
            }
            var results = new Dictionary<string, Statistic>();
            foreach (var res in Simresult.First())
            {
                var list = Simresult.Select(value => value[res.Key]).ToList();
                results.Add(res.Key, new Statistic(list.Mean(), list.StandardDeviation()));
            }
            return results;
        }

        public abstract List<Pair<string, Pair<ulong, decimal>>> CalculateIntervalTime(ulong interval, ulong start, ulong end, string product, ParallelQuery<ProductResult> filtredProductResult, ParallelQuery<MachineState> fitredMachineStates, string index);

        public abstract Pair<string, decimal> CalculateTime(ulong start, ulong end, string product, ParallelQuery<ProductResult> filtredProductResult, ParallelQuery<MachineState> fitredMachineStates, string index);

        public abstract Pair<string, decimal> CalculateIndirectForOneProduct(string product, ParallelQuery<ProductResult> filtredProductResult, ParallelQuery<MachineState> fitredMachineStates, string index);
        public abstract Pair<string, decimal> CalculatePerProduct(string product, ParallelQuery<ProductResult> filtredProductResult, ParallelQuery<MachineState> fitredMachineStates, string index);
        public abstract Pair<string, decimal> CalculatePerMachine(ParallelQuery<ProductResult> filtredProductResult, ParallelQuery<MachineState> fitredMachineStates, string index);
        public abstract Pair<string, decimal> CalculateIndirectPerMachine(ParallelQuery<ProductResult> filtredProductResult, ParallelQuery<MachineState> fitredMachineStates, string index);

        public abstract Pair<string, decimal> CalculateIndex(ParallelQuery<ProductResult> filtredProductResult, ParallelQuery<MachineState> fitredMachineStates, string index);
           */
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}