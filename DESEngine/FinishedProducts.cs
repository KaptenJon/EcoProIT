using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace React
{
    public class FinishedProducts : Condition
    {
        public FinishedProducts(string Name):base(Name)
        {
            waitForMoreProducts = new Condition(Name+"wait");
        }
        private decimal consumabel = 0;
        private Condition waitForMoreProducts;
        [BlockingMethod]
        public DesTask Consume(DesTask task, decimal consumption)
        {

            while (!Consume(ref consumption))
            {
                return waitForMoreProducts.Block(task);
            }
            return task;
        }

        private bool Consume(ref decimal consumption)
        {
            for (; consumption > 1; consumption--)
            {
                consumabel--;
                Signal();
            }
            if (consumption > consumabel)
            {
                consumption -= consumabel;
                consumabel = 0;
                return false;
            }
            else
            {
                consumabel -= consumption;
                consumption = 0;
                return true;
            }
        }

        [BlockingMethod]
        public DesTask Fill(DesTask task)
        {
            consumabel++;
            waitForMoreProducts.Signal();
            return Block(task);
        }
    }
}
