using System;
using System.Collections.Generic;
using EcoProIT.DataLayer;
using EcoProIT.UserControles.ViewModel;
using HelpClasses;
using React;


namespace EcoProIT.UI.SimulationModel
{
    public class DESProduct : Process
    {
        private List<Pair<DESMachine, Job>> jobsPairs;
        private static ulong producedProducts = 0;


        public DESProduct(Simulation simulationModel)
            : base(simulationModel)
        {
            ProductID = producedProducts++;
        }



        public DESProduct(Simulation simulationModel, string productName, List<Pair<DESMachine, Job>> jobsPairs):this(simulationModel)
        {
           
            this.jobsPairs = jobsPairs;
            Name = productName;
        }

        public ulong ProductID { get; private set; }
        protected override IEnumerator<DesTask> GetProcessSteps()
        {
            DESMachine prev = null;

            for (int i = 0; i < jobsPairs.Count; i++)
            {
                var jobpair = jobsPairs[i];

                while (!jobpair.Key.Machine.IsOwner(this))
                    yield return jobpair.Key.Machine.Acquire(this);


                if (prev != null && prev != jobpair.Key)
                    yield return prev.Machine.Release(this);

                var n = jobsPairs.Count < i + 1 ? jobsPairs[i + 1].Key : null;
                //var job = jobpair.Key.GetProcessSteps(this, jobpair.Value, n);
                yield return ProcessJob(jobpair.Key,this, jobpair.Value, n);
                prev = jobpair.Key;
            }
            
                yield return prev.Machine.Release(this);
            yield break;
        }
        public Process ProcessJob(DESMachine machine, DESProduct product, Job job, DESMachine nextDesMachine)
        {
            if (machine != null || job != null)
            {

                var p = new Process(Simulation, (process, o) => machine.GetProcessSteps(process, product, job, nextDesMachine));
                return p;
            }
            return null;

        }


    }
}
