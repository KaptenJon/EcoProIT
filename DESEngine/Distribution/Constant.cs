using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace React.Distribution
{
    public class Constant:Uniform
    {
        public double ConstantNumber { get; private set; }

        public Constant(double constant)
        {
            ConstantNumber = constant;
        }

        public override float NextSingle()
        {
            return (float)ConstantNumber;
        }

        public override double NextDouble()
        {
            return ConstantNumber;
        }

        public override int NextInteger()
        {
            return (int)ConstantNumber;
        }

        public override long NextLong()
        {
            return (long) ConstantNumber;
        }
        public override ulong Nextulong()
        {
            return (ulong)ConstantNumber;
        }
    }
}
