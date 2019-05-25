using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace EcoProIT.UI.UserControls
{
    public class DragBehavior : Behavior<UIElement>
    {
        private Point elementStartPosition;
        private Point mouseStartPosition;
        private TranslateTransform transform = new TranslateTransform();
        private MouseEventHandler _dragBegun;
        private Control node;

        public MouseEventHandler DragBegun
        {
            get { return _dragBegun; }
            set { _dragBegun = value; }
        }

        public MouseEventHandler DragFinished { set; get; }
        
        public double X { get; set; }
        public double Y { get; set; }

        public bool SpecialNode { get;
            set;
        }


        protected override void OnAttached()
        {
            
            Window parent = Application.Current.MainWindow;
            node = (AssociatedObject as Control);
            if (node == null)
                return;
            node.RenderTransform = transform;
            
            node.MouseLeftButtonDown += (sender, e) =>
            {
                if (SpecialNode)
                    return;
                elementStartPosition = node.TranslatePoint(new Point(), parent);
                mouseStartPosition = e.GetPosition(parent);
                node.CaptureMouse();
                if (DragBegun != null)
                    DragBegun(this, new MouseEventArgs(e.MouseDevice, 0));
            };

            node.MouseLeftButtonUp += (sender, e) =>
            {
                if (SpecialNode)
                    return;
                node.ReleaseMouseCapture();
                Vector diff = e.GetPosition(parent) - mouseStartPosition;
                transform.X = 0;
                transform.Y = 0;
                node.Margin = new Thickness(node.Margin.Left + diff.X, node.Margin.Top + diff.Y, 0, 0);
                if (DragFinished != null)
                    DragFinished(this, new MouseEventArgs(e.MouseDevice, 0));
            };

            node.MouseMove += (sender, e) =>
            {
                if (SpecialNode)
                    return;
                Vector diff = e.GetPosition(parent) - mouseStartPosition;
                if (node.IsMouseCaptured)
                {
                    transform.X = diff.X;
                    transform.Y = diff.Y;
                }
            };
        }
    }
}