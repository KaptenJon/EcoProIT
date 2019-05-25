using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcoProIT.DataLayer;

namespace EcoProIT.UserControles
{
    public class Transport:ModelNode
    {
        private static int i = 0;
        
        public Transport():base()
        {
            NodeImage.Source = HelpClasses.InteropHelp.LoadBitmap(Properties.Resources.Transport);
            ResourceModel.Capacity = 100;
            ResourceModel.ProcessName = "Transport" + i++;
            ResourceModel.HasBreakdown = true;
            ResourceModel.IsTransport = true;
            ResourceModel.Speed = 1;
            ResourceModel.Length = 5;
        }
    }
}
