using System;
using System.Collections.Generic;
using System.Linq;
using EcoProIT.UserControles;
using EcoProIT.UserControles.ViewModel;
using GalaSoft.MvvmLight;

namespace EcoProIT.UI.ViewModel
{
    public class StatisticProductModel:ViewModelBase
    {
  
        private Product _results;
        private IEnumerable<string> _indexes;
        private string _selectedIndex;
        private string _selectedType;
        public Product Results
        {
            get { return _results; }
            set { 
                _results = value; 
                RaisePropertyChanged("Result");
                var set = Results.Result.Consumables.Select(t => t.Key.Name).ToList();
                set.Add("Processed per hour");
                set.Add("Processed");
                Indexes = set;
            }
        }
        public IEnumerable<ModelNode> Modelnodes { get; set; }
        public IEnumerable<String> Typeof
        {
            get { return new[] {"Compare Sources", "History"}; }
        }

        public string SelectedType
        {
            set { _selectedType = value;
            RaisePropertyChanged("SelectedType");
            RaisePropertyChanged("SelectedDataSet");
            RaisePropertyChanged("NameOfDiagram");
            }
            get { return _selectedType;
            
            }
        }

        public Dictionary<string, decimal> SelectedDataSet
        {
            get
            {
                switch (SelectedType)
                {
                    case "Compare Sources":
                        var dic = Modelnodes.ToDictionary(p =>  p.ResourceModel.ProcessName,
                                                i =>
                                                i.ResourceModel.Result.CalculateForOneProduct(Results,SelectedIndex));
                        if (Results.Nodes.Count > 0)
                        {
                            //var count = Results.Nodes.Last().Key.ResourceModel.Result.TotalProduced(0,UInt64.MaxValue, Results.ProductName);
                            var res = dic.ToDictionary(i => i.Key, i => i.Value);/// (SelectedIndex.StartsWith("Processed")?1:count)
                            dic = res;
                        }
                        dic.Add("Product Materials", Results.Result.CalculateForOneProduct(Results, SelectedIndex));
                        for(int i =0 ; i < dic.Count ; i++)
                        {
                            if (dic.ElementAt(i).Value == 0)
                            {
                                dic.Remove(dic.ElementAt(i).Key);
                                i--;
                            }
                        }
                        return dic;
                    case "History":
                        return Results.Result.PerTime(3600000,SelectedIndex);
                }
                return null;
            }
        }
        public IEnumerable<String> Indexes
        {
            get { return _indexes; }
            set { _indexes = value;
            RaisePropertyChanged("Indexes");
            }
        }

        public string SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value;
            RaisePropertyChanged("NameOfDiagram");
            RaisePropertyChanged("SelectedDataSet");
            RaisePropertyChanged("SelectedIndex");
            }
        }

        public string NameOfDiagram
        {
            get {
                if (SelectedType == "History")
                    return SelectedType + " of " + SelectedIndex + " for " + Results.Result.ResultId;
                if (SelectedType == "Compare sources")
                    return "Comparison of " + SelectedIndex;
                return SelectedIndex;
            }

        }
    }
    
}