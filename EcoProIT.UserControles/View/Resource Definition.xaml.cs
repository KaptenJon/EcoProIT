using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EcoProIT.UserControles.View
{
    /// <summary>
    /// Interaction logic for Resource_Definition.xaml
    /// </summary>
    public partial class Resource_Definition : UserControl, INotifyPropertyChanged
    {
        public Resource_Definition()
        {
            InitializeComponent();

            // Insert code required on object creation below this point.
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion



        private void ProcessName_MouseDoubleClick_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ProcessName.Visibility = Visibility.Hidden;
            ChangeName.Visibility = Visibility.Visible;
            ChangeName.Focus();
        }

        private void UserControl_KeyDown_1(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessName.Visibility = Visibility.Visible;
                ChangeName.UpdateLayout();
                ChangeName.Visibility = Visibility.Hidden;
            }
        }

        private void Cancel_Click_1(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);


            DialogResult = false;
            if (window != null) window.Close();

        }

        public bool DialogResult = false;
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            
            DialogResult = true;
            if (window != null) window.Close();


        }
    }


}