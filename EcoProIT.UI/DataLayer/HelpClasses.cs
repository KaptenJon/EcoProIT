using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using HelpClasses;
using HelpClasses.Annotations;

namespace EcoProIT.UI.DataLayer
{
/*    public class DatabaseConnection
    {
        private static modeloutputContext current;
        private static object blocked = new object();

        public static modeloutputContext GetModelContext()
        {
            
            lock (blocked)
            {
                if (current != null)
                    return current;
                string connectString = "Data Source = ";
                try
                {
                    connectString += ApplicationDeployment.CurrentDeployment.DataDirectory +
                                    "\\Resources\\modeloutput.sdf;";
                    //"\\Resources\\modeloutput.sdf";
                }
                catch
                {
                    connectString += Environment.CurrentDirectory +
                                        "\\Resources\\modeloutput.sdf;";
                                    //"\\Resources\\modeloutput.sdf";
                }
                if (connectString == "")
                    return null;
                current = new modeloutputContext(new SqlCeConnection(connectString));
            }
            return current;
        }
    }*/

    public enum UnitTypes
    {
        Hour, Seconds, Minutes, MilliSeconds, Kg, g, mg, ton, MWh, Wh, l, ml
    }
    public class GeneralConverter : IList<Double>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        
        public GeneralConverter(UnitTypes unitType) : this(new double[] {}, unitType) { }
        public GeneralConverter(double[] values, UnitTypes unitType)
        {
            UnitType = unitType;
            foreach (var value in values)
            {
                Add(value);
            }
           
        }

        public static UnitTypes GetInstance(string unitType)
        {
            switch (unitType.ToLowerInvariant())
            {
                case "milliseconds":
                    return TimeConverters[0];
                case "seconds":
                    return TimeConverters[1];
                    
                case "minutes":
                    return TimeConverters[2];
                    
                case "hour":
                    return TimeConverters[3];
             }
            return UnitTypes.MilliSeconds;
        }

        public static GeneralConverter[] AllConverters
        {
            get { return (from UnitTypes t in Enum.GetValues(typeof (UnitTypes)) select new GeneralConverter(t)).ToArray(); }
        }


           private static readonly UnitTypes[] _timeTypes = {UnitTypes.MilliSeconds, UnitTypes.Seconds, UnitTypes.Minutes, UnitTypes.Hour};    

        public static UnitTypes[] TimeConverters
        {
            get
            {
                return _timeTypes;
            }
        }

        public static GeneralConverter[] MassConverters
        {
            get
            {
                return new GeneralConverter[3]
                    {
                        new GeneralConverter(UnitTypes.Kg), new GeneralConverter(UnitTypes.mg),
                        new GeneralConverter(UnitTypes.ton)
                    };
            }
        }

        public static GeneralConverter[] EnergyConverters
        {
            get
            {
                return new GeneralConverter[2]
                    {
                        new GeneralConverter(UnitTypes.MWh), new GeneralConverter(UnitTypes.Wh)
                    };
            }
        }

        public static GeneralConverter[] LiquidConverters
        {
            get
            {
                return new GeneralConverter[2]
                    {
                        new GeneralConverter(UnitTypes.l), new GeneralConverter(UnitTypes.ml)
                    };
            }
        }

        public UnitTypes UnitType
        {
            get { return _unitType; }
            set
            {
                if (value == _unitType) return;

                //if (!KeepStandardValue)
                //    var i = parameters.ToList();
                var temp = new List<double>();
                foreach (var parameter in parameters)
                {
                        temp.Add(ToRealValue(parameter));
                }        
                   
                _unitType = value;
                for (int index = 0; index < temp.Count; index++)
                {
                        parameters[index] = ToStandard(temp[index]);
                }

                OnCollectionChanged(0,1);
                OnPropertyChanged();
                OnPropertyChanged("Display");
                OnPropertyChanged("AutomodCode");
            }
        }
        
        private List<double> parameters = new List<double>();
       
        private UnitTypes _unitType;

        private double ToRealValue(double value)
        {
            {
                switch (UnitType)
                {
                    case UnitTypes.MilliSeconds:
                        return value;
                    case UnitTypes.Seconds:
                        return value / 1000;
                    case UnitTypes.Minutes:
                        return value / 60000;
                    case UnitTypes.Hour:
                        return value / 3600000;
                    case UnitTypes.MWh:
                        return value;
                    case UnitTypes.Wh:
                        return value * 1000000;
                    case UnitTypes.Kg:
                        return value;
                    case UnitTypes.g:
                        return value * 1000;
                    case UnitTypes.ton:
                        return value / 1000;
                    case UnitTypes.l:
                        return value;
                    case UnitTypes.ml:
                        return value * 1000;
                }
                Debug.Assert(true, "no unittype match");
                return value;
            }
        }
        private double ToStandard(double value)
        {
            {
                switch (UnitType)
                {
                    case UnitTypes.MilliSeconds:
                        return value;
                    case UnitTypes.Seconds:
                        return value*1000;
                    case UnitTypes.Minutes:
                        return value*60000;
                    case UnitTypes.Hour:
                        return value*3600000;
                    case UnitTypes.MWh:
                        return value;
                    case UnitTypes.Wh:
                        return value/1000000;
                    case UnitTypes.Kg:
                        return value;
                    case UnitTypes.g:
                        return value/1000;
                    case UnitTypes.ton:
                        return value*1000;
                    case UnitTypes.l:
                        return value;
                    case UnitTypes.ml:
                        return value/1000;
                }
                Debug.Assert(true, "no unittype match");
                return value;
            }
        }


        public double GetStandard(int index)
        {
            return parameters[index];
        }

        public string AutomodCode
        {
            get {
                switch (UnitType)
                {
                    case UnitTypes.MilliSeconds:
                        return "ms";
                    case UnitTypes.Hour:
                        return "hr";
                    case UnitTypes.Seconds:
                        return "sec";
                    case UnitTypes.Minutes:
                        return "min";
                }
                return "";
            }

        }

        public override string ToString()
        {
            return _unitType.ToString();
        }

        public IEnumerator<double> GetEnumerator()
        {
            foreach (var parameter in parameters)
            {
                yield return ToRealValue(parameter);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(double item)
        {
            parameters.Add(ToStandard(item));
        }



        public void Clear()
        {
            parameters.Clear();
        }

        public bool Contains(double item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(double[] array, int arrayIndex)
        {
            int i = arrayIndex;
            foreach (var parameter in parameters)
            {
                array[i++]=parameter;
            }
           
        }

        public bool Remove(double item)
        {
            throw new NotImplementedException();
        }

        public int Count { get { return parameters.Count; } }
        public bool IsReadOnly { get { return false; } }
        public int IndexOf(double item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, double item)
        {
            parameters.Insert(index,ToStandard(item));
        }

        public void RemoveAt(int index)
        {
            parameters.RemoveAt(index);
        }

        public double this[int index]
        {
            get
            {
                var ret = ToRealValue(parameters[index]);
                return ret;
            }
            set
            {
                var old = parameters[index];
                parameters[index]=ToStandard(value);
                OnCollectionChanged(old,index);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected void OnCollectionChanged(double changed, int index)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null) handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
    public class ConnectLCIDB
    {
        private static ObservableCollection<Consumable> _consumables = new ObservableCollection<Consumable>();
        static ConnectLCIDB()
        {
            //_consumables.Add(new Consumable() { Name = "Electricity", PerUnit = SIUnits.kWh, LciSet = new LciSet() { Emissions = new List<Emission>() { new Emission() { EmissionName = "co2", SIUnit = SIUnits.kg, Value = new decimal(0.1) } } } });
            //_consumables.Add(new Consumable() { Name = "Pressurized Air", PerUnit = SIUnits.kWh, LciSet = new LciSet() { Emissions = new List<Emission>() { new Emission() { EmissionName = "co2", SIUnit = SIUnits.kg, Value = new decimal(0.1) } } } });
            //_consumables.Add(new Consumable() { Name = "Black Paint", PerUnit = SIUnits.L, LciSet = new LciSet() { Emissions = new List<Emission>() { new Emission() { EmissionName = "co2", SIUnit = SIUnits.kg, Value = new decimal(0.1) } } } });
            //_consumables.Add(new Consumable() { Name = "Plastic", PerUnit = SIUnits.kg, LciSet = new LciSet() { Emissions = new List<Emission>() { new Emission() { EmissionName = "co2", SIUnit = SIUnits.kg, Value = new decimal(0.1) } } } });
            //_consumables.Add(new Consumable() { Name = "Steel", PerUnit = SIUnits.kg, LciSet = new LciSet() { Emissions = new List<Emission>() { new Emission() { EmissionName = "co2", SIUnit = SIUnits.kg, Value = new decimal(0.1) } } } });
        }
        public static ObservableCollection<Consumable> Consumebles
        {
            get
            {
                return _consumables;
            }
        }

        public static Consumable GetFromName(string s)
        {
            return _consumables.FirstOrDefault(t => t.Name == s);
        }
    }
    public class IndexCalculator
    {
        private Index _index; 
        public IndexCalculator(Index index)
        {
            _index = index;
        }

        public decimal Calculate(IEnumerable<Consumption> consumptions)
        {
            return Calculate(consumptions, this._index.IndexName);
        }
         
        public static decimal Calculate(IEnumerable<KeyValuePair<Consumable, decimal>> selectMany, string index)
        {
            var consumption = selectMany.Select(keyValuePair => new Consumption() {Consumable = keyValuePair.Key, Amount = keyValuePair.Value}).ToList();
            return Calculate(consumption, index);
        }
        
        public static decimal Calculate(IEnumerable<Consumption> consumptions, string index)
        {
            decimal result = 0;
            var indexbase = Indexes.FirstOrDefault(t => t.IndexName == index);
            if (indexbase != null)
            {
                foreach (var consumption in consumptions)
                {
                    if (consumption.Consumable.LciSet != null)
                        foreach (var calc in indexbase.Calculations)
                        {
                            
                            KeyValuePair<string, decimal> calc1 = calc;
                            var em = consumption.Consumable.LciSet.Emissions.AsParallel()
                                       .FirstOrDefault(t => calc1.Key.Contains(t.EmissionName));
                            if (em != null)
                            {
                                result += consumption.Amount*em.Value;
                            }
                        }
                }
            }
            else
            {
                result = consumptions.Where(t => t.Consumable.Name == index).Sum(t => t.Amount);
            }
            return result;
        }

        private static IEnumerable<Index> _indexes;
        public static IEnumerable<Index> Indexes
        {
            get
            {
                if (_indexes != null)
                    return _indexes;
                while (_indexes == null)
                {


                    try
                    {
                        Type type = typeof (Index);
                        
                        var types = System.AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(s => s.GetTypes())
                            .Where(
                                p =>
                                    p.AssemblyQualifiedName != null && type.IsAssignableFrom(p) && p.IsClass &&
                                    !p.IsAbstract).AsParallel();

                        _indexes =
                            types.Select(
                                type1 =>
                                    type1.AssemblyQualifiedName != null
                                        ? (Index) Activator.CreateInstance(Type.GetType(type1.AssemblyQualifiedName))
                                        : null)
                                .ToList();
                    }
                    catch
                    {
                    }
                }
                return _indexes;
            }
        }
    }
    public class EPS:Index
    {
        private readonly IDictionary<string, decimal> _calculation = new Dictionary<string, decimal>();
        public EPS()
        {
            _calculation.Add("co2", (decimal) 0.108);
            _calculation.Add("carbon", (decimal)0.108);
            _calculation.Add("co", (decimal) 0.331);
            _calculation.Add("so2", (decimal) 3.27);
            _calculation.Add("nox", (decimal) 2.13);
            _calculation.Add("pah", (decimal) 64300);
            _calculation.Add("nmvoc", (decimal) 2.14);
            _calculation.Add("methane", (decimal) 2.72);
            _calculation.Add("pm10", (decimal) 36);
        }
        
        public override IDictionary<string, decimal> Calculations { get { return _calculation; } }
        public override string IndexName
        {
            get { return "EPS"; }
        }
    }

    public class CO2 : Index
    {
        private readonly IDictionary<string, decimal> _calcualtions = new Dictionary<string, decimal>();
        public CO2()
        {
            _calcualtions.Add("co2", (decimal)1);
            _calcualtions.Add("carbon", (decimal)1);

        }

        public override IDictionary<string, decimal> Calculations { get { return _calcualtions; } }
        public override string IndexName
        {
            get { return "CO2 Emissions"; }
        }
    }

    public abstract class Index
    {
        public abstract IDictionary<string, decimal> Calculations { get; }
        public abstract string IndexName { get; }
        public decimal Calculation(IEnumerable<Consumption> consumptions)
        {
            return IndexCalculator.Calculate(consumptions, this.IndexName);
        }

        public decimal Calculation(IEnumerable<KeyValuePair<Consumable, decimal>> selectMany)
        {
            var consumption = selectMany.Select(keyValuePair => new Consumption() {Consumable = keyValuePair.Key, Amount = keyValuePair.Value}).ToList();
            return Calculation(consumption);
        }


        public Statistic StatisticCalculation(Dictionary<Consumable, Statistic> consumables)
        {
            var mean = consumables.Select(keyValuePair => new Consumption() { Consumable = keyValuePair.Key, Amount = keyValuePair.Value.Mean }).ToList();
            var std = consumables.Select(keyValuePair => new Consumption() { Consumable = keyValuePair.Key, Amount = keyValuePair.Value.Std }).ToList();
            return new Statistic(Calculation(mean), Calculation(std));
        }
    }
}
