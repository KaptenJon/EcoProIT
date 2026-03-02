using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using EcoProIT.DataLayer;

namespace EcoProIT.UserControles
{
    [Serializable]
    public class Buffer:ModelNode
    {
        private static int i = 0;

        public Buffer():base()
        {

            NodeImage.Source = new BitmapImage(new Uri("pack://application:,,,/EcoProIT.UserControles;component/Resources/Buffer.png"));
              ResourceModel.Capacity = 10;
            ResourceModel.ProcessName = "Buffer" + i++;
        }

        
    }
}
