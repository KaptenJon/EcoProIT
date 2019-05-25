using System;
using System.Collections.Generic;
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

namespace EcoProIT.UserControles
{
    /// <summary>
    /// Interaction logic for MachineInformation.xaml
    /// </summary>
    public partial class MachineInformation : UserControl
    {
        public MachineInformation()
        {
            InitializeComponent();
            Info.Content =
                "EmissionName: Process B\r\nProduct A:\r\n  Cycle Total = 102 sec\r\n  Set-Up = normal 300, 25 sec\r\nProduct B:\r\n   Cycle Total = 89 sec\r\n   Set-Up = normal 350, 20 sec\r\nTTB = exponential 10 hour\r\nTTR = exponential 0.5 hour";
        }
    }
}
