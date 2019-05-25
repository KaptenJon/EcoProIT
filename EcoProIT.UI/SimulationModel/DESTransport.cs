using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EcoProIT.UserControles;
using React;
using React.Tasking;

namespace EcoProIT.UI.SimulationModel
{
    public class DESTransport:DESMachine
    {
        private decimal speed;
        private decimal startdistances;
        private Dictionary<string, decimal> distances = new Dictionary<string, decimal>();

        public DESTransport(SimulationModel sim, Transport involvednode, DESMachine parentNode) : base(sim, involvednode, parentNode)
        {
            speed = involvednode.ResourceModel.Speed;
            startdistances = involvednode.ResourceModel.DistansToNode;
            var list =
                involvednode.ResourceModel.DistanceBetweenChildrenNodesWithNodes.Select(
                    t => new KeyValuePair<string, decimal>(t.Key.Text, t.Value));
            foreach (var pair in list)
            {
                distances.Add(pair.Key,pair.Value);
            }
        }

        public DesTask FirstTransport(Process p)
        {
            return p.Delay((ulong)(1000 * (startdistances / speed)));
        }

        public DesTask TransportProduct(Process p, string machine)
        {
            return p.Delay((ulong)( 1000 * (distances[machine]/speed)));
        }
    }
}
