using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EcoProIT.UserControles.ViewModel;

namespace EcoProIT.UserControles
{
    /// <summary>
    /// Description for Products.
    /// </summary>
    public partial class Products : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the Products class.
        /// </summary>
        public Products()
        {
            InitializeComponent();
        }

        protected void SelectCurrentItem(object sender, KeyboardFocusChangedEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;
            _listBox.SelectedItem = null;
            //Keyboard.Focus(item);
            item.IsSelected = false;
            item.IsSelected = true;
        }

        private void CurrentItemClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;
            _listBox.SelectedItem = null;
            item.IsSelected = false;
            item.IsSelected = true;
        }
    }
}