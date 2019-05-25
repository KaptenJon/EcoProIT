using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EcoProIT.DataLayer;
using EcoProIT.UI.Properties;
using EcoProIT.UserControles;
using HelpClasses;
using React;
using React.Distribution;

namespace EcoProIT.UI.SimulationModel
{
    
    public class SimulationModel : Simulation, INotifyPropertyChanged
    {
        private List<DESMachine> machines = new List<DESMachine>();
        private ulong _closingTime = 10*8*3600*1000;
        private double _runStatus;
        private List<Pair<string, List<Pair<DESMachine, Job>>>> products = new List<Pair<string, List<Pair<DESMachine, Job>>>>();
        private List<ProductResult> _productResult = new List<ProductResult>();
        private List<MachineState> _mashinStates = new List<MachineState>(); 

        public void AddMachines(List<DESMachine> tasks)
        {
            machines.Clear();
            machines.AddRange(tasks);
            var process = new Process(this, GetTasks);
            process.Activate(this);
        }
        public void AddProduct(string productName, List<Pair<DESMachine, Job>> jobs)
        {
            products.Add(new Pair<string, List<Pair<DESMachine, Job>>>(productName, jobs));
        }
        
        public ulong ClosingTime
        {
            get { return _closingTime; }
            set { _closingTime = value; }
        }

        private IEnumerator<DesTask> GetTasks(Process process, object data)
        {

            do
            {
                //start one process for each product!
                foreach (var product in products)
                {
                    var jobdesmachine = product.Value.FirstOrDefault();
                    if (jobdesmachine != null)
                    {
                        if (jobdesmachine.Key != null)
                        {
                            if (jobdesmachine.Key.Machine.BlockCount <= 1)
                            {
                                var c = new DESProduct(this, product.Key, product.Value);
                                c.Activate(this, 0L);
                            }
                            else
                            {
                                yield return jobdesmachine.Key.Machine.Acquire(process);
                                yield return jobdesmachine.Key.Machine.Release(process);
                            }
                            yield return process.Delay(1);
                            RunStatus = ((Now*1.0)/(_closingTime*1.0));
                        }
                    }
                }
            } while (Now < _closingTime);
            Stop();
            _mashinStates.Add(new MachineState() {Machine = "BaseModel", State = "BaseTime", Total = Now});
            
        }

        public double RunStatus   
        {
            get { return _runStatus; }
            private set
            {
                if (value == _runStatus) return;
                _runStatus = value;
                OnPropertyChanged();
            }
        }

        

        public List<DESMachine> Machines
        {
            get { return machines; }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public IEnumerable<ProductResult> ProductResultTables
        {
            get { return _productResult; }
        }
        public IEnumerable<MachineState> MachineStates
        {
            get { return _mashinStates; }
        }
        public void AddProductEvent(ProductResult productResult)
        {
            _productResult.Add(productResult);
        }
        public void AddMachineStateEvent(MachineState machineState)
        {
            _mashinStates.Add(machineState);
        }
    }
}
