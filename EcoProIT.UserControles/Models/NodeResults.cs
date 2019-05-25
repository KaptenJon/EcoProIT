using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using EcoProIT.DataLayer;
using EcoProIT.UserControles.ViewModel;
using HelpClasses;

namespace EcoProIT.UserControles.Models
{
    
    public class NodeResults: IResults
    {
        private  IEnumerable<State>  _states;
        
        public  IEnumerable<State>  States
        {
            get { return _states; }
            set
            {
                if (_states == value)
                    return;
                _states = value;
                OnPropertyChanged();
            }
        }


#region Consumptions



        public override void UpdateConsumptions()
        {
            oldCalculateOneProductIndex.Clear();
            oldConsumptionsPerProduct.Clear();
            oldPerProductResult.Clear();
            oldPerTime.Clear();
            
            var dic = new Dictionary<Consumable, List<decimal>>();
            
            
            foreach (var pair in SourceSimulationModelSet)
            {
                var runresult = GetConsumablesForOneRun(pair.Key, pair.Value);
                
                foreach (var runresultcons in runresult)
                {
                    if (dic.ContainsKey(runresultcons.Key))
                        dic[runresultcons.Key].Add(runresultcons.Value); //(IResults.TotalTime/3600);
                    else
                        dic.Add(runresultcons.Key, new List<decimal>(new [] {runresultcons.Value}));
                }
            }
            var res = dic.ToDictionary(t=> t.Key, i=>new Statistic(i.Value.Mean(), i.Value.StandardDeviation()));
            RunResultsTotal = dic;
            Consumables = res;
            MeanConsumables = res.ToDictionary(t => t.Key, t => t.Value.Mean);
            UpdateBaseIndicator();
        }

        private Dictionary<Consumable, decimal> GetConsumablesForOneRun(ParallelQuery<ProductResult> productresults, ParallelQuery<MachineState> machineStates)
        {
            var tableOrdered = (from i in productresults where i.ModelNode == ResultId select i).ToList();
            var dic = new Dictionary<Consumable, decimal>();
            if (!tableOrdered.Any())
                return dic;

            var f = ProductResults(tableOrdered);
            decimal count = f.Count();
            ulong sumUsed = f.Sum(t => t.Total, null);


            ulong downtime =
                (from i in machineStates where i.Machine == ResultId && i.State == "Down" select i.Total).Sum(new object());
            ulong idletime =
                (from i in machineStates where i.Machine == ResultId && i.State == "Idle" select i.Total).Sum(new object());
            

            foreach (State state in States)
            {
                foreach (Consumption consumption in state.Consumptions)
                {
                    var res = consumption.Amount;
                    if (!consumption.Static)
                    {
                        res /= 3600000;
                        if (state.Name == "Processing" || state.Name == "Used")
                            res += res*sumUsed;
                        else if (state.Name == "Down")
                            res += res*downtime;
                        else if (state.Name == "Idle")
                            res += res*idletime;
                    }
                    else
                        res = res*count;
                    if (IResults.TotalTime > 0)
                    {
                        if (dic.ContainsKey(consumption.Consumable))
                            dic[consumption.Consumable] += res/count; //(IResults.TotalTime/3600);
                        else
                            dic.Add(consumption.Consumable, res/count);
                    }
                }
            }
            if (IResults.TotalTime != 0)
                dic.Add(new Consumable() {Name = "Processed per hour"}, count/(IResults.TotalTime/3600000));
            dic.Add(new Consumable() {Name = "Processed"}, count);
            return dic;
        }

        private Dictionary<string, Dictionary<Consumable, decimal>> oldConsumptionsPerProduct = new Dictionary<string, Dictionary<Consumable, decimal>>();
        public Dictionary<Consumable, decimal> ConsumptionsPerProduct(string product)
        {
            if (oldConsumptionsPerProduct.ContainsKey(product))
                return oldConsumptionsPerProduct[product];
            var tableOrdered = (from i in SourceTableOrdered where i.ProductType == product && i.ModelNode == ResultId select i).ToList();
            
            if (!tableOrdered.Any())
                return null;
            
            var f = ProductResults(tableOrdered);
            decimal count = f.Count();
            ulong sumUsed = f.Sum(t => t.Total, null);


            ulong downtime = Downtime(0, UInt64.MaxValue);
            ulong idletime = Idletime(0, UInt64.MaxValue);

            var dic = new Dictionary<Consumable, decimal>();
            oldConsumptionsPerProduct.Add(product, dic);
            foreach (State state in States)
            {
                foreach (Consumption consumption in state.Consumptions)
                {
                    var res = consumption.Amount;
                    if (!consumption.Static)
                    {
                        res /= 3600000;
                        if (state.Name == "Processing" || state.Name == "Used")
                            res += res * sumUsed;
                        else if (state.Name == "Down")
                            res += res * downtime;
                        else if (state.Name == "Idle")
                            res += res * idletime;
                    }
                    else
                        res = res * count;

                    if (dic.ContainsKey(consumption.Consumable))
                        dic[consumption.Consumable] += res / count;
                    else
                        dic.Add(consumption.Consumable, res / count);
                }
            }
            if (IResults.TotalTime != 0)
                dic.Add(new Consumable() { Name = "Processed per hour" }, count / (IResults.TotalTime / 3600000));
            dic.Add(new Consumable() { Name = "Processed" }, count);
            return dic;
            
        }
        private Dictionary<string, Dictionary<string, decimal>> oldPerTime = new Dictionary<string, Dictionary<string, decimal>>();
        public override Dictionary<string, decimal> PerTime(ulong timeinterval, string index)
        {
            if (oldPerTime.ContainsKey(index + timeinterval))
                return oldPerTime[index + timeinterval];
            var dic = new Dictionary<string, decimal>();
            for (ulong i = 0; i < IResults.TotalTime; i += timeinterval)
            {
                dic.Add((i / timeinterval).ToString(CultureInfo.InvariantCulture), CalculateInterval(i, i + timeinterval, index));
            }
            oldPerTime.Add(index + timeinterval, dic);
            return dic;
        }

        Dictionary<string, decimal> oldCalculateOneProductIndex = new Dictionary<string, decimal>();
        public override decimal CalculateForOneProduct(Product results, string index)
        {
            if (oldCalculateOneProductIndex.ContainsKey(results.ProductName + index))
                return oldCalculateOneProductIndex[results.ProductName + index];
            var tableOrdered =
                (from i in SourceTableOrdered where i.ModelNode == ResultId && i.ProductType == results.ProductName select i)
                    .ToList();
            if (!tableOrdered.Any())
                return 0;
            
            var f = ProductResults(tableOrdered);
            decimal count = f.Count();
            var sumUsed = f.Sum(t => t.Total , 0uL) ;

            if (index == "Processed per hour")
                return count/(IResults.TotalTime == 0 ? 1 : IResults.TotalTime/3600000);
            else if (index =="Processed")
            {
                return count;
            }
            var downtime = Downtime(0, UInt64.MaxValue);
            var idletime = Idletime(0, UInt64.MaxValue);
            decimal dic = 0;

            foreach (State state in States)
            {
                var consumption = state.Consumptions.FirstOrDefault(t => t.Consumable.Name == index);
                if (consumption != default(Consumption))
                {
                    var res = consumption.Amount;
                    if (!consumption.Static)
                    {
                        res /= 3600000;
                        if (state.Name == "Processing" || state.Name == "Used")
                            res += res*sumUsed;
                        else if (state.Name == "Down")
                            res += res*downtime;
                        else if (state.Name == "Idle")
                            res += res*idletime;
                    }
                    else
                        res = res * count;
                    dic += res / count;
                }
            }
            oldCalculateOneProductIndex.Add(results.ProductName+index,dic);
            return dic;
        }

        protected override ulong TotalProduced(ulong start, ulong end, string productName, ParallelQuery<ProductResult> productResults)
        {
            var res = (from i in productResults where i.ModelNode == ResultId && i.ProductType == productName && i.Start > start && i.End < end select i).ToList().AsParallel();
            if (!res.Any())
                return 0;
            return (ulong)res.Count();
        }


        Dictionary<string, Dictionary<string, decimal>> oldPerProductResult = new Dictionary<string, Dictionary<string, decimal>>();


        public override Dictionary<string, decimal> PerProduct(IEnumerable<Product> products, string index)
        {
            if (oldPerProductResult.ContainsKey(index))
                return oldPerProductResult[index];
            var result = new Dictionary<string, decimal>();
            foreach (var product in products)
            {
                var consumption = ConsumptionsPerProduct(product.ProductName);
                result.Add(product.ProductName, IndexCalculator.Calculate(consumption,index));
            }
            oldPerProductResult.Add(index, result);
            return result;
        }


        /**
        public override Dictionary<string, decimal> CalculateIntervalTime(ulong interval, ulong start, ulong end, string product,
            ParallelQuery<ProductResult> filtredProductResult, ParallelQuery<MachineState> fitredMachineStates, string index)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, decimal> CalculateTime(ulong start, ulong end, string product, ParallelQuery<ProductResult> filtredProductResult,
            ParallelQuery<MachineState> fitredMachineStates, string index)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, decimal> CalculateIndirectForOneProduct(string product, ParallelQuery<ProductResult> filtredProductResult,
            ParallelQuery<MachineState> fitredMachineStates, string index)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, decimal> CalculatePerProduct(string product, ParallelQuery<ProductResult> filtredProductResult, ParallelQuery<MachineState> fitredMachineStates,
            string index)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, decimal> CalculatePerMachine(ParallelQuery<ProductResult> filtredProductResult, ParallelQuery<MachineState> fitredMachineStates, string index)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, decimal> CalculateIndirectPerMachine(ParallelQuery<ProductResult> filtredProductResult, ParallelQuery<MachineState> fitredMachineStates,
            string index)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, decimal> CalculateIndex(ParallelQuery<ProductResult> filtredProductResult, ParallelQuery<MachineState> fitredMachineStates, string index)
        {
            throw new NotImplementedException();
        }

        **/
        private decimal CalculateInterval(ulong start, ulong end, string index)
        {
            var tableOrdered = (from i in SourceTableOrdered where i.ModelNode == ResultId && i.Start > start && i.Start < end select i).ToList();
            if (!tableOrdered.Any())
                return 0;
            var f = ProductResults(tableOrdered);
            decimal count = f.Count();
            var sumUsed = f.Sum(t => t.Total,null) ;
            decimal intervaltime = end - start;

            var downtime = Downtime( start,  end);
            var idletime = Idletime(start, end);
            decimal result = 0;
            if (index == "Processed per hour")
                result += count/(intervaltime/3600000);
            else if (index == "Processed")
                result += count;

            else if (IndexCalculator.Indexes.Any(t => t.IndexName == index))
            {
                var dic = new Dictionary<Consumable, decimal>();
                foreach (State state in States)
                {
                    foreach (Consumption consumption in state.Consumptions)
                    {
                        var res = consumption.Amount;
                        if (!consumption.Static)
                        {
                            res /= 3600000;
                            if (state.Name == "Processing" || state.Name == "Used")
                                res += res * sumUsed;
                            else if (state.Name == "Down")
                                res += res * downtime;
                            else if (state.Name == "Idle")
                                res += res * idletime;
                        }
                        else
                            res = res * count;

                        if (dic.ContainsKey(consumption.Consumable))
                            dic[consumption.Consumable] += res / count;
                        else
                            dic.Add(consumption.Consumable, res / count);
                    }
                }
                result += IndexCalculator.Calculate(dic, index);
            }
            else
            {
                foreach (State state in States)
                {
                    var consumption = state.Consumptions.FirstOrDefault(t => t.Consumable.Name == index);
                    if (consumption != default(Consumption))
                    {
                        var res = consumption.Amount;
                        if (!consumption.Static)
                        {
                            res/=3600000;
                            switch (state.Name)
                            {
                                case "Used":
                                case "Processing":
                                    res += res*sumUsed;
                                    break;
                                case "Down":
                                    res += res*downtime;
                                    break;
                                case "Idle":
                                    res += res*idletime;
                                    break;
                            }
                        }
                        else
                            res = res*count;
                        result += res;
                    }

                }
            }

            return result;
        }



        private ulong Downtime(ulong start, ulong end)
        {
            var d = (from i in SourceMachineStates where i.Machine == ResultId && i.State == "Down" && i.Start + i.Total > start && i.Start < end select i);
            return ReduceToInterval(start, end, d.Cast<SimulationResult>());
        }



        private ulong Idletime(ulong start, ulong end)
        {
            var d = (from i in SourceMachineStates where i.Machine == ResultId && i.State == "Idle" && i.Start + i.Total > start && i.Start < end select i);
            return ReduceToInterval(start, end, d.Cast<SimulationResult>());
        }

        public override void UpdateBaseIndicator()
        {
            _nodeIndicatorBase.Remove(ResultId);
            _nodeIndicatorBase.Add(ResultId, MeanConsumables.Any(t => t.Key.Name == IResults.SelectedIndex) ? MeanConsumables.First(t => t.Key.Name == IResults.SelectedIndex).Value : IndexCalculator.Indexes.Any(t => t.IndexName == SelectedIndex) ? IndexCalculator.Indexes.First(t => t.IndexName == SelectedIndex).Calculation(MeanConsumables) : 0);
        }
        #endregion
    }
}
