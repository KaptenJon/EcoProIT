using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EcoProIT.DataLayer;
using EcoProIT.UserControles;
using EcoProIT.UserControles.Models;
using EcoProIT.UserControles.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using HelpClasses;

namespace EcoProIT.UI.ViewModel
{
    public  class StatisticModel:ObservableObject
    {
        private IHasResults _results;
        private IEnumerable<string> _indexes;
        private string _selectedIndex;
        private string _selectedType;
        private string _axisUnit;


        public IHasResults Results
        {
            get { return _results; }
            set { 
                _results = value; 
                OnPropertyChanged("Result");
                Indexes = Results.Result.Consumables.Select(t => t.Key.Name).Union(IndexCalculator.Indexes.Select(t=>t.IndexName));
                OnPropertyChanged("Modelnode");
            }
        }
        public ResourceDefinitionModel Modelnode
        {
            get { return _results as ResourceDefinitionModel; }

        }

        public IEnumerable<ModelNode> Modelnodes { get; set; }
        public IEnumerable<String> Typeof
        {
            get { return new[] {"Compare nodes", "History", "Compare products"}; }
        }

        public string SelectedType
        {
            set { _selectedType = value;
            OnPropertyChanged("SelectedType");
            OnPropertyChanged("SelectedDataSet");
            OnPropertyChanged("NameOfDiagram");
            }
            get { return _selectedType;
            
            }
        }

        public Dictionary<string, decimal> SelectedDataSet
        {
            get
            {
                if (Results?.Result == null || SelectedIndex == null || SelectedType == null)
                    return null;

                AxisUnit = Results.Result.Consumables.Any(t => t.Key.Name == SelectedIndex) ?  Results.Result.Consumables.FirstOrDefault(t => t.Key.Name == SelectedIndex).Key.PerUnit.ToString():"";
                switch (SelectedType)
                {
                    case "Compare nodes":
                        if (Modelnodes == null) return null;
                        return Modelnodes.ToDictionary(p =>  p.ResourceModel.ProcessName,
                                                i => IndexCalculator.Calculate(i.ResourceModel.Result.MeanConsumables,SelectedIndex));
                    //NO STD!!!!
                    case "History":
                        return Results.Result.PerTime((ulong) (IResults.TotalTime/50), SelectedIndex);//.ToDictionary(t=>t.Key, t =>new Statistic(t.Value,0));
                    ///NO STD!!!
                    case "Compare products":
                        if (Modelnode == null) return null;
                        return Results.Result.PerProduct(Modelnode.RelatedProducts, SelectedIndex);
                }
                return null;
            }
        }
        public IEnumerable<String> Indexes
        {
            get { return _indexes; }
            set { _indexes = value;
            OnPropertyChanged("Indexes");
            }
        }

        public string AxisUnit
        {
            get { return _axisUnit; }
            set { _axisUnit = value;
            OnPropertyChanged("AxisUnit");
            }
        }

        public string SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value;
            OnPropertyChanged("NameOfDiagram");
            OnPropertyChanged("SelectedDataSet");
            OnPropertyChanged("SelectedIndex");
            }
        }

        

        public string NameOfDiagram
        {
            get
            {
                if (Results?.Result == null || SelectedIndex == null || SelectedType == null)
                    return null;

                var duringtime = "";
                if (SelectedIndex == "Processed")
                    duringtime = " during " + (IResults.TotalTime / 3600000).ToString(CultureInfo.InvariantCulture) + " hours";
                if (SelectedType == "History")
                    return SelectedType + " of " + SelectedIndex + " for " + Results.Result.ResultId;
                if (SelectedType == "Compare nodes")
                    return "Comparison of " + SelectedIndex  + duringtime;
                if (SelectedType == "Compare products")
                    return "Comparison of " + SelectedIndex + " for products in " + Modelnode?.ProcessName +  duringtime;
                return SelectedIndex;
            }

        }
    }
}
