using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using EcoProIT.DataLayer;
using EcoProIT.UserControles.ViewModel;
using React;
using React.Distribution;
using Process = React.Process;

namespace EcoProIT.Main.SimulationModel
{

    public class SimulationMachine:Process, INotifyPropertyChanged
    {
        string name;
        private Normal dist;
        private Exponential breakdown;
        private Exponential repair;
        private DESResource _machine = DESResource.Create(1);
        private int _used;

        public SimulationMachine(Simulation sim, string name, double mean, double std, double breakdownmean, double repairmean):base(sim)
        {
            this.name = name;
            this.breakdown = new Exponential(breakdownmean);
            this.repair = new Exponential(repairmean);
            dist = new Normal(mean, std);
            GetBreakdown(sim).Activate(sim);
        }

        public Process GetBreakdown(Simulation sim)
        {
            return new Process(sim, Breakdown);
        }

        protected override IEnumerator<DesTask> GetProcessSteps()
        {
            yield return Delay( (long) dist.NextDouble());
            Used++;
            yield break;
        }
        protected IEnumerator<DesTask> Breakdown(Process proc, object brd)
        {
            while (proc.Simulation.State == SimulationState.Running)
            {
                var d = (long) breakdown.NextDouble();
                var u = proc.Simulation.Now;
                yield return proc.Delay(d);
                //yield return _machine.Acquire(proc);
                _machine.OutOfService++;
                var p = proc.Simulation.Now - u;
                long t = proc.Simulation.Now;
                d = (long) repair.NextDouble();
                yield return proc.Delay(d);
                long v = proc.Simulation.Now - t;
                _machine.OutOfService--;
                //yield return _machine.Release(proc);
            }
            yield break;
        }

        public DESResource Machine
        {
            get { return _machine; }
        }

        public int Used
        {
            private set
            {
                if (value == _used) return;
                _used = value;
                OnPropertyChanged();
            }
            get { return _used; }
        }

        public Job[] Jobs { get; set; }

        public Process ProcessJob(Product desProduct)
        {
            var pair = desProduct.Nodes.FirstOrDefault(t => t.Key.Name == Name);
            if (pair != null)
            {
                var job = pair.Value;
                Used++;
                return new Process(Simulation, (process, o) => Jobsiterator(process, o,job));
                            
            }
            else
            {
                Debug.Assert(pair == null,"This should never happen...");
                return null; 
            }
            
            
        }

        private IEnumerator<DesTask> Jobsiterator(Process process, object brd,Job job)
        {
            foreach (var sub in job.Subjobs)
            {
                yield return process.Delay((long)sub.Distribution.NextDouble());
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [Properties.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
}
