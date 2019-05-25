using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EcoProIT.Main.ViewModel;
using EcoProIT.UserControles;
using EcoProIT.UserControles.Models;
using EcoProIT.UserControles.ViewModel;
using Microsoft.Win32;

namespace EcoProIT.Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            


            InitializeComponent();
            
      
            _viewModel = (MainViewModel)DataContext;
            ModelNode._grid = _viewModel.GridUIElement;
            Product.Grid = _viewModel.GridUIElement;
            if (AppDomain.CurrentDomain.SetupInformation.ActivationArguments !=null && AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData !=null&& AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData.Any())
            {
                try
                {
                    Uri u = new Uri(AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData[0]);
                    if (u.IsFile)
                    {

                        Stream s = File.Open(u.LocalPath, FileMode.Open);

                        _viewModel.ClearNodes();
                        
                        
                        
                        OpenNodes(s);
                    }
                }
                catch { }
            }

        }

        


        private void _newProduct_Click_1(object sender, RoutedEventArgs e)
        {
 
           var p = new Product();
            _viewModel.ProductList.Add(p);
            p.Editmode = true;
            _viewModel.SelectedProduct = p;
            UpdateLayout();
            }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }


        private void Retrive_Results(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b == null)
                return;
            if (b.Content.ToString() == "Hide NodeResults")
            {
                b.Content = "Retrive NodeResults";
                _viewModel.SelectedNode = null;
                foreach (var modelNode in _viewModel.GridNodes)
                {                   
                   modelNode.ResourceModel.ShowResults = false;
                }
                foreach (var product in _viewModel.ProductList)
                {
                    product.ShowResults = false;
                }
                _viewModel.ShowResults = false;
            }
            else
            {
                IResults.UpdateSourceData();
                b.Content = "Hide NodeResults";
                foreach (var modelNode in _viewModel.GridNodes)
                {
                    modelNode.UpdateResultsBase();
                }
                foreach (var modelNode in _viewModel.GridNodes)
                {
                    modelNode.UpdateIndex();
                    modelNode.ResourceModel.ShowResults = true;
                }
                foreach (var product in _viewModel.ProductList)
                {
                    product.Result.UpdateConsumptions();
                    product.ShowResults = true;
                }
                foreach (var product in _viewModel.ProductList)
                {
                    product.ShowResults = true;
                }
                _viewModel.Update();
                _viewModel.SelectedResultConsumable = "Processed";
                _viewModel.ShowResults = true;
            }
        }

        #region Implementation of IComponentConnector

        /// <summary>
        /// Attaches events and names to compiled content. 
        /// </summary>
        /// <param name="connectionId">An identifier token to distinguish calls.</param><param name="target">The target to connect events and names to.</param>
        public void Connect(int connectionId, object target)
        {
            throw new NotImplementedException();
        }

        #endregion

        private async void Click_Save(object sender, RoutedEventArgs e)
        {
            var save = new SaveFileDialog();
            save.DefaultExt = ".ecos";
            save.FileName = "Model";
            save.AddExtension = true;
            save.Filter = "Model file (.ecos)|*.ecos";
            if (save.ShowDialog()??false)
            {
                bool res = await Save.SaveNodes(_viewModel.GridNodes,save,_viewModel.ProductList);
                MessageBox.Show(res ? "Save Success" : "Error Saving");
            }
        }

        private void Click_Load(object sender, RoutedEventArgs e)
        {
            
            var load = new OpenFileDialog();
            load.AddExtension = true;
            load.DefaultExt = ".ecos";
            load.Filter = "Model file (.ecos)|*.ecos";
            if (load.ShowDialog() ?? false)
            {
                _viewModel.ProductList.Clear();
                _viewModel.ClearNodes();
                Stream s = load.OpenFile();
                MessageBox.Show(OpenNodes(s)?"Load Success" : "Error Loading");
            }
        }

        private bool OpenNodes(Stream s)
        {
            var products = new List<Product>();
            var nodes = new List<ModelNode>();
            if (Save.LoadModel(s, nodes, products))
            {
                foreach (var n in nodes)
                    _viewModel.AddNode(n);
                foreach (var product in products)
                {
                    _viewModel.ProductList.Add(product);
                    product.PrintLines();
                }
                foreach (var modelNode in nodes)
                {
                    modelNode.UpdateProductLabels();
                }
            }
        

            return nodes != null;
        }

        private void LayoutRoot2_OnDrop(object sender, DragEventArgs e)
        {
            if (!e.Handled && e.Data.GetDataPresent("ModelNode"))
            {
                var n = e.Data.GetData("ModelNode") as ModelNode;

                n.XGrid = e.GetPosition(n).X + n.XGrid-n.Width/2;
                n.YGrid = e.GetPosition(n).Y + n.YGrid-n.Height/2;
            }
        }

        private void Image_Drop_1(object sender, DragEventArgs e)
        {
            if (!e.Handled && e.Data.GetDataPresent("ModelNode"))
            {
                var n = e.Data.GetData("ModelNode") as ModelNode;
                _viewModel.GridUIElement.Remove(n);
                foreach (var product in n.ResourceModel.RelatedProducts)
                {
                    foreach (var source in product.Nodes.Where(t => t.Key == n))
                    {
                        product.Nodes.Remove(source);
                    }
                    
                }
                
            }
            else if (!e.Handled && e.Data.GetDataPresent("Product"))
            {
                var n = e.Data.GetData("Product") as Product;
                if (n != null)
                {
                    var delete = n.Nodes.ToArray();
                    foreach (var node in delete)
                    {
                        n.Nodes.Remove(node);
                    }
                    _viewModel.ProductList.Remove(n);
                }
            }
        }
    }
}