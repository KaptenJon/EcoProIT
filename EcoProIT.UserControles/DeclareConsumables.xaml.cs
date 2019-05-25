using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EcoProIT.DataLayer;

namespace EcoProIT.UserControles
{
    #region New region

    /// <summary>
    /// Interaction logic for DeclareConsumables.xaml
    /// </summary>
    public partial class DeclareConsumables : UserControl,INotifyPropertyChanged
    {
        public DeclareConsumables()
        {
            InitializeComponent();
        }
        public DeclareConsumables(string name):this()
        {
            Consumable = new Consumable() {Name = name};
            
        }

        public Consumable Consumable { get; set; }

        public string ConsumableName
        {
            get { return Consumable.Name; }
        }

        private double _amount;
        public double Amount
        {
            get { return _amount; }
            set { _amount = value; NotifyPropertyChanged("Amount"); }
        }

        private GeneralConverter _selectedConsumableConverter;

        public GeneralConverter SelectedConsumableConverter
        {
            get { return _selectedConsumableConverter; }
            set
            {
                _selectedConsumableConverter = value;
                NotifyPropertyChanged("SelectedGeneralConverter");
            }
        }
        private GeneralConverter _selectedTimeConverter;
        
        public GeneralConverter SelectedTimeConverter
        {
            get { return _selectedTimeConverter; }
            set
            {
                _selectedTimeConverter = value;
                NotifyPropertyChanged("SelectedTimeConverter");
            }
        }
        private string _selectedState;
        public string SelectedState { get { return _selectedState; } set { _selectedState = value; NotifyPropertyChanged("SelectedState"); } }

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    #endregion
}
