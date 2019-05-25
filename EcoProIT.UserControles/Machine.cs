using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EcoProIT.DataLayer;
using HelpClasses;

namespace EcoProIT.UserControles
{
    [Serializable]
    public class Machine:ModelNode
    {
        private static int i = 0;
        
        public Machine():base()
        {
            //NodeImage.Source = HelpClasses.InteropHelp.LoadBitmap(Properties.Resources.Machine);
            DeclaredJobs.Clear();
            DeclaredJobs.Add(new Job()
            {
                Name = "Default",
                Subjobs =
                    new ObservableCollection<SubJob>(new SubJob[] { new SubJob() { Distribution = new NormalDistribution() { Mean = 10, Std = 2 },} })
            });
            ResourceModel.Capacity = 1;
            ResourceModel.ProcessName = "Machine" + i++;
            ResourceModel.IsProcess = true;
        }
    }
}
