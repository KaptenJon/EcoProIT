using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EcoProIT.DataLayer;
using EcoProIT.UserControles.Annotations;
using EcoProIT.UserControles.Models;
using EcoProIT.UserControles.View;
using FeserWard.Controls;
using GalaSoft.MvvmLight.CommandWpf;
using HelpClasses;

namespace EcoProIT.UserControles.ViewModel
{
    [Serializable]
    public class ResourceDefinitionModel : INotifyPropertyChanged, IHasResults
    {
        public static IEnumerable<ModelNode> Nodes;
        [NonSerialized] private UserControl _selectedView;
        private string _processName = "ResultId";
        private readonly ObservableCollection<Job> _job = new ObservableCollection<Job>();
        //private readonly ObservableProduct _rimProducts;
        //private readonly RIMConnectionDataContext connection = new RIMConnectionDataContext();
        [NonSerialized] public CommandBinding CustomCommandBinding;
        [NonSerialized] private Product _selectedProduct;

        private NodeResults _nodeNodeResults;
        private List<ModelNode> _children;

        public ResourceDefinitionModel()
        {
            //_rimProducts = new ObservableProduct(connection);
            _nodeNodeResults = new NodeResults() { ResultId = ProcessName, States = States };
            _nodeNodeResults.PropertyChanged += NodeNodeResultsPropertyChanged;
            AddSubJobClickAction = new RelayCommand(AddSubJob);
            Job.CollectionChanged += Job_CollectionChanged;
            _states.Add(_idle);
            _states.Add(_down);
            _states.Add(_off);
            SelectedState = _idle;
            Result.PropertyChanged += Result_PropertyChanged;
            if(IntelliLCISearch == null)
                IntelliLCISearch = new ConsumableELCDDatabaseSearch();
        }

        void Result_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "NodeIndicator")
                NotifyPropertyChanged("Gradient");
        }

        #region relatedUserControles

        public List<ModelNode> Children
        {
            set { _children = value; }
            get { return _children; }
        }


        #endregion

        private void Job_CollectionChanged(object sender,
                                           NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                SelectedJob = (Job) e.NewItems[0];
            }
        }



        public RelayCommand AddSubJobClickAction { get; set; }
        private RelayCommand<UserControl> _menuCommand;

        public RelayCommand<UserControl> MenuCommand
        {
            get
            {
                return _menuCommand ??
                       (_menuCommand = new RelayCommand<UserControl>(
                                           s => SelectedView = s,
                                           s => s != null
                                           ));
            }
        }
        private RelayCommand<object> _addNewConsumable;
        
        public RelayCommand<object> AddNewConsumable
        {
            get
            {
                return _addNewConsumable ??
                       (_addNewConsumable = new RelayCommand<object>(
                                           (s) => {
                                                var con = s as Consumable;
                                                      if (con != null && !ConnectLCIDB.Consumebles.Contains(con))
                                                      {
                                                          ConnectLCIDB.Consumebles.Add(con);
                                                          if(SelectedConsumption != null)
                                                            SelectedConsumption.Consumable = con;
                                                      }
                                           },
                                                (s) => s is Consumable
                                           ));
            }
        }
        public static IIntelliboxResultsProvider IntelliLCISearch { private set; get; }
        
        private RelayCommand _addJobClickAction;

        public RelayCommand AddJobClickAction
        {
            get
            {
                return _addJobClickAction ??
                       (_addJobClickAction = new RelayCommand(
                                                 () => Job.Add(new Job {Name = "NewJob"})));
            }
        }

        private RelayCommand _removeJobClickAction;

        public RelayCommand RemoveJobClickAction
        {
            get
            {
                return _removeJobClickAction ??
                       (_removeJobClickAction = new RelayCommand(
                                                 () => {
                                                           foreach (var relatedProduct in RelatedProducts)
                                                           {
                                                               var pairstoRemove = new List<Pair<ModelNode, Job>>();
                                                               foreach (var jobpair in relatedProduct.Nodes.Where(t=>t.Key.ResourceModel == this && t.Value == SelectedJob))
                                                               {
                                                                   pairstoRemove.Add(jobpair);
                                                               }
                                                               foreach (var pair in pairstoRemove)
                                                               {
                                                                   relatedProduct.Nodes.Remove(pair);
                                                               }
                                                               
                                                           }
                                                            Job.Remove(SelectedJob);
                                                    }));
            }
        }

        private void NodeNodeResultsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "NodeIndicator")
            {
                NotifyPropertyChanged("Intensity");
                NotifyPropertyChanged("Intensity2");
            }
        }

        private bool CanDelete(SubJob p)
        {
            if (SelectedJob == null || SelectedJob.Subjobs == null)
                return false;
            return SelectedJob.Subjobs.Contains(p);
        }

        private ICommand _mDeleteCommand;

        public ICommand DeleteCommand
        {
            get
            {
                if (_mDeleteCommand == null)
                {
                    _mDeleteCommand = new RelayCommand<SubJob>(Delete, CanDelete);
                }
                return _mDeleteCommand;
            }
        }

        private void Delete(SubJob result)
        {
            SelectedJob.Subjobs.Remove(result);
            SelectedSubJob = new SubJob();
        }



        public IResults Result
        {
            get
            {
                return _nodeNodeResults;
            }
        }

        private static readonly Color BaseColor = (Color) ColorConverter.ConvertFromString("#D63398EB");
        private static readonly Color BaseColor2 = (Color) ColorConverter.ConvertFromString("#FF68E5F9");

        public LinearGradientBrush Gradient
        {

            get
            {
                if (ShowResults)
                {
                    if (Result.NodeIndicator < (decimal) 0.5)
                        return new LinearGradientBrush(Red, Green,
                                                       new Point((double)Result.NodeIndicator, 0.5),
                                                       new Point((double) Result.NodeIndicator + 0.1, 0.5))
                            {
                                Opacity = 30
                            };
                    else
                    {
                        return new LinearGradientBrush(Red, Green,
                                                       new Point((double)Result.NodeIndicator - 0.1, 0.5),
                                                       new Point((double)Result.NodeIndicator , 0.5))
                            {
                                Opacity = 30
                            };
                    }
                }
                return new LinearGradientBrush(BaseColor, BaseColor2, new Point(1, 0.5), new Point(0, 0.5))
                    {
                        Opacity = 30
                    };
            }
        }

        protected Color Green
        {
            get { return Colors.Chartreuse; }
        }

        protected Color Red
        {
            get { return Colors.Crimson; }
        }

        public bool ShowResults
        {
            get { return _showResults; }
            set
            {
                _showResults = value;
                NotifyPropertyChanged("Gradient");
                NotifyPropertyChanged("ShowResults");
                NotifyPropertyChanged("ShowEmissions");
            }
        }

        public Product SelectedProduct
        {
            get { return _selectedProduct; }
            set
            {
                _selectedProduct = value;
                NewProductSelected();
                NotifyPropertyChanged("SelectedProduct");
            }
        }

        private int _capacity = 0;

        public int Capacity
        {
            get { return _capacity; }
            set
            {
                if (_capacity == value)
                    return;
                _capacity = value;
                NotifyPropertyChanged("Capacity");
            }
        }

        #region Consumption

        private Consumption _selectedConsumption;

        public Consumption SelectedConsumption
        {
            get { return _selectedConsumption ?? new Consumption(); }
            set
            {
                
                if (value == null)
                    _selectedConsumption = new Consumption();
               
                else
                {
                    
                    if (_selectedConsumption == value)
                        return;
                    _selectedConsumption = value;
                }
                NotifyPropertyChanged("SelectedConsumption");
            }
        }


        public ObservableCollection<Consumption> SelectedConsumptions
        {
            get { return SelectedState == null ? new ObservableCollection<Consumption>() : SelectedState.Consumptions; }
        }



        private RelayCommand _newConsumptionCommand;

        public RelayCommand NewConsumptionCommand
        {
            get
            {
                return _newConsumptionCommand ??
                       (_newConsumptionCommand = new RelayCommand(
                                                     () =>
                                                         {
                                                             
                                                             SelectedConsumptions.Add(_selectedConsumption.Copy());
                                                             SelectedConsumption = new Consumption();
                                                         }));
            }
        }

        private ISet<Product> _relatedProducts = new HashSet<Product>();
        public ISet<Product> RelatedProducts
        {
            get { return _relatedProducts; }
        }

        private ICommand _deleteConsumptionCommand;

        public ICommand DeleteConsumptionCommand
        {
            get
            {
                if (_deleteConsumptionCommand == null)
                {
                    _deleteConsumptionCommand = new RelayCommand<Consumption>(DeleteConsumption, CanDeleteConsumption);
                }
                return _deleteConsumptionCommand;
            }
        }

        private void DeleteConsumption(Consumption result)
        {
            SelectedConsumptions.Remove(result);
            SelectedConsumption = new Consumption();
        }

        private bool CanDeleteConsumption(Consumption p)
        {
            return SelectedConsumptions.Contains(p);
        }

        #endregion

        #region Job

        private void AddSubJob()
        {
            SubJob subjob = SelectedSubJob.Copy();
            SelectedJob.Subjobs.Add(subjob);
            SelectedSubJob = subjob;
        }

        public ObservableCollection<Job> Job
        {
            get { return _job; }
        }

        private Job _selectedJob;
        private bool _showResults;

        public Job SelectedJob
        {
            get { return _selectedJob; }
            set
            {
                if (value == _selectedJob)
                    return;

                if (value != null)
                {
                    var remove = _states.FirstOrDefault(t => t.Name == "Processing" || t.Name == "Used");
                    _states.Remove(remove);
                }

                _selectedJob = value;

                if (value == null)
                {
                    SelectedState = _idle;
                }
                else
                {
                    if(!_states.Contains(value.State))
                        _states.Insert(0, value.State);
                    SelectedSubJob = new SubJob();
                    NotifyPropertyChanged("SelectedSubJobs");
                    if (value.State != SelectedState)
                        SelectedState = value.State;
                }

                NotifyPropertyChanged("SelectedJob");
            }
        }


        public ObservableCollection<SubJob> SelectedSubJobs
        {
            get
            {
                if (_selectedJob != null)
                    return SelectedJob.Subjobs;
                return null;
            }
        }



        private SubJob _selectedSubJob;

        public SubJob SelectedSubJob
        {
            get { return _selectedSubJob; }
            set
            {

                if (_selectedSubJob == value)
                    return;
                if (value == null)
                    _selectedSubJob = new SubJob();
                else
                    _selectedSubJob = value;
                NotifyPropertyChanged("SelectedSubJob");
            }
        }

        private String _descriptionSelectedSubJob;

        public String DescriptionSelectedSubJob
        {
            get { return _descriptionSelectedSubJob; }
            set
            {
                if (_descriptionSelectedSubJob == value)
                    return;
                _descriptionSelectedSubJob = value;
                NotifyPropertyChanged("DescriptionSelectedSubJob");
            }
        }

        public ICommand SelectionChangedCommand { set; get; }

        private State _selectedState;

        public State SelectedState
        {
            get { return _selectedState; }
            set
            {
                if (_selectedState == value)
                    return;

                if (!Job.Select(job => job.State).Contains(value))
                {
                    _selectedState = value;
                    SelectedJob = null;
                }
                else if (SelectedJob == null)
                {

                    SelectedJob = Job.FirstOrDefault(t => t.State == value);
                }
                if (SelectedJob != null)
                {
                    _selectedState = SelectedJob.State;
                    NotifyPropertyChanged("States");

                }
                else
                    _selectedState = value;

                SelectedConsumption = new Consumption();
                NotifyPropertyChanged("SelectedConsumptions");
                NotifyPropertyChanged("SelectedState");
            }
        }

        #endregion

        #region INotifyPropertyChanged

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        #region States

        private State _down = new State("Down");

        public State Down
        {
            get { return _down; }
        }

        private State _idle = new State("Idle");

        public State Idle
        {
            get { return _idle; }
        }

        private State _off = new State("Off");

        public State Off
        {
            get { return _off; } 
        }

        public ObservableCollection<State> _states = new ObservableCollection<State>();
        public IEnumerable<State> States
        {
            get
            {
                return _states;
            }
        }

        #endregion

        public void NewProductSelected()
        {
            /**
            var job = from p in connection.Products
                      join pe in connection.PlannedEfforts on p.product_no equals pe.product_no
                      join j in connection.Jobs on pe.job_no equals j.job_no
                      where p.product_no == SelectedProduct.product_no
                      select new {name = j.job_name + " " + j.job_description, p};
            _job.Clear();
            foreach (var j in job)
            {
                _job.Add(j.name);
            }
             */
        }

        public UserControl SelectedView
        {
            get
            {
                if (_selectedView == null)
                    _selectedView = new NodeDef();
                return _selectedView;
            }
            set
            {
                _selectedView = value;
                NotifyPropertyChanged("SelectedView");
            }
        }



        public string ProcessName
        {
            get { return _processName; }
            set
            {
                if(_processName == value)
                     return;
                if (Nodes.Any(t => t.ResourceModel.ProcessName == value))
                {
                    var node = Nodes.First(t => t.ResourceModel.ProcessName == value);
                    int i = 0;
                    if(Int32.TryParse(node.ResourceModel.ProcessName.Last() + "", out i))
                        ProcessName = value.Substring(0, value.Length - 1) + ++i;
                    else
                        ProcessName = value + 1;
                    return;
                }
                _processName = value;
                Result.ResultId = value;
                NotifyPropertyChanged("ResultId");
                NotifyPropertyChanged("ProcessName");
            }
        }

        #region MTTR/MTTF

        public sealed class DistContainer:IHasDistribution
        {
            private IDistribution _distribution;
            public event PropertyChangedEventHandler PropertyChanged;

            public IDistribution Distribution
            {
                get { return _distribution; }
                set
                {
                    _distribution = value;
                    OnPropertyChanged();
                }
            }

            [NotifyPropertyChangedInvocator]
            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private DistContainer _mTBF = new DistContainer();
        public DistContainer MTBF
        {
            get { return _mTBF; }
        }

        private DistContainer _mTTR = new DistContainer();
       

        public DistContainer MTTR
        {
            get { return _mTTR; }
        }

        private bool _isProcess;

        public bool IsProcess
        {
            get { return _isProcess; }
            set
            {
                if (_isProcess == value)
                    return;
                _isProcess = value;
                HasBreakdown = value;
                NotifyPropertyChanged("IsProcess");
            }
        }   

        public String AutoModBreakdownCode
        {
            get
            {
                string code = "";
                if(MTBF.Distribution != null && MTTR.Distribution != null)
                    code = "begin P_"+ProcessName+"BreakDown arriving procedure\r\n" +
                              "    while 1=1 begin\r\n" +
                              "        wait for " + MTBF.Distribution.AutomodCode + "\r\n" +
                              "        take down R_" + ProcessName + "\r\n" +
                              "        wait for "+ MTTR.Distribution.AutomodCode + "\r\n" +
                              "        bring up R_" + ProcessName + "\r\n" +
                              "    end\r\n" +
                              "end";
                return code;
            }
        }

        public bool IsTransport { get; set; }

        //Speed in meter per sec, used for transports
        public decimal Speed
        {
            get { return _speed; }
            set { _speed = value;
            NotifyPropertyChanged("Speed");}
        }

        //Length in meter, used for transport.
        public decimal Length
        {
            get { return _length; }
            set { _length = value;
            NotifyPropertyChanged("Length");
            }
        }
        //used for transport
        private readonly ObservableCollection<Pair<ModelNode, decimal>> _distanceBetweenChildrenNodesWithNodes 
            = new ObservableCollection<Pair<ModelNode, decimal>>();
        public ObservableCollection<Pair<ModelNode, decimal>> DistanceBetweenChildrenNodesWithNodes
        {
            get { return _distanceBetweenChildrenNodesWithNodes; }
        }

        public decimal DistansToNode
        {
            get { return _distansToNode; }
            set { _distansToNode = value;
            NotifyPropertyChanged("DistansToNode");
            }
        }

        public bool HasBreakdown
        {
            get
            {
                if (MTBF.Distribution == null || MTTR.Distribution == null)
                    return false;
                return _hasBreakdown;
            }
            set { _hasBreakdown = value; }
        }

        private Pair<ModelNode, decimal> _selectedTransport = new Pair<ModelNode, decimal>();
        public Pair<ModelNode, decimal> SelectedTransport
        {
            get { return _selectedTransport; }
            set
            {
                _selectedTransport = value;
                NotifyPropertyChanged("SelectedTransport");
            }
        }



        #endregion

        private Resource _oldCopy;
        private List<CMSDDocumentDataSectionJob> _oldjobCopy;
        private decimal _speed;
        private decimal _length;
        private decimal _distansToNode;


        public void SetPrev()
        {
            _oldCopy = HarvestResourceDefinition();
            _oldjobCopy = HarvestJobs(_oldCopy);
        }

        public void RestorePrev()
        {
            if (_oldCopy != null && _oldjobCopy != null)
            {
                SetDefinitionModel(_oldCopy,_oldjobCopy);
            }
        }

        public Resource HarvestResourceDefinition()
        {
            string resType = null, id = null;
            id = ProcessName;
            
            var properties = new List<ResourceProperty>();
            if (MTBF.Distribution != null)
                properties.Add(new ResourceProperty()
                {
                    Distribution = new[] { MTBF.Distribution.CMSDDistribution },
                    Name = "MTBF",
                    Unit = MTBF.Distribution.ParameterConverter.UnitType.ToString()
                });
            if (MTTR.Distribution != null)
                properties.Add(new ResourceProperty()
                {
                    Distribution = new[] { MTTR.Distribution.CMSDDistribution },
                    Name = "MTTR",
                    Unit = MTTR.Distribution.ParameterConverter.UnitType.ToString()
                });
            
            properties.Add(new ResourceProperty() { Name = "Capacity", Value = Capacity + "" });

            if (Idle != null)
                foreach (var consumption in Idle.Consumptions)
                    HarvestConsumption(properties, consumption, "Idle");
            if (Off != null)
                foreach (var consumption in Off.Consumptions)
                    HarvestConsumption(properties, consumption, "Off");
            if (Down != null)
                foreach (var consumption in Down.Consumptions)
                    HarvestConsumption(properties, consumption, "Down");
            foreach (var declaredJob in Job)
                foreach (var consumption in declaredJob.State.Consumptions)
                    HarvestConsumption(properties, consumption, declaredJob.Name);
            if (IsTransport)
            {
                HarvestDistances(this, properties);
            }
            return new Resource()
            {
                Name = id,
                Identifier = id,
                ResourceType = resType,
                Property = properties.ToArray()

            };
        }

        private static void HarvestConsumption(List<ResourceProperty> properties, Consumption consumption, string state)
        {
            properties.Add(new ResourceProperty()
            {
                Name =
                    state + ":" + consumption.Consumable.Name + ":" + (consumption.AllocationPerTime ? "PerTime:" : "PerUsed:") +
                    (consumption.Static ? "OneTime:" : "TimeDependent"),
                Value = consumption.Amount.ToString(CultureInfo.InvariantCulture),
                Unit = consumption.UnitType
            });
        }
        public void SetDefinitionModel(Resource resource, IEnumerable<CMSDDocumentDataSectionJob> CMSDjobs)
        {
            foreach (ResourceProperty resourceProperty in resource.Property)
            {
                if (resourceProperty != null)
                {
                    switch (resourceProperty.Name)
                    {
                        case "MTTR":
                            var distsource = resourceProperty.Distribution.First();
                            MTTR.Distribution = IDistribution.GetFromCmsd(distsource);
                            MTTR.Distribution.ParameterConverter.UnitType = GeneralConverter.GetInstance(resourceProperty.Unit);
                            break;
                        case "MTBF":
                            distsource = resourceProperty.Distribution.First();
                            MTBF.Distribution = IDistribution.GetFromCmsd(distsource);
                            MTBF.Distribution.ParameterConverter.UnitType = GeneralConverter.GetInstance(resourceProperty.Unit);
                            break;
                        case "Capacity":
                            try
                            {
                                Capacity = Int32.Parse(resourceProperty.Value);
                            }
                            catch
                            {
                            }
                            break;
                        case "Speed":
                            try
                            {
                                Speed = Int32.Parse(resourceProperty.Value,CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                            }
                            break;
                        case "DistanceToNode":
                            try
                            {
                                DistansToNode = int.Parse(resourceProperty.Value, CultureInfo.InvariantCulture);
                            }
                            catch
                            { }
                            break;
                    }
                }
            }

            ProcessName = resource.Name;

            AddConsumption(resource.Property.FirstOrDefault(t => t.Name.StartsWith("Idle")), Idle);
            AddConsumption(resource.Property.FirstOrDefault(t => t.Name.StartsWith("Off")), Off);
            AddConsumption(resource.Property.FirstOrDefault(t => t.Name.StartsWith("Down")), Down);

            Job.Clear();
            var cmsdDocumentDataSectionJobs = CMSDjobs as IList<CMSDDocumentDataSectionJob> ?? CMSDjobs.ToList();
            foreach (var sectionJob in cmsdDocumentDataSectionJobs.Where(t => t.Identifier.StartsWith("BaseJob")))
            {
                var job = new Job()
                    {
                        Name = sectionJob.Identifier.Remove(0, 7).Replace(ProcessName, "")
                    };
                AddConsumption(resource.Property.FirstOrDefault(t=>t.Name.Contains(job.Name)),job.State);
                job.Subjobs.Clear();
                foreach (var pointSubJob in sectionJob.SubJob)
                {
                    var cmsdSubJob = cmsdDocumentDataSectionJobs.FirstOrDefault(t => t.Identifier == pointSubJob.JobIdentifier);
                    if (cmsdSubJob != null)
                    {
                        var sub = new SubJob()
                            {
                                Description = cmsdSubJob.Description,
                                Distribution = IDistribution.GetFromCmsd(cmsdSubJob.PlannedEffort.ProcessingTime.Distribution)

                            };
                        sub.Distribution.ParameterConverter.UnitType = GeneralConverter.TimeConverters.FirstOrDefault(
                                t => t.ToString() == cmsdSubJob.PlannedEffort.ProcessingTime.TimeUnit);
                        job.Subjobs.Add(sub);
                    }
                }
                
                Job.Add(job);
                LoadDistances(new []{this}, new []{resource});

            }
            foreach (var reloadJob in _reloadJobs)
            {
                var productcollection = reloadJob.Key.Nodes.Where(t => t.Key.ResourceModel == this).GetEnumerator();
                foreach (var jobstring in reloadJob.Value)
                {
                    productcollection.MoveNext();
                    productcollection.Current.Value = Job.First(t => t.Name == jobstring);
                }
            }
        }
        private Dictionary<Product,List<string>> _reloadJobs = new Dictionary<Product, List<string>>();
        private bool _hasBreakdown;

        public  List<CMSDDocumentDataSectionJob> HarvestJobs(Resource res)
        {
            _reloadJobs.Clear();
            foreach (var relatedProduct in RelatedProducts)
            {
                var relodedlist = new List<string>();
                foreach (var jobname in relatedProduct.Nodes.Where(t => t.Key.ResourceModel == this))
                {
                     relodedlist.Add(jobname.Value.Name);
                }
                _reloadJobs.Add(relatedProduct, relodedlist);
            }
            var list = new List<CMSDDocumentDataSectionJob>();
            foreach (Job declaredJob in Job)
            {
                var subjobs = new List<CMSDDocumentDataSectionJob>();
                int i = 1;
                foreach (SubJob subjob in declaredJob.Subjobs)
                {

                    subjobs.Add(new CMSDDocumentDataSectionJob()
                    {
                        PlannedEffort =
                            new CMSDDocumentDataSectionJobPlannedEffort()
                            {
                                PartType =
                                    RelatedProducts.Select(
                                        t => new CMSDDocumentDataSectionJobPlannedEffortPartType()
                                        {
                                            PartTypeIdentifier = t.ProductName
                                        }).ToArray(),
                                ProcessingTime = new CMSDDocumentDataSectionJobPlannedEffortProcessingTime()
                                {
                                    TimeUnit = subjob.Distribution.ParameterConverter.UnitType.ToString(),
                                    Distribution = subjob.Distribution.CMSDDistribution
                                }
                            },
                        Description = subjob.Description,
                        Identifier = res.Identifier + declaredJob.Name + i++
                    });
                }
                list.AddRange(subjobs);
                

                list.Add(new CMSDDocumentDataSectionJob()
                {
                    SubJob = subjobs.Select(t => new CMSDDocumentDataSectionJobSubJob()
                    {
                        JobIdentifier = t.Identifier
                    }).ToArray(),
                    Identifier = "BaseJob" + res.Identifier + declaredJob.Name,
                    PlannedEffort = new CMSDDocumentDataSectionJobPlannedEffort()
                    {
                        PartType =
                            RelatedProducts.Select(
                                t => new CMSDDocumentDataSectionJobPlannedEffortPartType()
                                {
                                    PartTypeIdentifier = t.ProductName
                                }).ToArray(),
                        ResourcesRequired = new Resource[] { res },
                    }
                });
            }
            return list;
        }

        private static void AddConsumption(ResourceProperty property, State state)
        {
            if (property == null)
                return;
            var consumable = ConnectLCIDB.GetFromName(property.Name.Split(':')[1]);
            state.Consumptions.Add(new Consumption()
            {
                AllocationPerTime = property.Name.Contains("PerTime"),
                Static = property.Name.Contains("OneTime"),
                Amount = Decimal.Parse(property.Value, CultureInfo.InvariantCulture),
                Consumable = consumable
            });
        }

        public static void LoadDistances(IEnumerable<ResourceDefinitionModel> nodes, Resource[] defs)
        {
            foreach (var resource in defs)
            {
                var node = nodes.FirstOrDefault(t => resource.Name == t.ProcessName);
                foreach (var distances in resource.Property.Where(t => t.Name.StartsWith("DistanceBetweenNodes")))
                {
                    var setdest = node.DistanceBetweenChildrenNodesWithNodes.FirstOrDefault(
                        t => t.Key.ResourceModel.ProcessName == distances.Name.Remove(0, 20));
                    if (setdest != null)
                        setdest.Value = Decimal.Parse(distances.Value, CultureInfo.InvariantCulture);
                }
            }
        }

        public static void HarvestDistances(ResourceDefinitionModel resourceModel, List<ResourceProperty> properties)
        {
            properties.Add(new ResourceProperty()
                {
                    Name = "Speed",
                    Value = resourceModel.Speed.ToString(CultureInfo.InvariantCulture)
                });
            properties.Add(new ResourceProperty()
                {
                    Name = "DistanceToNode",
                    Value = resourceModel.DistansToNode.ToString(CultureInfo.InvariantCulture)
                });
            foreach (var resourceProperty in resourceModel.DistanceBetweenChildrenNodesWithNodes)
            {
                properties.Add(new ResourceProperty()
                    {
                        Name = "DistanceBetweenNodes" + resourceProperty.Key.ResourceModel.ProcessName,
                        Value = resourceProperty.Value.ToString(CultureInfo.InvariantCulture)
                    });
            }
        }
    }
}
