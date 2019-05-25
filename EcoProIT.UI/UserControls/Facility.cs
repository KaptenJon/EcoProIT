using System;

namespace EcoProIT.UI.UserControls
{
    [Serializable]
    public class Facility:ModelNode
    {
        private static int i = 0;
        public Facility():base()
        {

            //NodeImage.Source = HelpClasses.InteropHelp.LoadBitmap(Properties.Resources.Facility);

            ResourceModel.IsFacility = true;

            ResourceModel.ProcessName = "ProductionFacility" + i++;
        }
    }
}
