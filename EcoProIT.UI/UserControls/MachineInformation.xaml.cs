using System.Windows.Controls;

namespace EcoProIT.UI.UserControls
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
