using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EcoProIT.DataLayer;
using EcoProIT.UserControles.View;
using EcoProIT.UserControles.ViewModel;
using GalaSoft.MvvmLight.Threading;
using HelpClasses;
using BooleanToVisibilityConverter = System.Windows.Controls.BooleanToVisibilityConverter;
using WindowStartupLocation = System.Windows.WindowStartupLocation;

namespace EcoProIT.UserControles
{
    /// <summary>
    /// Interaction logic for ModelNode.xaml
    /// </summary>

    
    public partial class ModelNode : UserControl, INotifyPropertyChanged
    {

        private ResourceDefinitionModel _nodeDef;
        private ModelNode _parentNode;
        private Path _line;
        private MachineInformation _infoBox = new MachineInformation() {Visibility = Visibility.Hidden};
        private Thickness _previousPoint;
        private static Resource_Definition _nodeDefView = new Resource_Definition() ;
        private bool _specialNode;
        
  

        
        
        public ModelNode()
        {
            InitializeComponent();
            _nodeDef = (ResourceDefinitionModel)DataContext;
            ResourceModel.Capacity = 10000000;
            VerticalAlignment = VerticalAlignment.Top;
            HorizontalAlignment = HorizontalAlignment.Left;
            Height = 76;
            Width = 110;
            ResourceModel.IsProcess = false;
            DeclaredJobs.Add(new Job()
            {
                Name = "Default",
                State = new State("Used")
            });
        }


        public List<Label> ProductLabels
        {
            get
            {
                
                var list = new List<Label>();
                if(ResourceModel != null)
                foreach (Product relatedProduct in ResourceModel.RelatedProducts)
                {
                    for (var i = 1; i<= relatedProduct.Nodes.Count; ++i)
                    {
                        if (relatedProduct.Nodes[i - 1].Key == this)
                        {
                            var l = new Label {Content = i + ""};

                            var bind = new Binding("ColorBrush")
                                {
                                    Source = relatedProduct
                                };
                            l.SetBinding(ForegroundProperty, bind);
                            bind = new Binding("CheckChanged")
                                {
                                    Source = relatedProduct,
                                    Converter = new BooleanToVisibilityConverter()
                                };
                            l.SetBinding(VisibilityProperty, bind);


                            l.FontSize = 20;
                            list.Add(l);
                        }
                    }
                }
                return list;
            }
        }
        public void UpdateProductLabels()
        {
            OnPropertyChanged("ProductLabels");
        }

        public bool SpecialNode
        {
            get { return _specialNode; }
            set
            {
                MovedPosition.SpecialNode = true;

                _specialNode = value;
            }

        }



        public ModelNode(ModelNode parentNode, bool specialNode)
            : base()
        {
            
            ParentNode = parentNode;
            SpecialNode = specialNode;
        }

        public Resource_Definition ResourceView
        {
            get { return _nodeDefView; }
        }

        public ResourceDefinitionModel ResourceModel
        {
            get { return _nodeDef; }
        }



        public ObservableCollection<Job> DeclaredJobs
        {
            get { return ResourceModel.Job; }
        }

        public String Text
        {
            set { ResourceModel.ProcessName = value; }
            get { return ResourceModel.ProcessName; }
        }

        public ModelNode ParentNode
        {
            get { return _parentNode; }
            set { _parentNode = value;
            DrawLine();}
        }

        public void UpdateResultsBase()
        {
            _nodeDef.Result.UpdateConsumptions();
        }

        public void UpdateIndex()
        {
            _nodeDef.Result.NotifyIndicatiorBase();
        }

        private Viewbox _graf = new Viewbox();
        
        public static ObservableCollection<UIElement> _grid;



        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            _infoBox.Visibility = Visibility.Visible;
            ArrangeChildren();
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            _infoBox.Visibility = Visibility.Hidden;
            HideChildren();
            DrawLine();
        }


        public void AddChild(ModelNode m)
        {
            if (_nodeDef.Children == null)
                _nodeDef.Children = new List<ModelNode>();
            if (m.ParentNode != null)
                m.ParentNode.RemoveChild(m);
            
            ResourceModel.DistanceBetweenChildrenNodesWithNodes.Add(new Pair<ModelNode, decimal>(m,1));
            m.ParentNode = this;
            _nodeDef.Children.Add(m);
            ArrangeChildren();
        }

        private void RemoveChild(ModelNode modelNode)
        {
            _nodeDef.Children.Remove(modelNode);
            var removepair = ResourceModel.DistanceBetweenChildrenNodesWithNodes.FirstOrDefault(t => t.Key == modelNode);
            ResourceModel.DistanceBetweenChildrenNodesWithNodes.Remove(removepair);
            modelNode.ParentNode = null;
        }

        private void ArrangeChildren()
        {
            
            if (_nodeDef !=null && _nodeDef.Children != null)
                foreach (var modelNode in _nodeDef.Children)
                {
                    modelNode.Visibility = Visibility.Visible;
                    modelNode.ParentNode = this;
                }
        }

        private void HideChildren()
        {
            if (_line != null)
            {
                _grid.Remove(_line);
                _line = null;
            }
            if (_nodeDef.Children != null)
                foreach (var modelNode in _nodeDef.Children)
                {
                    modelNode.Visibility = Visibility.Hidden;
                    modelNode.HideChildren();
                }
        }

        public void DrawLine()
        {
            foreach (var relatedProduct in ResourceModel.RelatedProducts)
            {
                if (relatedProduct.IsChecked??false)
                    relatedProduct.PrintLines();
            }
            if (_grid != null && _line != null)
                _grid.Remove(_line);
                

            if (ParentNode == null)
                return;
            var g = new StreamGeometry();
            var context = g.Open();
            context.BeginFigure(
                new Point(ParentNode.XGrid + ParentNode.Width/2,
                          ParentNode.YGrid + ParentNode.Height), true,
                true);
            context.LineTo(
                new Point(XGrid + Width/2,
                          YGrid), true, true);
            context.Close();
            Path path = new Path();
            path.Data = g;
            Brush v = new LinearGradientBrush(Colors.DarkCyan, Colors.DeepSkyBlue, 90);
            path.Stroke = v; //new SolidColorBrush(Colors.BlueViolet);
            path.StrokeThickness = 1.9;
            path.MouseRightButtonUp += path_MouseRightButtonUp;
            if (_grid != null)
            {
               _line = path;
               _grid.Add(path);
            }
        }
        void path_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ParentNode.RemoveChild(this);
        }

        public double YGrid
        {
            get
            {
                double x = 0;
                DispatcherHelper.CheckBeginInvokeOnUI(() => { x = Margin.Top + RenderTransform.Value.OffsetY; });
                return x;
            }
            set
            {
                Margin = new Thickness(Margin.Left, value - RenderTransform.Value.OffsetY, 0, 0);

            }
        }

        public double XGrid
        {
            get
            {
                double y = 0;
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        y = Margin.Left + RenderTransform.Value.OffsetX;
                    });
                return y;
            }
            set
            {
                Margin = new Thickness(value - RenderTransform.Value.OffsetX,Margin.Top,0,0);
            }
        }


        private void NodeLoaded(object sender, RoutedEventArgs e)
        {
            ArrangeChildren();
            
            //if (grid != null) grid.Children.Add(_infoBox);
        }

        private void MouseDragElementBehavior_DragBegun(object sender, MouseEventArgs e)
        {
            if (SpecialNode)
                return;
            SendTo(false);
            _previousPoint = new Thickness(XGrid,
                                          YGrid, 0, 0);
            if (_line != null)
            {
                _grid.Remove(_line);
            }
        }

        private void MouseDragElementBehavior_DragFinished(object sender, MouseEventArgs e)
        {
            if(SpecialNode)
                return;
            
            DragDrop.DoDragDrop(this, new DataObject("ModelNode", this), DragDropEffects.All);
            DrawLine();
            ArrangeChildren();
            AllowDrop = true;
            SendPathsTo(false);
            SendTo(true);
        }

        /// <summary>
        /// Helper method used by the BringToFront and SendToBack methods.
        /// </summary>
        /// <param name="element">
        /// The element to bring to the front or send to the back.
        /// </param>
        /// <param name="bringToFront">
        /// Pass true if calling from BringToFront, else false.
        /// </param>
        private void SendPathsTo(bool bringToFront)
        {
            
            int paths = 0;
            int inc = bringToFront ? 1 : -1;
            int elementNewZIndex = -1;
            if (bringToFront)
            {
                foreach (UIElement elem in _grid)
                {
                    if (elem.Visibility != Visibility.Collapsed)
                    {
                        elementNewZIndex = Math.Max(Panel.GetZIndex(elem), elementNewZIndex);
                        if ((elem as Path) != null)
                            paths++;
                    }
                }
                elementNewZIndex++;
            }
            else
            {
                foreach (UIElement elem in _grid)
                {
                    if (elem.Visibility != Visibility.Collapsed)
                    {
                        elementNewZIndex = Math.Min(Panel.GetZIndex(elem), elementNewZIndex);
                        if ((elem as Path) != null)
                            paths++;
                    }
                }
                elementNewZIndex--;
            }

            int elementCurrentZIndex = Panel.GetZIndex(this);

            // Update the Z-NodeIndicator of every UIElement in the Canvas.
            foreach (UIElement childElement in _grid)
            {
                if ((childElement as Path) != null)
                {
                    Panel.SetZIndex(childElement, elementNewZIndex);
                    elementNewZIndex += inc;
                }
                else
                {
                    int zIndex = Panel.GetZIndex(childElement);
                    Panel.SetZIndex(childElement, zIndex - inc);
                }
            }

        }

        /// <summary>
        /// Helper method used by the BringToFront and SendToBack methods.
        /// </summary>
        /// <param name="element">
        /// The element to bring to the front or send to the back.
        /// </param>
        /// <param name="bringToFront">
        /// Pass true if calling from BringToFront, else false.
        /// </param>
        private void SendTo(bool bringToFront)
        {
            
            int inc = bringToFront ? 1 : -1;

            #region Calculate Z-Indici And Offset

            int elementNewZIndex = -1;
            if (bringToFront)
            {
                foreach (UIElement elem in _grid)
                {
                    if (elem.Visibility != Visibility.Collapsed)
                    {
                        elementNewZIndex = Math.Max(Panel.GetZIndex(elem), elementNewZIndex) + 1;
                    }
                }
            }
            else
            {
                foreach (UIElement elem in _grid)
                {
                    if (elem.Visibility != Visibility.Collapsed)
                    {
                        elementNewZIndex = Math.Min(Panel.GetZIndex(elem), elementNewZIndex) - 1;
                    }
                }
            }
            elementNewZIndex += inc;

            #endregion // Calculate Z-Indici And Offset

            #region Update Z-Indexes

            // Update the Z-NodeIndicator of every UIElement in the Canvas.
            foreach (UIElement childElement in _grid)
            {
                if (childElement == this)
                {
                    Panel.SetZIndex(this, elementNewZIndex);
                }
                else
                {
                    int zIndex = Panel.GetZIndex(childElement);

                    Panel.SetZIndex(childElement, zIndex - inc);

                }
            }

            #endregion // Update Z-Indici
        }

        private void MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            if (SpecialNode)
                return;

            if (!ResourceModel.ShowResults)
            {
                _nodeDef.SetPrev();
                _nodeDefView.DataContext = _nodeDef;

                var win = new Window
                    {
                        Width = 650,
                        Height = 670,
                        Topmost = true,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        WindowStyle = WindowStyle.ToolWindow,
                        Owner = Window.GetWindow(this)
                        
                    };
                win.Content = _nodeDefView;
                win.Show();
                win.Closed += win_Closed;
            }
        }

        void win_Closed(object sender, EventArgs e)
        {           
            if (!ResourceView.DialogResult)
            {
                ResourceModel.RestorePrev();
            }
        }


        private void DropMethod(object sender, DragEventArgs e)
        {
            
            if (e.Data.GetDataPresent("ModelNode"))
            {
                var child = e.Data.GetData("ModelNode") as ModelNode;
                if (child != null && child != this)
                {
                    child.ResetPosition();
                    AddChild(child);
                }
                e.Handled = true;
            }
            else if (e.Data.GetDataPresent("Product"))
            {
                Product contact = e.Data.GetData("Product") as Product;

                if (contact != null)
                {
                    if (contact.Dropped)
                        return;
                    contact.Dropped = true;
                    contact.AddNextMachine(this, DeclaredJobs[0]);
                    ResourceModel.RelatedProducts.Add(contact);
                    OnPropertyChanged("ProductLabels");
                }
                
                SendPathsTo(false);
                e.Handled = true;
            }
        }
        
        private void ResetPosition()
        {
            Margin = _previousPoint;
            MovedPosition.X = MovedPosition.X - (MovedPosition.X - _previousPoint.Left)/2;
            MovedPosition.Y = _previousPoint.Top;
            //SetValue(MarginProperty, new Thickness(_previousPoint.X,_previousPoint.Y,0,0));
        }
       
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Drag_Enter(object sender, DragEventArgs e)
        {
            if (sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        #region Export Code

        public virtual string AutomodInit(ISet<ModelNode> modelNodes, ISet<ModelNode> Initiated  )
        {
            var code = "";
            var pre = "\r\n    ";

            code += pre + "set Q_" + Text + " capacity = " + ResourceModel.Capacity;
            code += pre + "set R_" + Text + " capacity = " + ResourceModel.Capacity;
            if(ResourceModel.MTBF != null && ResourceModel.MTTR != null)
                code += pre + "create 1 load of load type L_Dummy to P_" + Text + "BreakDown";
            Initiated.Add(this);
            if (ParentNode != null && !modelNodes.Contains(ParentNode)&& !Initiated.Contains(ParentNode))
            {
                code += ParentNode.AutomodInit(modelNodes, Initiated);
            }

            return code;
        }
        public virtual string AutomodProductEnter(int deeph, ModelNode prev,string productName)
        {
            var pre = "\r\n";
            for (int i = 0; i < deeph; ++i)
                pre += "    ";
            var code = "";
            var par = prev;
            bool enter = true;
            while (par != null)
            {
                if (par == this)
                    enter = false;
                par = par.ParentNode;
            }
            if (enter || (prev != null && prev.ParentNode == null) || (prev == null) || (prev.ParentNode != null && ParentNode == prev.ParentNode))
            {
                if (ParentNode != null)
                    code += ParentNode.AutomodProductEnter(--deeph, prev,null);
                if (!(this is Buffer))
                    code +=  pre + "move into Q_" + Text+
                             pre + "get R_" + Text;
                else 
                    code += pre + "move into Q_" + Text;
                
                if(productName != null)
                    code += pre + "order 1 load from OL_" + productName + "Init";

                code += pre + "set A_" + Text + "NodeEnter = rc";
                if(ResourceModel.IsTransport)
                    code += pre + "    wait for " +
                               (ResourceModel.DistansToNode /
                                 ResourceModel.Speed).ToString(CultureInfo.InvariantCulture) + " sec";
            }
            return code;
        }

        public virtual string AutomodProductUse(int deeph, Job job)
        {
            var pre = "\r\n    ";
            for (int i = 0; i < deeph; ++i)
                pre += "    ";
            var code = "";

            foreach (var subjob in job.Subjobs)
            {
                if (subjob != null)
                {
                    code += pre + "wait for ";
                    code += subjob.AutomodCode + "\r\n";

                }
                if (job.Quality < 100 && job.Quality > 0)
                {
                    code += pre + "if u 50, 50 > " + job.Quality.ToString(CultureInfo.InvariantCulture) +
                            pre + "begin" +
                            pre + "    free R_" + Text +
                            pre + "    set A_" + Text + "NodeTotal = rc - A_" + Text + "NodeEnter";
                    if(ParentNode != null)
                            code += pre + ParentNode.AutomodEmergencyLeave(pre+"    ");
            
                    code+=  pre + "    send to P_ExportFunction"+
                            pre + "end";
                }
            }
            return code;
        }

        public virtual string AutomodEmergencyLeave(string pre)
        {
            var code="";
            if (ParentNode != null)
            {
                code += ParentNode.AutomodEmergencyLeave(pre);
            }

            if (!(this is Buffer))
                code += pre + "free R_" + Text +
                        pre + "set A_" + Text + "NodeTotal = rc - A_" + Text + "NodeEnter";
            else
                code += pre + "set A_" + Text + "NodeTotal = rc - A_" + Text + "NodeEnter";

            return code;
        }

        public virtual string AutomodProductLeave(int deeph,  ModelNode next)
        {
            var pre = "\r\n";
            for (int i = 0; i < deeph; ++i)
                pre += "    ";
            var code = "";
            
            var par = next;
            bool enter = true;

            while (par != null)
            {
                if (par == this)
                    enter = false;
                par = par.ParentNode;
            }
            


            if (enter || (next != null && next.ParentNode == null) || (next == null) || (next.ParentNode != null && ParentNode == next.ParentNode))
            {
                if(!(this is Buffer))
                    code += pre + "free R_" + Text +
                            pre + "set A_" + Text + "NodeTotal = rc - A_" + Text + "NodeEnter";
                else
                    code += pre + "set A_" + Text + "NodeTotal = rc - A_" + Text + "NodeEnter";

                if (ParentNode != null && ParentNode.ResourceModel.IsTransport && ParentNode.ResourceModel.Speed>0)
                {
                    code += pre + "    wait for " +
                            (ParentNode.ResourceModel.DistanceBetweenChildrenNodesWithNodes.FirstOrDefault(t => t.Key == this).Value /
                                ParentNode.ResourceModel.Speed).ToString(CultureInfo.InvariantCulture) + " sec";
                }

                if(ParentNode!= null)
                    code += ParentNode.AutomodProductLeave(--deeph, next);
            }

            //if (ParentNode != null && ((next != null && next.ParentNode == null) || (next == null) || (next.ParentNode != null && ParentNode == next.ParentNode)))
                
            return code;
        }


        public virtual string AutoModBreakdownProcess()
        {
            return ResourceModel.AutoModBreakdownCode;
        }

        public int GetDeeph(int i)
        {
            if (ParentNode == null)
                return ++i;
            return ParentNode.GetDeeph(++i);
        }
        
        #endregion
    }
}


