/*
  In App.xaml:
  <Application.Resources>
      <vm:MvvmViewModelLocator xmlns:vm="clr-namespace:EcoProIT.UI.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>

  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using System.ComponentModel;
using System.Windows;
using EcoProIT.UI.Model;

namespace EcoProIT.UI.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private static readonly MainViewModel _main;

        static ViewModelLocator()
        {
            IDataService dataService;
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                dataService = new Design.DesignDataService();
            }
            else
            {
                dataService = new DataService();
            }

            _main = new MainViewModel(dataService);
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainViewModel Main
        {
            get
            {
                return _main;
            }
        }
    }
}