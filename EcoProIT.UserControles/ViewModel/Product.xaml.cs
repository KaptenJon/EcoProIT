using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using EcoProIT.DataLayer;
using EcoProIT.UserControles.Models;
using EcoProIT.UserControles.View;
using GalaSoft.MvvmLight.Command;
using HelpClasses;

namespace EcoProIT.UserControles.ViewModel
{
	/// <summary>
	///     This class contains properties that a View can data bind to.
	///     <para>
	///         See http://www.galasoft.ch/mvvm
	///     </para>
	/// </summary>
	public partial class Product :  INotifyPropertyChanged, IHasResults
	{
		public static ObservableCollection<UIElement> Grid;
		private bool _checkChanged;
		private Color _color;
		private Brush _colorBrush;
		private bool _editMode;
		private List<UIElement> _line;

		private ObservableCollection<Pair<ModelNode, Job>> _nodes =
			new ObservableCollection<Pair<ModelNode, Job>>();

		private string _productName ="";
		private Point _startPoint;

		/// <summary>
		///     Initializes a new instance of the Product class.
		/// </summary>
		/// <param name="jobstodo"></param>
		public Product()
		{
			Color = Colors.Red;
			InitializeComponent();
			Result = new ProductResults(){ResultId = ProductName, Nodes = _nodes};
			MouseRightButtonUp += OnMouseRightButtonUp;
			PropertyChanged += Product_PropertyChanged;
			Nodes.CollectionChanged += Nodes_CollectionChanged;
		}

		public Product(IEnumerable<Pair<ModelNode, Job>> jobstodo) : this()
		{
			foreach (var jobs in jobstodo)
			{
				_nodes.Add(jobs);
			}
		}

		public class ProductResults : IResults
		{
		   
			public ObservableCollection<Pair<ModelNode, Job>> Nodes { get; set; }
			
			public ProductResults()
			{

			}
			
			public override void UpdateConsumptions()
			{
				var dic = new Dictionary<Consumable, List<decimal>>();
				foreach (var pair in SourceSimulationModelSet)
				{
					var runresult = UpdateConsumablesOneRun(pair.Key, pair.Value);
					foreach (var runresultcons in runresult)
					{
						if (dic.ContainsKey(runresultcons.Key))
						dic[runresultcons.Key].Add(runresultcons.Value); //(IResults.TotalTime/3600);
					else
						dic.Add(runresultcons.Key, new List<decimal>(new [] {runresultcons.Value}));
					}
				}
				var res = dic.ToDictionary(t=> t.Key, i=>new Statistic(i.Value.Mean(), i.Value.StandardDeviation()));
				Consumables = res;
				MeanConsumables = res.ToDictionary(t => t.Key, t => t.Value.Mean);
				
			}

			private Dictionary<Consumable, decimal> UpdateConsumablesOneRun(ParallelQuery<ProductResult> productResultsTableOrdered, ParallelQuery<MachineState> mashineStatesOrdered)
			{
				var dic = new Dictionary<Consumable, decimal>();
				var f = productResultsTableOrdered.Where(t => t.ProductType == ResultId);
				if (!f.Any())
					return dic;

				var allNodes = new HashSet<ModelNode>();

				//Direct
				foreach (var pair in Nodes)
				{
					var sumTime = f.Where(t => t.ModelNode == pair.Key.ResourceModel.ProcessName).Sum(t => t.Total, null);
					var count = f.Count(t => t.ModelNode == pair.Key.ResourceModel.ProcessName);
					foreach (var consumption in pair.Value.State.Consumptions)
					{
						if (!consumption.AllocationPerTime)
						{
							if (dic.ContainsKey(consumption.Consumable))
								dic[consumption.Consumable] += consumption.Amount;
							else
								dic.Add(consumption.Consumable, consumption.Amount);
						}
						else
						{
							if (sumTime != 0)
							{
								if (dic.ContainsKey(consumption.Consumable))
									dic[consumption.Consumable] += consumption.Amount*sumTime/count;
								else
									dic.Add(consumption.Consumable, consumption.Amount*sumTime/count);
							}
						}
					}
				}
				//Indirect
				foreach (var node in Nodes.Select(t => t.Key))
				{
					FindAll(allNodes, node);
				}
				foreach (var node in allNodes)
				{
					IndirectConsumptionFor(f, node, dic);
				}

				if (Nodes.Count > 0)
				{
					var count = f.Count(t => t.ModelNode == Nodes.Last().Key.ResourceModel.ProcessName);
					if (count == 0)
						count = 1;
					var res = dic.ToDictionary(i => i.Key, i => i.Value/((decimal)count));
					var totaltime = TotalTime;
					if (totaltime == 0)
						totaltime = 1;
					res.Add(new Consumable() {Name = "Processed per hour"}, count/(totaltime/3600000));
					res.Add(new Consumable() {Name = "Processed"}, count);
					return res;
				}
				return dic;

			}

			public decimal ConsumptionforInterval(ulong start, ulong end, string index)
			{
				var dic = Consumables.ToDictionary(consumable => consumable.Key, consumable => consumable.Value);
				var f = SourceTableOrdered.Where(t => t.ProductType == ResultId && t.Start > start && t.Start < end);
				if (!f.Any())
					return 0;

				var allNodes = new HashSet<ModelNode>();

				//Direct
				decimal result = 0;
				foreach (var pair in Nodes)
				{
					var sumTime = f.Where(t => t.ModelNode == pair.Key.ResourceModel.ProcessName).Sum(t => t.Total,null);
					var count = f.Count(t => t.ModelNode == pair.Key.ResourceModel.ProcessName);
					foreach (var consumption in pair.Value.State.Consumptions.Where(t => t.Consumable.Name == index))
					{
						if (consumption == null) continue;
						if (!consumption.AllocationPerTime)
						{
							result += consumption.Amount;
						}
						else
						{
							result = consumption.Amount*sumTime/count;
						}
					}
				}
				//Indirect
				foreach (var node in Nodes.Select(t => t.Key))
				{
					FindAll(allNodes, node);
				}
				foreach (var node in allNodes)
				{
					result += IndirectConsumptionFor(f, node, index);
				}
				return result;
			}
			private void FindAll(HashSet<ModelNode> allNodes, ModelNode node)
			{
				allNodes.Add(node);
				if(node.ParentNode != null)
					FindAll(allNodes,node.ParentNode);
			}

			private decimal IndirectConsumptionFor(ParallelQuery<DataLayer.ProductResult> f, ModelNode node,
											  string index)
			{
				var count = f.Count(t => t.ModelNode == node.ResourceModel.ProcessName);
				decimal downtime =
					(from i in SourceMachineStates where i.Machine == node.ResourceModel.ProcessName && i.State == "Down" select i.Total)
						.FirstOrDefault();
				decimal idletime =
					(from i in SourceMachineStates where i.Machine == node.ResourceModel.ProcessName && i.State == "Idle" select i.Total)
						.FirstOrDefault();
				decimal result = 0;
				foreach (State state in node.ResourceModel.States.Where(t => t.Name == "Down" || t.Name == "Idle"))
				{
					foreach (Consumption consumption in state.Consumptions.Where(t=>t.Consumable.Name==index))
					{
						var res = consumption.Amount / 3600000;
						if (!consumption.Static)
						{
							if (state.Name == "Down")
								res += res * downtime;
							else if (state.Name == "Idle")
								res += res * idletime;
						}
						else
							res = res * count;

						result += res;
					}
				}
				return result;
			}

			private static void IndirectConsumptionFor(ParallelQuery<DataLayer.ProductResult> f, ModelNode node,
											   Dictionary<Consumable, decimal> dic)
			{
				var count = f.Count(t => t.ModelNode == node.ResourceModel.ProcessName);
				decimal downtime =
					(from i in SourceMachineStates where i.Machine == node.ResourceModel.ProcessName && i.State == "Down" select i.Total)
						.FirstOrDefault();
				decimal idletime =
					(from i in SourceMachineStates where i.Machine == node.ResourceModel.ProcessName && i.State == "Idle" select i.Total)
						.FirstOrDefault();

				foreach (State state in node.ResourceModel.States.Where(t => t.Name == "Down" || t.Name == "Idle"))
				{
					foreach (Consumption consumption in state.Consumptions)
					{
						var res = consumption.Amount/3600000;
						if (!consumption.Static)
						{
							if (state.Name == "Down")
								res += res * downtime;
							else if (state.Name == "Idle")
								res += res * idletime;
						}
						else
							res = res*count;

						if (dic.ContainsKey(consumption.Consumable))
							dic[consumption.Consumable] += res;
						else
							dic.Add(consumption.Consumable, res);
					}
				}
			}

			public override Dictionary<string, decimal> PerTime(ulong timeinterval, string index)
			{
				if (index == "Processed per hour")
				{
					if (Nodes.Count > 0)
					{
						var node = Nodes.Last().Key;
						return node.ResourceModel.Result.PerTime(timeinterval, index);
					}
				}
				else if (index == "Processed")
				{
					if (Nodes.Count > 0)
					{
						var node = Nodes.Last().Key;
						return node.ResourceModel.Result.PerTime(timeinterval, index);
					}
				}
				var dic = new Dictionary<string, decimal>();
				for (ulong i = 0; i < IResults.TotalTime; i += timeinterval)
				{
					dic.Add((i / 3600000).ToString(CultureInfo.InvariantCulture), ConsumptionforInterval(i, i + 3600000, index));
				}
				return dic;
			}

			public override decimal CalculateForOneProduct(Product results, string selectedIndex)
			{
				return results.BillOfMaterials.Where(consumable => consumable.Consumable.Name == selectedIndex).Sum(t => t.Amount);
			}

			protected override ulong TotalProduced(ulong start, ulong end, string productName, ParallelQuery<ProductResult> productResults)
			{
				throw new NotImplementedException();
			}

			public override Dictionary<string, decimal> PerProduct(IEnumerable<Product> products, string index)
			{
				throw new NotImplementedException();
			}
			/**
			public override Dictionary<string, Statistic> CalculateStatistics(ulong interval, ulong start, ulong end, string product, ISet<ParallelQuery<ProductResult>> filtredProductResult,
				string index)
			{
				throw new NotImplementedException();
			}

			public override Dictionary<string, decimal> CalculateIntervalTime(ulong interval, ulong start, ulong end, string product,
				ParallelQuery<ProductResult> filtredProductResult, string index)
			{
				throw new NotImplementedException();
			}

			public override Dictionary<string, decimal> CalculateTime(ulong start, ulong end, string product, ParallelQuery<ProductResult> filtredProductResult, string index)
			{
				throw new NotImplementedException();
			}

			public override Dictionary<string, decimal> CalculatePerProduct(string product, ParallelQuery<ProductResult> filtredProductResult, string index)
			{
				throw new NotImplementedException();
			}

			public override Dictionary<string, decimal> CalculatePerMachine(ParallelQuery<ProductResult> filtredProductResult, string index)
			{
				throw new NotImplementedException();
			}

			public override Dictionary<string, decimal> CalculateIndex(ParallelQuery<ProductResult> filtredProductResult, string index)
			{
				throw new NotImplementedException();
			}
			**/
			//private static Dictionary<string, decimal> _nodeIndicatorBase = new Dictionary<string, decimal>(); 
		   public override void UpdateBaseIndicator()
			{
				//decimal baseforIndex = _nodeIndicatorBase.Sum(t => t.Value);
			}
		}

		void Nodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if(e.OldItems == null)
				return;
			
			var listToRemove = (from Pair<ModelNode, Job> o in e.OldItems select o.Key).ToList();
			
			listToRemove.RemoveAll(p => Nodes.Select(t => t.Key).Contains(p));
			foreach (var modelNode in listToRemove)
			{
				modelNode.ResourceModel.RelatedProducts.Remove(this);
				modelNode.UpdateProductLabels();
			}

			foreach(Pair<ModelNode, Job> currentnode in Nodes)
			{
				currentnode.Key.UpdateProductLabels();
			}
			
			PrintLines();
		}

		public bool CheckChanged
		{
			get { return _checkChanged; }
			set
			{
				if (_checkChanged == value)
					return;
				_checkChanged = value;
				OnPropertyChanged("CheckChanged");
			}
		}

		private bool CanDelete(Pair<ModelNode, Job> result)
		{
			return Nodes.Contains(result);
		}
		private void Delete(Pair<ModelNode, Job> result)
		{
			Nodes.Remove(result);

		}

		private ICommand _mDeleteCommand;

		public ICommand DeleteCommand
		{
			get
			{
				if (_mDeleteCommand == null)
				{
					_mDeleteCommand = new RelayCommand<Pair<ModelNode, Job>>(Delete, CanDelete);
				}
				return _mDeleteCommand;
			}
		}

		private bool CanMove(Pair<ModelNode, Job> p)
		{
			return Nodes.IndexOf(p) != 0;
		}
		private void Move(Pair<ModelNode, Job> result)
		{

			Nodes.Move(Nodes.IndexOf(result), Nodes.IndexOf(result) - 1);
			foreach (var node in Nodes)
			{
				node.Key.UpdateProductLabels();
				node.Key.DrawLine();
			}
			

		}
		private ICommand _moveUpCommand;

		public ICommand MoveUpCommand
		{
			get
			{
				if (_moveUpCommand == null)
				{
					_moveUpCommand = new RelayCommand<Pair<ModelNode, Job>>(Move,CanMove);
				}
				return _moveUpCommand;
			}
		}




		public ObservableCollection<Pair<ModelNode, Job>> Nodes
		{
			get { return _nodes; }

		}

		public Color Color
		{
			get { return _color; }
			set
			{
				_color = value;
				_colorBrush = new SolidColorBrush(Color);
				OnPropertyChanged("Color");
				OnPropertyChanged("ColorBrush");
				if (_nodes.Count > 0 && (IsChecked ?? false))
					PrintLines();
			}
		}

		public Brush ColorBrush
		{
			get { return _colorBrush; }
		}

		public string ProductName
		{
			get { return _productName; }
			set
			{
				_productName = value;
				OnPropertyChanged("ProductName");
				Result.ResultId = ProductName;
			}
		}

		public bool Editmode
		{
			get { return _editMode; }
			set
			{
				_editMode = value;
				OnPropertyChanged("Editmode");
				if (value)
				{
					if (IsVisible)
						Keyboard.Focus(Editbox);
				}
				
			}
		}

		public bool Dropped { get; set; }

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		private void Product_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CheckChanged")
			{
				if (_nodes.Count > 0 && CheckChanged)
					PrintLines();
				else
				{
					HideLines();
				}
			}
		}

		private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
		{
			Editmode = !Editmode;
		}

		private void EditBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				Editmode = false;
			}
			else if (e.Key == Key.Enter)
			{
				Editmode = false;
			}
		}


		public void AddNextMachine(ModelNode newNode, Job job)
		{
			InsertMachine(_nodes.Count, newNode, job);
		}


		public void InsertMachine(int pos, ModelNode newNode, Job job)
		{
			_nodes.Insert(pos, new Pair<ModelNode, Job>(newNode, job));

			if (_nodes.Count > 0 && (IsChecked ?? false))
				PrintLines();
		}


		public void PrintLines()
		{
			HideLines();
			ModelNode prev = null;
			_line = new List<UIElement>();
			Pathdic.Clear();
			int i = 0;
			foreach (var machine in _nodes)
			{
				if (prev != null)
				{
					int linenr = 0;
					foreach (var relatedProduct in machine.Key.ResourceModel.RelatedProducts)
					{
						if(relatedProduct == this)
							break;
						linenr++;
					}
					var b = new Point(prev.Margin.Left + prev.RenderTransform.Value.OffsetX + prev.Width/2,
									  prev.Margin.Top + prev.RenderTransform.Value.OffsetY + prev.Height/2 - 10 );
					var a = new Point(
						machine.Key.Margin.Left + machine.Key.RenderTransform.Value.OffsetX + machine.Key.Width/2,
						machine.Key.Margin.Top + machine.Key.RenderTransform.Value.OffsetY + machine.Key.Height/2 - 10);
					
					var dx = b.X- a.X;
					var dy = b.Y - a.Y;
					double answer = 0;
					if(dx == 0 && dy == 0){
						answer = 0;
					} else if(dx > 0 && dy >= 0 )
					{
						answer = Math.Atan(dy/dx) + Math.PI/4;
					} else if(dx <= 0 && dy > 0)
					{
						answer = Math.Atan(dx / dy) + Math.PI / 4;
					} else if(dx <= 0 && dy <= 0)
					{
						answer = Math.Atan(dy / dx) + Math.PI / 4;
					} else if(dx >= 0 && dy <= 0)
					{
						answer = Math.Atan(dy / dx) + Math.PI / 4;
					}
					b.X = b.X + (Math.Cos(answer) * linenr * 7);
					b.Y = b.Y + (Math.Sin(answer) * linenr * 7);
					a.X = a.X + (Math.Cos(answer) * linenr * 7);
					a.Y = a.Y + (Math.Sin(answer) * linenr * 7);
					
					var g = new StreamGeometry();
					StreamGeometryContext context = g.Open();
					context.BeginFigure(b,true, true);
					context.LineTo(a,true, true);
					context.Close();
					
					var path = new Path();

					path.MouseRightButtonUp += path_MouseRightButtonUp;
					path.Data = g;
					path.RenderTransform = new TranslateTransform();
					var copy = new Color {A = 30, B = Color.B, G = Color.G, R = Color.R};
					Brush v = new LinearGradientBrush(Color, copy, 90);
					path.Stroke = v; //new SolidColorBrush(Colors.BlueViolet);
					path.StrokeThickness = 2.4;
					Pathdic.Add(path,i++);
					_line.Add(path);
				}
				prev = machine.Key;
			}
			if (Grid != null)
			{
				foreach (UIElement path in _line)
				{
					Grid.Add(path);
				}
			}
		}

		Dictionary<Path, int> Pathdic = new Dictionary<Path, int>();
		private bool _showResults;
		private RelayCommand<Product> _menuCommand;

		private Consumption _selectedConsumable;
		private ObservableCollection<Consumption> _billOfMaterials = new ObservableCollection<Consumption>();

		void path_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			var path = sender as Path;
			RemoveMachine(Pathdic[path]);
		}

		private void RemoveMachine(int i)
		{
			if(_nodes.Count > i)
			_nodes.RemoveAt(i);
			
			if (_nodes.Count > 0 && (IsChecked ?? false))
				PrintLines();
		}

		public void HideLines()
		{
			if (_line != null && _line.Count > 0 && Grid != null)
			{
				foreach (UIElement path in _line)
				{
					Grid.Remove(path);
				}
			}
		}

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public string AutomodCodeBody()
		{
			string code = "begin P_" + ProductName + " arriving procedure" +
						  "\r\n    set A_ProductID = V_ProductID" +
						  "\r\n    inc V_ProductID by 1" +
						  "\r\n    set As_ProductType = \"" + ProductName +"\"";
			ModelNode prev = null;


			var first = true;
			for (int i = 0; i < _nodes.Count; i ++)
			{
				ModelNode next = _nodes.Count > i + 1 ? _nodes[i + 1].Key : null;
				code += "\r\n\r\n    //" + _nodes[i].Key.Text;
				int deeph = _nodes[i].Key.GetDeeph(0);
				code += _nodes[i].Key.AutomodProductEnter(deeph, prev, first?ProductName:null);
				first = false;
				string temp = _nodes[i].Key.AutomodProductUse(deeph, _nodes[i].Value);
				code += temp;
				code += _nodes[i].Key.AutomodProductLeave(deeph, next);
				prev = _nodes[i].Key;
			}
			code += "\r\n    send to P_ExportFunction\r\nend";
			return code;
		}

		#region DragDrop

		private void List_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_startPoint = e.GetPosition(null);
		}

		private void List_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton != MouseButtonState.Pressed)
				return;
			// Get the current mouse position
			Point mousePos = e.GetPosition(null);
			Vector diff = _startPoint - mousePos;

			if ((Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
				 Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
			{
				Dropped = false;
				// Initialize the drag & drop operation
				var dragData = new DataObject("Product", this);
				try
				{
					DragDrop.DoDragDrop(this, dragData, DragDropEffects.Move);
				}
				catch
				{}
				IsChecked = true;
			}
		}

		#endregion

		public IResults Result { get; private set; }
		public bool ShowResults
		{
			get { return _showResults; }
			set { _showResults = value; 
			OnPropertyChanged("ShowResults");}
		}

		public ObservableCollection<Consumption> BillOfMaterials
		{
			get { return _billOfMaterials; }
		}

		public RelayCommand<Product> NewConsumptionCommand
		{
			get
			{
				return _menuCommand ??
					   (_menuCommand = new RelayCommand<Product>(t=> BillOfMaterials.Add(SelectedConsumption.Copy())));
			}
		}

		public  Consumption SelectedConsumption
		{
			get { return _selectedConsumable ?? (_selectedConsumable = new Consumption(){}); }
			set { _selectedConsumable = value;
			OnPropertyChanged("SelectedConsumption");
			}
		}

		private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (ShowResults)
			{

				return;
			}
			var win = new Window
				{
					Width = 650,
					Height = 400,
					Topmost = true,
					WindowStartupLocation = WindowStartupLocation.CenterOwner,
					WindowStyle = WindowStyle.ToolWindow,
					Owner = Window.GetWindow(this)
				};
			win.Content = new DefineProduct()
				{
					DataContext = this
				};
			win.Show();
		}
	}
	
}