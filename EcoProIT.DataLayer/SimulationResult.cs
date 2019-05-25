using System;

namespace EcoProIT.DataLayer
{
    public class SimulationResult
    {
        public ulong Start { get; set; }
        public ulong Total { get; set; }
        public ulong End { get { return Start + Total; } }
    }
}