namespace EcoProIT.DataLayer
{
    using System;

    public  class MachineState:SimulationResult,IComparable<MachineState> 
    {
        public string Machine { get; set; }
        public string State { get; set; }
        public int CompareTo(MachineState other)
        {
            if (Machine.Equals(other.Machine) && State.Equals(other.State) && Start.Equals(other.Start))
                return 0;
            var i = System.String.Compare(Machine, other.Machine, System.StringComparison.Ordinal);
            if (i != 0)
                return i;
            i = System.String.Compare(State, other.State, System.StringComparison.Ordinal);
            if (i != 0)
                return i;
            return (int) (Start - other.Start);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MachineState;
            if (other != null)
                return Machine.Equals(other.Machine) && State.Equals(other.State) && Start.Equals(other.Start);
            return false;
        }

        public override int GetHashCode()
        {
            return (Machine + State + Start).GetHashCode();
        }

        public override string ToString()
        {
            return Machine + " " + State + " " + Start + " " + Total;
        }
    }
}
