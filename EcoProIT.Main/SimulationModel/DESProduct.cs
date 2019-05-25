using System.Collections.Generic;
using EcoProIT.UserControles.ViewModel;
using React;

namespace EcoProIT.Main.SimulationModel
{
    public class DESProduct : Process
    {
        private Simulation simulationModel;
        private List<SimulationMachine> steps;
        
        public DESProduct(Simulation simulationModel, List<SimulationMachine> steps)
            : base(simulationModel)
        {
            this.simulationModel = simulationModel;
            this.steps = steps;
        }

        protected override IEnumerator<DesTask> GetProcessSteps()
        {
            SimulationMachine prev = null;
            foreach (SimulationMachine machine in steps)
            {
                while(!machine.Machine.IsOwner(this))  
                    yield return machine.Machine.Acquire(this);
                
                if(prev!=null)
                    yield return prev.Machine.Release(this);
               
                yield return machine.ProcessJob(new Product());
                prev = machine;            
            }
            yield return prev.Machine.Release(this);
            yield break;
        }
    }
}
