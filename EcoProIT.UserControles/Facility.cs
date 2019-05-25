using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using EcoProIT.DataLayer;

namespace EcoProIT.UserControles
{
    [Serializable]
    public class Facility:ModelNode
    {
        private static int i = 0;
        public Facility():base()
        {

            NodeImage.Source = HelpClasses.InteropHelp.LoadBitmap(Properties.Resources.Facility);
            

            ResourceModel.ProcessName = "ProductionFacility" + i++;
        }
    }
}
