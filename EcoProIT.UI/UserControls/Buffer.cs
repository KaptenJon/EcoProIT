using System;

namespace EcoProIT.UI.UserControls
{
    [Serializable]
    public class Buffer:ModelNode
    {
        private static int i = 0;

        public Buffer():base()
        {

            //NodeImage.Source = HelpClasses.InteropHelp.LoadBitmap(Properties.Resources.Buffer);
              ResourceModel.Capacity = 10;
            ResourceModel.ProcessName = "Buffer" + i++;
            ResourceModel.IsBuffer = true;
        }

        
    }
}
