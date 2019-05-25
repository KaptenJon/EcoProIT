using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Windows.Shapes;
using EcoProIT.DataLayer.Annotations;
using React.Distribution;


namespace EcoProIT.DataLayer
{
    [Serializable]
    public enum SIUnits
    {
        Kg, L, s, Piece, kW, kWh
    }
    [Serializable]
    public class Consumable:IComparable<Consumable>,IEquatable<Consumable>
    {
        public string Name { get; set; }
        public SIUnits PerUnit { get; set; }
        public LciSet LciSet { get; set; }
        public int CompareTo(Consumable other)
        {
            return System.String.Compare(Name, other.Name, System.StringComparison.Ordinal);
        }

        public bool Equals(Consumable other)
        {
            return String.Equals(Name,other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
    [Serializable]
    public class LciSet
    {
        public List<Emission> Emissions { get; set; }
    }
    [Serializable]
    public class Emission
    {
        public string EmissionName { get; set; }
        public decimal Value{get; set;}
        public SIUnits SIUnit{get; set;}

        
    }
    [Serializable]
    public class Job:INotifyPropertyChanged
    {
        public ObservableCollection<SubJob> Subjobs = new ObservableCollection<SubJob>();
        private String _name;

        public String Name
        {
            get { return _name; }
            set
            {
                if (_name == value)
                    return;
                _name = value;
                OnPropertyChanged();
            }
        }
        public override string ToString()
        {
            return Name;
        }
        public decimal Quality
        {
            get { return _quality; }
            set 
            { 
                _quality = value;
                OnPropertyChanged();
            }
        }

        public State _state = new State("Processing");
        private decimal _quality=100;

        public State State
        {
            get { return _state; }
            set
            {
                if (_state == value)
                    return;
                _state = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class State:INotifyPropertyChanged   
    {
        public State(String stateName)
        {
            _name = stateName;
        }

        private String _name;

        public String Name
        {
            get { return _name; }
            set
            {
                if (_name == value)
                    return;
                _name = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<Consumption> _consumptions = new ObservableCollection<Consumption>();

        public ObservableCollection<Consumption> Consumptions
        {
            get { return _consumptions; }
            set
            {
                if (_consumptions == value)
                    return;
                _consumptions = value;
               OnPropertyChanged("Consumptions");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Serializable]
    public class Consumption : INotifyPropertyChanged
    {
        private Consumable _consumable = ConnectLCIDB.Consumebles.FirstOrDefault();

        public Consumable Consumable
        {
            get { return _consumable; }
            set
            {
                _consumable = value;
                OnPropertyChanged("Consumable");
            }
        }

        private bool _static = false;

        public bool Static
        {
            get { return _static; }
            set
            {
                if (_static == value)
                    return;
                _static = value;
                OnPropertyChanged("Static");
                OnPropertyChanged("UnitType");
            }
        }

        private bool _allocationPerTime = false;

        public bool AllocationPerTime
        {
            get { return _allocationPerTime; }
            set
            {
                if (_allocationPerTime == value)
                    return;
                _allocationPerTime = value;
                OnPropertyChanged("AllocationPerTime");
               
            }
        }


        public String UnitType
        {
            get
            {
                if (Consumable == null)
                    return"";

                if (Static)
                {
                    return Consumable.PerUnit + "";
                }
                if (Consumable.PerUnit == SIUnits.kWh)
                    return SIUnits.kW+"";
                return Consumable.PerUnit + "/h";
            }
        }

        private decimal _amount;

        public decimal Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;
                OnPropertyChanged("Amount");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public Consumption Copy()
        {
            return new Consumption()
                {
                    _allocationPerTime = this._allocationPerTime,
                    _consumable = this.Consumable,
                    _amount = this.Amount,
                    _static = this.Static
                };
        }
    }
    [Serializable]
    public class SubJob :IHasDistribution, INotifyPropertyChanged
    {
        private IDistribution _distribution = new NormalDistribution(){Mean = 1, Std = 0};

        [NotNull]
        public IDistribution Distribution
        {
            get { return _distribution; }
            set
            {
                _distribution = value;
                OnPropertyChanged("Distribution");
            }
        }
        private String _description;

        public String Description
        {
            get { return _description; }
            set
            {
                if (_description == value)
                    return;
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        public override string ToString()
        {
            return _distribution.ToString();
        }

        public string AutomodCode { get { return _distribution.AutomodCode; } }

        public SubJob Copy()
        {
            return new SubJob()
                {
                    Distribution = Distribution.Copy(),
                    _description = Description
                };
        }
    }

    public interface IHasDistribution:INotifyPropertyChanged
    {
        IDistribution Distribution { get; set; }
    }

    #region Distribution

    public abstract class IDistribution:INotifyPropertyChanged, IRandom
    {
        public abstract Distribution CMSDDistribution { get; set; }
        public abstract String AutomodCode { get; }
        public abstract String[] ParameterNames { get; }
        public abstract String Name { get; }
        public IRandom DESRandom { protected set; get; }
        

        protected IFormatProvider Format = new NumberFormatInfo(){NumberDecimalSeparator = "."};

        public String ShowParameters
        {
            get { return ToString(); }
        }

        protected GeneralConverter _prameterConverter = new GeneralConverter(UnitTypes.Seconds);

        public GeneralConverter ParameterConverter
        {
            get { return _prameterConverter; }
            set
            {
                _prameterConverter = value;
                OnPropertyChanged();
            }
        }

       

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public static IDistribution GetFromCmsd(Distribution distsource)
        {
            double par1 = 0, par2 = 0;
            
            switch (distsource.Name.ToLowerInvariant())
            {
                case "normal":
                    foreach (var distributionDistributionParameter in distsource.DistributionParameter)
                    {
                        switch (distributionDistributionParameter.Name.ToLowerInvariant())
                        {
                            case "mean":
                                par1 = double.Parse(distributionDistributionParameter.Value);
                                break;
                            case "std":
                                par2 = double.Parse(distributionDistributionParameter.Value);
                                break;
                        }
                    }
                    return new NormalDistribution(){Mean = par1, Std = par2};
                case "uniform":
                    foreach (var distributionDistributionParameter in distsource.DistributionParameter)
                    {
                        switch (distributionDistributionParameter.Name.ToLowerInvariant())
                        {
                            case "lowerlimit":
                                par1 = double.Parse(distributionDistributionParameter.Value);
                                break;
                            case "upperlimit":
                                par2 = double.Parse(distributionDistributionParameter.Value);
                                break;
                        }
                    }
                    return new Uniform() { LowerLimit  = par1, UpperLimit= par2 };
                case "lognormal":
                    foreach (var distributionDistributionParameter in distsource.DistributionParameter)
                    {
                        switch (distributionDistributionParameter.Name.ToLowerInvariant())
                        {
                            case "scale":
                                par1 = double.Parse(distributionDistributionParameter.Value);
                                break;
                            case "shape":
                                par2 = double.Parse(distributionDistributionParameter.Value);
                                break;
                        }
                    }
                    return new LogNormalDistribution() { Scale = par1,  Shape = par2 };
                case "constant":
                    foreach (var distributionDistributionParameter in distsource.DistributionParameter)
                    {
                        switch (distributionDistributionParameter.Name.ToLowerInvariant())
                        {
                            case "mean":
                                par1 = double.Parse(distributionDistributionParameter.Value,CultureInfo.InvariantCulture);
                                break;
                        }
                    }
                    return new Constant() { Mean = par1};
                case "exponential":
                    foreach (var distributionDistributionParameter in distsource.DistributionParameter)
                    {
                        switch (distributionDistributionParameter.Name.ToLowerInvariant())
                        {
                            case "mean":
                                par1 = double.Parse(distributionDistributionParameter.Value,CultureInfo.InvariantCulture);
                                break;
                        }
                    }
                    return new ExponentionDistribution() { Mean = par1 };
                    
            }
            return null;
        }

        public abstract IDistribution Copy();
        public double NextDouble()
        {
            if (DESRandom != null)
            {
                return DESRandom.NextDouble();
            }
            System.Diagnostics.Debug.Assert(true, "DESDist not initialized");
            return 0;
        }

        public float NextSingle()
        {
            if (DESRandom != null)
            {
                return (DESRandom.NextSingle());
            }
            System.Diagnostics.Debug.Assert(true, "DESDist not initialized");
            return 0;
        }

        public ulong Nextulong()
        {
            if (DESRandom != null)
            {
                return DESRandom.Nextulong();
            }
            System.Diagnostics.Debug.Assert(true, "DESDist not initialized");
            return 0;
        }
    }


    [Serializable]
    public class Uniform : IDistribution
    {
        public Uniform()
        {
            ParameterConverter.Add(0);
            ParameterConverter.Add(0);
            ParameterConverter.CollectionChanged += ParameterConverter_CollectionChanged;
        }

        void ParameterConverter_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("UpperLimit");
            OnPropertyChanged("LowerLimit");
        }
        //lowerlimit par 0
        //upperlimit par 1
        public double UpperLimit
        {
            get { return ParameterConverter[0]; }
            set { ParameterConverter[0] = value;

            DESRandom = new Uniform() { LowerLimit = ParameterConverter.GetStandard(0), UpperLimit = ParameterConverter.GetStandard(1) };
            OnPropertyChanged();
            OnPropertyChanged("AutomodCode");
            OnPropertyChanged("CMSDDistribution");
            OnPropertyChanged("ShowParameters");
            }
        }

        public double LowerLimit
        {
            get { return ParameterConverter[1]; }
            set
            {
                ParameterConverter[1] = value;
                DESRandom = new Uniform() { LowerLimit = ParameterConverter.GetStandard(0), UpperLimit = ParameterConverter.GetStandard(1) };
                OnPropertyChanged();
                OnPropertyChanged("AutomodCode");
                OnPropertyChanged("CMSDDistribution");
                OnPropertyChanged("ShowParameters");
            }
        }

        public override string ToString()
        {
            return "Uniform, LowerLimit: " + LowerLimit + " UpperLimit: " + UpperLimit + " " +ParameterConverter.AutomodCode ;
        }

        public override Distribution CMSDDistribution
        {
            get
            {
                return new Distribution()
                {
                    Name = "Uniform",
                    DistributionParameter =
                        new[]
                                {
                                    new DistributionDistributionParameter() {Name = "LowerLimit", Value = LowerLimit + ""},
                                    new DistributionDistributionParameter() {Name = "UpperLimit", Value = UpperLimit + ""}
                                }
                };
            }
            set
            {
                foreach (var v in value.DistributionParameter)
                {
                    switch (v.Name)
                    {
                        case "LowerLimit":
                            LowerLimit = double.Parse(v.Value);
                            break;
                        case "UpperLimit":
                            UpperLimit = double.Parse(v.Value);
                            break;
                    }
                }
            }
        }

        public override string AutomodCode { get { return "u " + ((LowerLimit + UpperLimit) / 2).ToString(Format) + ", " + (UpperLimit - LowerLimit).ToString(Format) + " " + ParameterConverter.AutomodCode ; } }
        public override string[] ParameterNames { get { return new string[] { "UpperLimit", "LowerLimit" }; } }
        public override string Name { get { return "Uniform"; } }
        

        public override IDistribution Copy()
        {
            return new Uniform()
                {
                    ParameterConverter = new GeneralConverter(ParameterConverter.ToArray(),ParameterConverter.UnitType)
                };
        }
    }
    [Serializable]
    public class NormalDistribution : IDistribution
    {

        public NormalDistribution()
        {
            ParameterConverter.Add(0);
            ParameterConverter.Add(0);
            ParameterConverter.CollectionChanged += ParameterConverter_CollectionChanged;
        }

        void ParameterConverter_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Mean");
            OnPropertyChanged("Std");
        }
        //mean par 0
        //std par 1

        public double Mean
        {
            get { return ParameterConverter[0]; }
            set { ParameterConverter[0] = value;
            DESRandom = new Normal(ParameterConverter.GetStandard(0),ParameterConverter.GetStandard(1));
            OnPropertyChanged();
            OnPropertyChanged("AutomodCode");
            OnPropertyChanged("CMSDDistribution");
            OnPropertyChanged("ShowParameters");
            }
        }

        public double Std
        {
            get { return ParameterConverter[1]; }
            set {  ParameterConverter[1] = value;
            DESRandom = new Normal(ParameterConverter.GetStandard(0),ParameterConverter.GetStandard(1));
            OnPropertyChanged();
            OnPropertyChanged("AutomodCode");
            OnPropertyChanged("CMSDDistribution");
            OnPropertyChanged("ShowParameters");
            }
        }

        public override string[] ParameterNames { get { return new string[] { "Mean", "Std" }; } }
        public override string Name { get { return "Normal"; } }
        public override IDistribution Copy()
        {
            
            return new NormalDistribution()
                {
                    ParameterConverter = new GeneralConverter(ParameterConverter.ToArray(),ParameterConverter.UnitType)
                };
        }

        public override string ToString()
        {
            return "Normal, Mean: " + Mean + " Std: " + Std;
        }
        public override string AutomodCode { get { return "n " + (Mean.ToString(Format)) + ", " + (Std.ToString(Format)) + " " + ParameterConverter.AutomodCode; } }
        public override Distribution CMSDDistribution
        {
            get
            {
                return new Distribution()
                    {
                        Name = "Normal",
                        DistributionParameter =
                            new[]
                                {
                                    new DistributionDistributionParameter() {Name = "Mean", Value = Mean + ""},
                                    new DistributionDistributionParameter() {Name = "Std", Value = Std + ""}
                                }
                    };
            }
            set
            {
                foreach (var v in value.DistributionParameter)
                {
                    switch (v.Name)
                    {
                        case "Mean":
                            Mean = double.Parse(v.Value);
                            break;
                        case "Std":
                            Std = double.Parse(v.Value);
                            break;
                    }
                }
            }
        }
    }
    [Serializable]
    public class Constant : IDistribution
    {
        public Constant()
        {
            ParameterConverter.Add(0);
            ParameterConverter.CollectionChanged += ParameterConverter_CollectionChanged;
        }

        void ParameterConverter_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Constant");

        }
        public double Mean
        {
            get { return ParameterConverter[0]; }
            set
            {
                ParameterConverter[0] = value;
                DESRandom = new React.Distribution.Constant(ParameterConverter.GetStandard(0));
                OnPropertyChanged();
                OnPropertyChanged("AutomodCode");
                OnPropertyChanged("CMSDDistribution");
                OnPropertyChanged("ShowParameters");
            }
        }

        public override string[] ParameterNames { get { return new string[] { "Mean" }; } }
        public override string Name { get { return "Constant"; } }
        public override IDistribution Copy()
        {
            return new Constant() { ParameterConverter = new GeneralConverter(ParameterConverter.ToArray(), ParameterConverter.UnitType) };
        }

        public override string ToString()
        {
            return "Constant, " + "Mean: " + Mean;
        }
        public override string AutomodCode { get { return (Mean.ToString(Format)) + " " + ParameterConverter.AutomodCode; } }
        public override Distribution CMSDDistribution
        {
            get
            {
                return new Distribution()
                {
                    Name = "Constant",
                    DistributionParameter =
                        new[]
                                {
                                    new DistributionDistributionParameter() {Name = "Mean", Value = Mean.ToString(Format) + ""}
                                }
                };
            }
            set
            {
                foreach (var v in value.DistributionParameter)
                {
                    switch (v.Name)
                    {
                        case "Mean":
                            Mean = double.Parse(v.Value);
                            break;
                    }
                }
            }

        }
        

    }
    public class ExponentionDistribution : IDistribution
    {
        public ExponentionDistribution()
        {
            ParameterConverter.Add(0);
            ParameterConverter.CollectionChanged += ParameterConverter_CollectionChanged;
        }

        void ParameterConverter_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Mean");
        }
        public double Mean
        {
            get { return ParameterConverter[0]; }
            set
            {
                ParameterConverter[0] = value;
                DESRandom = new Exponential(ParameterConverter.GetStandard(0));
                ParameterConverter.CollectionChanged += ParameterConverterOnCollectionChanged;
                OnPropertyChanged();
                OnPropertyChanged("AutomodCode");
                OnPropertyChanged("CMSDDistribution");
                OnPropertyChanged("ShowParameters");
            }
        }

        private void ParameterConverterOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            DESRandom = new Exponential(ParameterConverter.GetStandard(0));
        }


        public override string[] ParameterNames { get { return new string[] { "Mean" }; } }
        public override string Name { get { return "Exponential"; } }
        public override IDistribution Copy()
        {
            return new ExponentionDistribution() { ParameterConverter = new GeneralConverter(ParameterConverter.ToArray(), ParameterConverter.UnitType)};
        }

        public override string ToString()
        {
            return "Exponential, " + "Mean: " + Mean;
        }
        public override string AutomodCode { get { return "e " + (Mean.ToString(Format)) + " " + ParameterConverter.AutomodCode; } }
        public override Distribution CMSDDistribution
        {
            get
            {
                return new Distribution()
                {
                    Name = "Exponential",
                    DistributionParameter =
                        new[]
                                {
                                    new DistributionDistributionParameter() {Name = "Mean", Value = Mean.ToString(Format) + ""}
                                }
                };
            }
            set
            {
                foreach (var v in value.DistributionParameter)
                {
                    switch (v.Name)
                    {
                        case "Mean":
                            Mean = double.Parse(v.Value);
                            break;
                    }
                }
            }

        }
        

    }
    [Serializable]
    public class LogNormalDistribution : IDistribution
    {
        public LogNormalDistribution()
        {
            
            ParameterConverter.Add(0);
            ParameterConverter.Add(0);

            ParameterConverter.CollectionChanged += ParameterConverter_CollectionChanged;
        }

        void ParameterConverter_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Scale");
            OnPropertyChanged("Shape");
        }
        //Scale Par0 
        //Shape par1
        
        public double Scale
        {
            get { return ParameterConverter[0]; }
            set
            {
                if (value.Equals(ParameterConverter[0])) return;
                ParameterConverter[0] = value;
                DESRandom = new Lognormal(true, ParameterConverter.GetStandard(0), ParameterConverter.GetStandard(1));
                OnPropertyChanged();
                OnPropertyChanged("AutomodCode");
                OnPropertyChanged("CMSDDistribution");
                OnPropertyChanged("ShowParameters");
            }
        }

        public double Shape
        {
            get { return ParameterConverter[1]; }
            set
            {
                if (value.Equals(ParameterConverter[1])) return;
                ParameterConverter[1] = value;
                OnPropertyChanged();
                OnPropertyChanged("AutomodCode");
                OnPropertyChanged("CMSDDistribution");
                OnPropertyChanged("ShowParameters");
            }
        }

        public override string[] ParameterNames { get { return new string[] { "Scale", "Shape" }; } }
        public override string Name { get { return "LogNormal"; } }
        public override IDistribution Copy()
        {
            return new LogNormalDistribution() { ParameterConverter = new GeneralConverter(ParameterConverter.ToArray(), ParameterConverter.UnitType) };
        }

        public override string ToString()
        {
            return "LogNormal, " + "Scale: " + Scale + " Shape: " + Shape;
        }
        public override string AutomodCode { get { return "lognormal " + Scale.ToString(Format) + ", " + Shape.ToString(Format) + " " + ParameterConverter.AutomodCode; } }
        public override Distribution CMSDDistribution
        {
            get
            {
                return new Distribution()
                {
                    Name = "LogNormal",
                    DistributionParameter =
                        new[]
                                {
                                    new DistributionDistributionParameter() {Name = "Scale", Value = Scale + ""},
                                    new DistributionDistributionParameter() {Name = "Shape", Value = Shape + ""}
                                }
                    };
            }
            set
            {
                foreach (var v in value.DistributionParameter)
                {
                    switch (v.Name)
                    {
                        case "Scale":
                            Scale = double.Parse(v.Value);
                            break;
                        case "Shape":
                            Shape = double.Parse(v.Value);
                            break;
                    }
                }
            }

        }

    }
    #endregion

}
