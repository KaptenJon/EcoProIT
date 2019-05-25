using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using EcoProIT.DataLayer;
using EcoProIT.UI.ViewModel;
using EcoProIT.UserControles;
using EcoProIT.UserControles.ViewModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using HelpClasses;
using HelpClasses.Annotations;

namespace EcoProIT.UI.SimulationModel
{
    [System.Runtime.InteropServices.GuidAttribute("D0624425-0F8D-4EB2-AA5A-7997A3F75F7B")]
    public class SimulationHandler : INotifyPropertyChanged


    {

        private List<SimulationModel> sims = new List<SimulationModel>();
        private double _status;

        private bool _isRunning;
        private long _nrOfIterations = System.Environment.ProcessorCount;


        private void CreateModel(IEnumerable<Product> productenumerable, ulong simulationTime)
        {
            var products = productenumerable.ToList();
            sims.Clear();
            //simprogress.Value = 0;

            var involvedproductsjobs = products.Select(t => t.Nodes).Distinct();

            for (int i = 0; i < _nrOfIterations; i++)
            {
                var machines = new List<DESMachine>();
                var sim = new SimulationModel(){ClosingTime = simulationTime};
                var enumerable = involvedproductsjobs as ObservableCollection<Pair<ModelNode, Job>>[] ?? involvedproductsjobs.ToArray();
                
                foreach (var involvedproductsjob in enumerable)
                {
                    foreach (var pair in involvedproductsjob)
                    {
                        var machine = machines.FirstOrDefault(t => t.Name == pair.Key.ResourceModel.ProcessName);
                        if (machine == null)
                        {
                            DESMachine parent = CreateParentMachines(pair.Key, machines, sim);
                            machine = new DESMachine(sim, pair.Key, parent);
                            machines.Add(machine);
                        } 
                    }
                }
                sim.AddMachines(machines);

                foreach (var product in products)
                {
                    sim.AddProduct(product.ProductName,
                        product.Nodes.Select(
                            t =>
                                new Pair<DESMachine, Job>(
                                    machines.FirstOrDefault(p => p.Name == t.Key.ResourceModel.ProcessName), t.Value))
                            .ToList());
                }


                sim.PropertyChanged += sim_PropertyChanged;

                sims.Add(sim);
            }
        }

        private static DESMachine CreateParentMachines(ModelNode node, List<DESMachine> machines, SimulationModel sim)
        {
            DESMachine DESNode = null;
            if (node.ParentNode != null)
            {
                DESMachine parent = CreateParentMachines(node.ParentNode, machines, sim);
                DESNode = machines.FirstOrDefault(t => t.Name == node.ParentNode.ResourceModel.ProcessName);
                if (DESNode == null)
                {
                    DESNode = new DESMachine(sim, node.ParentNode, parent);
                    machines.Add(DESNode);
                }
            }
            return DESNode;
        }

        public double Status
        {
            get { return _status; }
            set
            {
                if (value.Equals(_status)) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        public void sim_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(new Action(() => Status = (int)(sims.Average(t => t.RunStatus) * 100.0 )));
        }

        internal async void run(IEnumerable<Product> products, ulong simulationTime)
        {
            
            IsRunning = true;

            CreateModel(products, simulationTime);
            List<Task> runs = new List<Task>();
            var db = DataLayer.DatabaseConnection.GetModelContext();

        foreach (SimulationModel sim in sims)
            {
                if (sim != null)
                {
                    SimulationModel sim1 = sim;
                    //sim.Run();
                    runs.Add(new Task(() => sim1.Run()));
                    runs.Last().Start();
                }
            }
            var waitforall = new Task(() => Task.WaitAll(runs.ToArray()));
            waitforall.Start();
            await waitforall;
            IsRunning = false;
            Status = 100;
        }

        public void StopSimulation()
        {
            foreach (var s in sims)
            {
                s.ClosingTime = s.Now;
            }
        }

        public bool IsRunning
        {
            get { return _isRunning || ViewModelBase.IsInDesignModeStatic; }
            set
            {
                if (value.Equals(_isRunning)) return;
                _isRunning = value;
                OnPropertyChanged();
            }
        }

        public bool HasResults {
            get
            {
                return sims.Any() &&
                       sims.First()
                           .MachineStates.Contains(new MachineState() {Machine = "BaseModel", State = "BaseTime"});
            }  }

        public long NrOfIterations
        {
            get { return _nrOfIterations; }
            set
            {
                if (value == _nrOfIterations) return;
                _nrOfIterations = value;
                OnPropertyChanged();
            }
        }

        public List<SimulationModel> Simulations
        {
            get
            {
                if (IsRunning)
                    return null;
                else
                {
                    return sims;
                }
            }
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
