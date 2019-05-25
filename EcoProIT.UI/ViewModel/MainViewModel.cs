using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using EcoProIT.DataLayer;
using EcoProIT.UI.SimulationModel;
using EcoProIT.UserControles;
using EcoProIT.UserControles.Models;
using EcoProIT.UserControles.ViewModel;
using GalaSoft.MvvmLight;
using EcoProIT.UI.Model;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using HelpClasses;
using Microsoft.Win32;
using Buffer = EcoProIT.UserControles.Buffer;
using Statistics = EcoProIT.UI.Views.Statistics;

namespace EcoProIT.UI.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private ObservableCollection<Product> _products = new ObservableCollection<Product>();
        private Product _selectedProduct;
        private ModelNode _selectedNode;
        private static modeloutputContext db;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MainViewModel()
        {
            MyButtonClickAction = new RelayCommand(ExportFileAutomod);
            SelectedNode = new ModelNode();
            ResourceDefinitionModel.Nodes = GridNodes;
            _newNodes.Add("New Machine", new Machine() {SpecialNode = true, Text = "New Machine"});
            _newNodes.Add("New Buffer", new Buffer() {SpecialNode = true, Text = "New Buffer"});
            _newNodes.Add("New Facility", new Facility() {SpecialNode = true, Text = "New Facility"});
            _newNodes.Add("New Transport", new Transport() {SpecialNode = true, Text = "New Transport"});
            _newNodes.Add("New General Node", new ModelNode() {SpecialNode = true, Text = "New General Node"});

            foreach (var node in _newNodes.Values)
            {
                node.MouseDoubleClick += NodeOnMouseDoubleClick;
            }
            AddNode(new ModelNode() {Text = "Production", Margin = new Thickness(100, 81, 0, 0)});
            AddNode(new ModelNode() {Text = "Distribution", Margin = new Thickness(300, 81, 0, 0)});
            AddNode(new ModelNode() {Text = "Usage", Margin = new Thickness(500, 81, 0, 0)});
            AddNode(new ModelNode() {Text = "Recycle", Margin = new Thickness(700, 81, 0, 0)});
            ProductList.CollectionChanged += ProductList_CollectionChanged;
            PropertyChanged += MainViewModel_PropertyChanged;
            simHandler.PropertyChanged += simHandler_PropertyChanged;
            if (IsInDesignMode)
                return;
            
            db = DatabaseConnection.GetModelContext();
            if(db == null)
                return;
        }

        void simHandler_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsRunning")
            {

                RaisePropertyChanged("ShowToResults");
                RaisePropertyChanged("ShowToDesigner");
                RaisePropertyChanged("RunSimulations");
                
            }
            if (e.PropertyName == "HasResults")
            {
                RaisePropertyChanged("ShowToResults");
                RaisePropertyChanged("ShowToDesigner");
                RaisePropertyChanged("HasResults");
            }
            
        }



        public bool HasResult
        {
            get { return !SimHandler.IsRunning && simHandler.HasResults; }
        }

        private void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "GridNodes")
                ResourceDefinitionModel.Nodes = GridNodes;
        }

        private void ProductList_CollectionChanged(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
                return;
            foreach (Product product in e.NewItems)
            {
                product.MouseDoubleClick += product_MouseDoubleClick;
            }
        }

        private void product_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Product)) return;
            var product = sender as Product;
            if (product.ShowResults)
            {
                var win = new Window
                {
                    Width = 650,
                    Height = 670,
                    Topmost = true,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    WindowStyle = WindowStyle.ToolWindow,
                    Owner = Application.Current.MainWindow
                };
                win.Content = new Statistics()
                {
                    DataContext =
                        new StatisticProductModel()
                        {
                            Results = product,
                            Modelnodes = GridNodes,
                            SelectedIndex = "Processed per hour",
                            SelectedType = "Compare Sources"
                        }
                };
                win.Show();
            }
        }

        #region AutomodExport

        public void ExportFileAutomod()
        {
            var automodInitCode = "begin model initialization function"
                                  + "\r\n    //Define all A_* as integer attributes"
                                  + "\r\n    //Define all Vs as string variables"
                                  + "\r\n    set Q_ExportBlocking capacity to 1"
                                  +
                                  "\r\n    print \"OpenSource command (0=OK): \" DBOpenSourceUDL(\"connectdb.udl\",\"connectdb\")to message\r\n    print \"OpenSession command (0=OK): \" DBOpenSession(\"connectdb\",\"first\")to message"
                                  + "\r\n    print \"Delete from [ProductResult]\" to Vs_sqlCommand"
                                  +
                                  "\r\n    print \"Executing command (0=OK): \" DBExecuteCmd(\"first\", Vs_sqlCommand) to message"
                                  + "\r\n    print \"Delete from [MachineStates]\" to Vs_sqlCommand"
                                  +
                                  "\r\n    print \"Executing command (0=OK): \" DBExecuteCmd(\"first\", Vs_sqlCommand) to message"
                                  + "\r\n    create 1 loads of load type L_Dummy to P_TerminateModel";
            var automodProductCode = "";
            var automodProductInitProcedures = "";
            var automodProductExportData = "\r\nbegin P_ExportFunction arriving procedure" +
                                           "\r\n    move into Q_ExportBlocking";

            var includedModelNodes = new HashSet<ModelNode>();
            foreach (var product in ProductList)
            {

                if (product.IsChecked ?? false)
                {
                    automodInitCode += "\r\n    create 1 loads of load type L_" + product.ProductName + " to P_" +
                                       product.ProductName + "Init";
                    automodProductInitProcedures += "\r\nbegin P_" + product.ProductName + "Init arriving procedure\r\n" +
                                                    "    wait for 1 sec\r\n" +
                                                    "    while(1=1) begin\r\n        create 1 loads of load type L_" +
                                                    product.ProductName +
                                                    " to P_" + product.ProductName + "\r\n" +
                                                    "        wait to be ordered on OL_" + product.ProductName + "Init" +
                                                    "\r\n    end\r\nend\r\n";
                    automodProductCode += "\r\n" + product.AutomodCodeBody();
                    foreach (var node in product.Nodes)
                    {
                        includedModelNodes.Add(node.Key);

                    }
                }

            }
            automodInitCode += "\r\n    //Define V_ExportArray as integer array variable with dimension 1 x " +
                               includedModelNodes.Count;
            var exportNodeStates = "";
            var exported = new HashSet<ModelNode>();
            var initiated = new HashSet<ModelNode>();
            foreach (var includedModelNode in includedModelNodes)
            {
                exportNodeStates +=
                    "\r\n    print  \"INSERT INTO [MachineStates] ([Machine], [State] ,[Total]) VALUES('" +
                    includedModelNode.ResourceModel.ProcessName + "',"
                    + "'Down', \" (rc * R_" + includedModelNode.ResourceModel.ProcessName +
                    " average down) \");\" to Vs_sqlCommand"
                    + "\r\n    print \"(0=OK):\" DBExecuteCmd(\"first\", Vs_sqlCommand) to message"
                    + "\r\n    print  \"INSERT INTO [MachineStates] ([Machine], [State] ,[Total]) VALUES('" +
                    includedModelNode.ResourceModel.ProcessName + "',"
                    + "'IdleTime', \" (rc * (1-R_" + includedModelNode.ResourceModel.ProcessName +
                    " average)) \");\" to Vs_sqlCommand"
                    + "\r\n    print \"(0=OK):\" DBExecuteCmd(\"first\", Vs_sqlCommand) to message";
                if (!initiated.Contains(includedModelNode))
                    automodInitCode += includedModelNode.AutomodInit(includedModelNodes, initiated);
                automodProductCode += "\r\n" + includedModelNode.AutoModBreakdownProcess();
                //automodProductExportData += "\r\n    set V_ExportArray(" + nodenumber++ + ") to A_" +
                //                            includedModelNode.ResourceModel.ResultId + "NodeTotal";

                automodProductExportData += AutomodProductExportData(exported, includedModelNode);
            }

            //automodProductExportData += "\r\n    call XLSetArea(\"[Output.xlsx]Sheet1\",\"A1:A" + (nodenumber-1) + "\",\"V_ExportArray\")" + 
            automodProductExportData += "\r\nend";
            automodInitCode += "\r\n    return 0\r\nend"
                               + "\r\n "
                               + "\r\nbegin P_TerminateModel arriving procedure"
                               + "\r\n    wait for " + SimulationTimeConverted/3600000 + " hr"
                               + "\r\n    wait for V_SnapLength sec"
                               +
                               "\r\n    print  \"INSERT INTO [MachineStates] ([Machine], [State] ,[Total]) VALUES('BaseModel',"
                               + "'BaseTime', \" rc \");\" to Vs_sqlCommand"
                               + "\r\n    print \"(0=OK):\" DBExecuteCmd(\"first\", Vs_sqlCommand) to message"
                               + exportNodeStates
                               + "\r\n    call DBCloseSource(\"\")"
                               + "\r\n    terminate"
                               + "\r\nend"
                               + "\r\n"
                               + "\r\nbegin model snap function"
                               + "\r\n    if V_SnapLength = 0 begin"
                               + "\r\n        set V_SnapLength = rc"
                               + "\r\n        print \"Delete from [ProductResult]\" to Vs_sqlCommand"
                               +
                               "\r\n        print \"Executing command (0=OK): \" DBExecuteCmd(\"first\", Vs_sqlCommand) to message"
                               + "\r\n    end"
                               + "\r\n    return 1"
                               + "\r\nend";

            var save = new SaveFileDialog();
            save.AddExtension = true;
            save.DefaultExt = ".m";
            save.FileName = "logic";
            save.Filter = "logic file (.m)|*.m";
            var result = save.ShowDialog();
            if (result == true)
            {
                var file = save.OpenFile();

                Encoding winLatinCodePage = Encoding.GetEncoding(1252);
                Byte[] bytes = Encoding.Convert(Encoding.ASCII, winLatinCodePage,
                    Encoding.UTF8.GetBytes(automodInitCode));
                Byte[] bytes2 = Encoding.Convert(Encoding.ASCII, winLatinCodePage,
                    Encoding.UTF8.GetBytes(automodProductInitProcedures));
                Byte[] bytes3 = Encoding.Convert(Encoding.ASCII, winLatinCodePage,
                    Encoding.UTF8.GetBytes(automodProductCode));
                Byte[] bytes4 = Encoding.Convert(Encoding.ASCII, winLatinCodePage,
                    Encoding.UTF8.GetBytes(automodProductExportData));
                file.Write(bytes, 0, bytes.Length);
                file.Write(bytes2, 0, bytes2.Length);
                file.Write(bytes3, 0, bytes3.Length);
                file.Write(bytes4, 0, bytes4.Length);
                file.Close();

                var encoding = new System.Text.UnicodeEncoding();
                string s = "[oledb]" + Environment.NewLine + "; Everything after this line is an OLE DB initstring" +
                           Environment.NewLine +
                           "Provider=Microsoft.SQLSERVER.CE.OLEDB.4.0;Data Source=";
                try
                {


                    s += ApplicationDeployment.CurrentDeployment.DataDirectory +
                         "\\Resources\\modeloutput.sdf;" + Environment.NewLine;
                }
                catch
                {
                    s += Environment.CurrentDirectory +
                         "\\Resources\\modeloutput.sdf;" + Environment.NewLine;
                }
                var fileinfo = new FileInfo(save.FileName);
                string folder = fileinfo.Directory + "\\connectdb.udl";
                var bytes5 = new List<byte>(StrToByteArray(s));
                bytes5.Insert(0, 254);
                bytes5.Insert(0, 255);
                var fi = new FileInfo(folder);
                if (fi.Exists)
                    fi.Delete();
                file = new FileStream(folder, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                file.Write(bytes5.ToArray(), 0, bytes5.Count);
                file.Close();
            }
        }

        private static string AutomodProductExportData(HashSet<ModelNode> exported, ModelNode includedModelNode)
        {
            string automodProductExportData = "";

            if (exported.Add(includedModelNode))
                automodProductExportData += "\r\n    if A_" + includedModelNode.ResourceModel.ProcessName +
                                            "NodeEnter > 0"
                                            + "\r\n    begin"
                                            +
                                            "\r\n        print  \"INSERT INTO [ProductResult] ([ProductType], [ModelNode] ,[Productid],[Start],[Total]) VALUES('\" As_ProductType \"','"
                                            + includedModelNode.ResourceModel.ProcessName + "', \" A_ProductID \",\" A_" +
                                            includedModelNode.ResourceModel.ProcessName + "NodeEnter \", \" A_"
                                            + includedModelNode.ResourceModel.ProcessName +
                                            "NodeTotal \");\" to Vs_sqlCommand"
                                            + "\r\n        call DBExecuteCmd(\"first\", Vs_sqlCommand)"
                                            + "\r\n    end";
            if (includedModelNode.ParentNode != null)
                automodProductExportData += AutomodProductExportData(exported, includedModelNode.ParentNode);
            return automodProductExportData;
        }

        public static byte[] StrToByteArray(string str)
        {
            var encoding = Encoding.Unicode; //Encoding.GetEncoding(1200);
            return encoding.GetBytes(str);
        }

        #endregion

        public ObservableCollection<Product> ProductList
        {
            get { return _products; }
        }

        public Product SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                _selectedProduct = value;
                RaisePropertyChanged("SelectedProduct");
                SelectedResult = value;
            }
        }

        #region CommandProperties

        public RelayCommand MyButtonClickAction { get; private set; }

        public ModelNode SelectedNode
        {
            get { return _selectedNode ?? new ModelNode(); }
            set
            {
                _selectedNode = value;
                if (value != null)
                {
                    RaisePropertyChanged("SelectedNode");
                    SelectedResult = value.ResourceModel;
                }
            }
        }

        #endregion

        #region ModelNodes

        private ModelNode _newNode;
        private Dictionary<string, ModelNode> _newNodes = new Dictionary<string, ModelNode>();
        private ObservableCollection<UIElement> _gridUIElement = new ObservableCollection<UIElement>();
        private string _selectedNewNode;
        private IHasResults _selectedResult;
        private ulong _simulationTime = 40;
        private string _selectedResultConsumable;

        private string _welcomeTitle;
       
        private int GetLastPrimeryNodePosition()
        {
            var f = (from i in GridNodes where i.YGrid > 100 orderby i.XGrid descending select i.XGrid).FirstOrDefault();
            return (int) ((int) f == 0 ? 10 : f);
        }

        public IEnumerable<ModelNode> GridNodes
        {
            get
            {
                return
                    (from i in _gridUIElement where i is ModelNode select i).Select(node => node as ModelNode).ToList();
            }
        }


        //Borde goras pa annat satt for std
        public Dictionary<string, Statistic> TotalNodeConsumebles
        {
            get
            {
                var result = (from i in GridNodes.SelectMany(t => t.ResourceModel.Result.Consumables)
                    group i by i.Key.Name
                    into g
                    select g).ToDictionary(i => i.Key, i => new Statistic(i.Sum(t => t.Value.Mean), 0));
                
                //foreach (var index in IndexCalculator.Indexes)
                //{
                //    result.Add(index.IndexName,
                //        index.Calculation(GridNodes.SelectMany(t => t.ResourceModel.Result.Consumables)));
                //}

                return result;
            }
        }

        public ObservableCollection<UIElement> GridUIElement
        {
            get { return _gridUIElement; }
        }

        private void NodeOnMouseDoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ModelNode newNode;
            if (sender is Machine)
                newNode = new Machine()
                {



                };
            else if (sender is Buffer)
                newNode = new Buffer()
                {

                };
            else if (sender is Facility)
                newNode = new Facility()
                {

                };
            else if (sender is Transport)
                newNode = new Transport()
                {

                };
            else
                newNode = new ModelNode()
                {

                };

            AddNode(newNode);
        }

        public void RemoveNode(ModelNode node)
        {
            _gridUIElement.Remove(node);
            RaisePropertyChanged("GridNodes");
        }

        public void ClearNodes()
        {
            _gridUIElement.Clear();
            RaisePropertyChanged("GridNodes");
        }

        public void AddNode(ModelNode newNode)
        {
            if (newNode == null)
                return;
            if (newNode.XGrid == 0 && newNode.YGrid == 0)
                newNode.Margin = new Thickness(GetLastPrimeryNodePosition() + 200, 200, 0, 0);


            

            newNode.MouseLeftButtonUp += NewNodeOnMouseLeftButtonUp;
            newNode.MouseDoubleClick += newNode_MouseDoubleClick;
            _gridUIElement.Add(newNode);
            RaisePropertyChanged("GridNodes");
            //newNode.MouseLeftButtonUp += NewNodeOnMouseLeftButtonUp;
        }

        private void newNode_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is ModelNode)) return;
            var node = sender as ModelNode;
            if (node.ResourceModel.ShowResults)
            {
                var win = new Window
                {
                    Width = 650,
                    Height = 670,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    WindowStyle = WindowStyle.ToolWindow,
                    Owner = Window.GetWindow(node),
                    Content = new Statistics()
                    {
                        DataContext =
                            new StatisticModel()
                            {
                                Results = node.ResourceModel,
                                Modelnodes = GridNodes,
                                SelectedIndex = "Processed per hour",
                                SelectedType = "Compare nodes"
                            }
                    }
                };
                win.Show();
            }
        }

        private void NewNodeOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (sender is ModelNode)
                SelectedNode = sender as ModelNode;
        }

        public ModelNode NewNode
        {
            get { return _newNode; }
            set
            {
                _newNode = value;
                RaisePropertyChanged("NewNode");

            }
        }

        public string SelectedNewNode
        {
            get { return _selectedNewNode; }
            set
            {
                _selectedNewNode = value;
                if (_newNodes.ContainsKey(_selectedNewNode))
                    NewNode = _newNodes[_selectedNewNode];
                RaisePropertyChanged("SelectedNewNode");
            }
        }

        public IEnumerable<string> NewNodes
        {
            get { return _newNodes.Keys; }
        }

        public IHasResults SelectedResult
        {
            get { return _selectedResult; }
            set
            {
                _selectedResult = null;
                RaisePropertyChanged("SelectedResult");
                _selectedResult = value;
                RaisePropertyChanged("SelectedResult");
            }
        }

        public ListBoxItem SimulationTimeUnit { private get; set; }

        public ulong SimulationTimeConverted
        {
            get
            {
                switch (SimulationTimeUnit.Content.ToString().ToLowerInvariant())
                {
                    case "hours":
                        return SimulationTime * 3600000;
                    case "weeks":
                        return SimulationTime * 7 * 24 * 3600000;
                    case "years":
                        return SimulationTime * 365 * 24 * 3600000;
                }
                return SimulationTime;
            }
        }

        public ulong SimulationTime
        {
            get { return _simulationTime; }
            set
            {
                _simulationTime = value;
                RaisePropertyChanged("SimulationTime");
            }
        }

        public String SelectedResultConsumable
        {
            get { return _selectedResultConsumable; }
            set
            {
                _selectedResultConsumable = value;
                IResults.SelectedIndex = value;
                foreach (var modelNode in GridNodes)
                {
                    modelNode.ResourceModel.Result.UpdateBaseIndicator();
                }
                foreach (var modelNode in GridNodes)
                {
                    modelNode.ResourceModel.Result.NotifyIndicatiorBase();
                }
                RaisePropertyChanged("SelectedResultConsumable");
                RaisePropertyChanged("SelectedResultNodeConsumables");
            }

        }

        /// <summary>
        /// No STD
        /// </summary>
        public Dictionary<string, Statistic> SelectedResultNodeConsumables
        {
            get
            {
                var indexes = IndexCalculator.Indexes;
                if (indexes.Any(t => t.IndexName == SelectedResultConsumable))
                    return GridNodes.AsParallel().ToDictionary(t => t.ResourceModel.ProcessName,
                        t =>
                            indexes.First(p => p.IndexName == SelectedResultConsumable)
                                .StatisticCalculation(t.ResourceModel.Result.Consumables));

                return (from q in from i in GridNodes
                    select
                        new
                        {
                            name = i.ResourceModel.ProcessName,
                            statistic =
                                (from p in i.ResourceModel.Result.Consumables
                                    where p.Key.Name == SelectedResultConsumable
                                    select p.Value).FirstOrDefault()
                           
                        }
                        where q.statistic !=null && q.statistic.Mean > 0
                        orderby q.statistic.Mean descending
                        select q).ToDictionary(t => t.name, t=> t.statistic );
            }
        }

        public enum Modes { DesignMode, ResultMode}
        private Modes _currentMode = Modes.DesignMode;
        public Modes CurrentMode
        {
            get { return _currentMode; }
            set
            {
                _currentMode = value;
                RaisePropertyChanged();
                RaisePropertyChanged("ShowToResults");
                RaisePropertyChanged("ShowToDesigner");
            }
        }

        public bool ShowToResults
        {
            get { return HasResult && _currentMode != Modes.ResultMode; }
        }

        public bool ShowToDesigner
        {
            get { return !SimHandler.IsRunning && _currentMode != Modes.DesignMode; }
        }

        #endregion

        public void Update()
        {
            RaisePropertyChanged("TotalNodeConsumebles");
        }



        private SimulationHandler simHandler = new SimulationHandler();

        public RelayCommand RunSimulations  
        {
            get
            {
                if(simHandler.IsRunning)
                    return new RelayCommand(() => simHandler.StopSimulation());
                return new RelayCommand(()=>simHandler.run(ProductList, SimulationTimeConverted));
            }
        }

        public SimulationHandler SimHandler
        { get { return simHandler; } }



        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        [PreferredConstructor]
        public MainViewModel(IDataService dataService):this()
        {
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    _welcomeTitle = item.Title;
                });
            if (_welcomeTitle == null)
                _welcomeTitle = "EcoProIT Tool";

        }



    }

}
