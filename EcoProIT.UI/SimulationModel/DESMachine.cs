using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using EcoProIT.DataLayer;
using EcoProIT.DataLayer.Annotations;
using EcoProIT.UserControles;
using EcoProIT.UserControles.ViewModel;
using HelpClasses;
using React;
using React.Distribution;
using React.Tasking;
using Process = React.Process;

namespace EcoProIT.UI.SimulationModel
{
    [System.Runtime.InteropServices.GuidAttribute ("0017EE8A-518B-4AD3-82F5-73595EE2119F")]
    public class DESMachine:Process, INotifyPropertyChanged
    {
        private IRandom breakdown;
        private IRandom repair;
        private DESResource _machine = DESResource.Create(1);
        private DESMachine parentDES = null;
        private int _nrofProductsProcessed;
    
        private HashSet<DESMachine> parents = new HashSet<DESMachine>();
        
        public DESMachine(SimulationModel sim, ModelNode involvednode, DESMachine parentNode) : base(sim)
        {
            Name = involvednode.ResourceModel.ProcessName;
            if (involvednode.ResourceModel.HasBreakdown)
            {
                breakdown = involvednode.ResourceModel.MTBF.Distribution.DESRandom;
                repair = involvednode.ResourceModel.MTTR.Distribution.DESRandom;
                GetBreakdown(sim).Activate(sim);
            }
            _machine = DESResource.Create(involvednode.ResourceModel.Capacity);
            parentDES = parentNode;
            DESMachine p = parentNode;
            while (p != null)
            {
                parents.Add(p);
                p = p.parentDES;
            }
            Machine.AllowOwnMany = false;
            currentstatus.Add("Idle",0);
        }

        public Process GetBreakdown(Simulation sim)
        {
            return new Process(sim, Breakdown);
        }

        protected override IEnumerator<DesTask> GetProcessSteps()
        {
            return base.GetProcessSteps();
        }

        public IEnumerator<DesTask> GetProcessSteps(Process process, DESProduct product, Job job, DESMachine nextMachine)
        {
            DESMachine p = parentDES;
            while (p!=null)
            {
                while (!p.Machine.IsOwner(process))
                {
                    yield return p.Machine.Acquire(process);
                }
                p.RegisterAcquire(product);
                var t = p as DESTransport;
                if (t != null)
                    yield return t.FirstTransport(process);
                p = p.parentDES;
            }

            if (!SetState(product.ProductID + "", product.ProductID + ""))
                Debug.Assert(true, "Foulty");
            ulong time = Simulation.Now;
            foreach (var subjob in job.Subjobs)
            {
                var d = subjob.Distribution.Nextulong();
                yield return new Delay(Simulation,d);
                
                
            }
            var tp = parentDES as DESTransport;
            if (tp != null)
                tp.TransportProduct(process, Name);
            //HALT here if brokendown!!!
            var checkdowntime = time;
            while (currentstatus[product.ProductID + ""] > checkdowntime)
            {
                checkdowntime = currentstatus[product.ProductID + ""];
                yield return process.Delay(checkdowntime - time);
            }

            
            (Simulation as SimulationModel).AddProductEvent(new ProductResult()
            {
                ProductType = product.Name,
                Start = time,
                Productid = product.ProductID,
                ModelNode = Name,
                Total = Simulation.Now - time
            });
            
            NrofProductsProcessed++;
            SetState(product.ProductID+"","Idle");
            p = parentDES;
            DESMachine next = nextMachine;

            while (p != null && (next ==null || !next.parents.Contains(p)))
            {
                while (p.Machine.IsOwner(process))
                {
                    yield return p.Machine.Release(process);
                }
                p.RegistrerRelease(product);
                p = p.parentDES;
            }
            yield break;
        }

        private Dictionary<DESProduct, ulong> timeDictionary = new Dictionary<DESProduct, ulong>();

        private void RegistrerRelease(DESProduct product)
        {
            
            (Simulation as SimulationModel).AddProductEvent(new ProductResult()
            {
                ProductType = product.Name,
                Start = timeDictionary[product],
                Productid = product.ProductID,
                ModelNode = Name,
                Total = Simulation.Now - timeDictionary[product]
            });
            timeDictionary.Remove(product);
        }

        private void RegisterAcquire(DESProduct product)
        {
            timeDictionary.Add(product, Now);
        }

        private void RegisterMachineStateChange(ulong start, string state)
        {
            (Simulation as SimulationModel).AddMachineStateEvent(new MachineState()
            {
                State = state,
                Machine = Name,
                Start = start,
                Total = Now - start
            });
        }

        

        private Dictionary<string, ulong> currentstatus = new Dictionary<string, ulong>();
        private bool SetState(string setter,string name)
        {


            switch(name)
            {
                case "Off":
                    if(currentstatus.ContainsKey("Off"))
                        return true;
                    if (currentstatus.ContainsKey("Idle"))
                    {
                        RegisterMachineStateChange(currentstatus["Idle"], "Idle");
                        currentstatus.Remove("Idle");
                    }
                    else if (currentstatus.ContainsKey("Down"))
                    {
                        RegisterMachineStateChange(currentstatus["Down"], "Down");
                        if (setter == "Breakdown")
                            currentstatus.Remove("Down");
                    }
                    else if (currentstatus.ContainsKey(setter))
                        return false;
                    currentstatus.Add("Off",Now);
                    return true;

                    
                case "Idle":
                    if (currentstatus.ContainsKey("Idle"))
                        return true;
                    else if (currentstatus.ContainsKey("Off"))
                    {
                        RegisterMachineStateChange(currentstatus["Off"], "Off");
                        if (currentstatus.ContainsKey("Down"))
                            currentstatus["Down"] = currentstatus["Off"];
                        currentstatus.Remove("Off");
                    }
                    else if (currentstatus.ContainsKey("Down"))
                    {
                        RegisterMachineStateChange(currentstatus["Down"], "Down");
                        if (setter == "Breakdown")
                            currentstatus.Remove("Down");
                    }
                    else
                    {
                        //UsedTime += Simulation.Now - currentstatus[setter];
                        currentstatus.Remove(setter);
                    }
                    if(currentstatus.Count == 0)
                        currentstatus.Add("Idle", Now);
                    return true;

                 case "Down":
                    if (currentstatus.ContainsKey("Down"))
                        return true;
                    if (currentstatus.ContainsKey("Idle"))
                    {
                        RegisterMachineStateChange(currentstatus["Idle"], "Idle");
                        currentstatus.Remove("Idle");
                    }
                    currentstatus.Add("Down", Simulation.Now);
                    return true;
                default:
                    if (currentstatus.ContainsKey("Off"))
                        return false;
                    if(currentstatus.ContainsKey("Idle"))
                    {
                        RegisterMachineStateChange(currentstatus["Idle"], "Idle");
                        currentstatus.Remove("Idle");
                    }
                    if (currentstatus.ContainsKey("Down"))
                        return false;
                    currentstatus.Add(name, Simulation.Now);
                    return true;
            }       
        }

        public ulong UsedTime { get; set; }

        protected IEnumerator<DesTask> Breakdown(Process proc, object brd)
        {
            while (proc.Simulation.State == SimulationState.Running)
            {
                var d =  breakdown.Nextulong();
                yield return proc.Delay(d);
                _machine.OutOfService++;
                SetState("Breakdown","Down");
                d =  repair.Nextulong();
                
                var copy = currentstatus.Select(ti=> new Pair<string, ulong>(ti.Key,ti.Value) ).ToArray();
                        currentstatus.Clear();
                        foreach (var current in copy)
                        {
                            current.Value += d;
                            currentstatus.Add(current.Key,current.Value);
                        }
                
                yield return proc.Delay(d);
                SetState("Breakdown","Idle");
                _machine.OutOfService--;
            }
        }

        public DESResource Machine
        {
            get { return _machine; }
        }

        public int NrofProductsProcessed
        {
            private set
            {
                if (value == _nrofProductsProcessed) return;
                _nrofProductsProcessed = value;
                OnPropertyChanged();
            }
            get { return _nrofProductsProcessed; }
        }

        public Job[] Jobs { get; set; }

        public ulong IdleTime
        {
            get { return Simulation.Now - DownTime - UsedTime; }
        }
        public ulong DownTime { get; set; }

        //public Process ProcessJob(DESProduct product, Job job, DESMachine nextDesMachine)
        //{
        //    if (job != null)
        //    {
        //        NrofProductsProcessed++;
        //        return new Process(Simulation, (process, o) => GetProcessSteps(product, job, nextDesMachine));
        //    }

        //    Debug.Assert(job == null, "This should never happen...");
        //    return null;
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        [Properties.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}