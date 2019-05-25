using System;
using System.Collections;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcoProIT.DataLayer;
using FeserWard.Controls;

namespace EcoProIT.UserControles.Models
{
    public class ConsumableELCDDatabaseSearch:IIntelliboxResultsProvider
    {
        Dictionary<string,Consumable> _db = new Dictionary<string,Consumable>();
        private Task updater; 
        public ConsumableELCDDatabaseSearch()
        {
            updater = new Task(() =>
            {

                var db = DatabaseConnection.GetModelContext();
                    var list = db.ConsumableBase.ToList();
                    var query = from i in list
                                select
                                    new Consumable()
                                        {
                                            LciSet = new LciSet() {Emissions = i.ConsumablesEmission.Select(t=>new Emission(){EmissionName = t.Emissions.Name +" "+t.Emissions.Unit, Value = new decimal(t.Value??0)}).ToList()},
                                            Name = i.Name,
                                            PerUnit = SIUnits.Kg
                                        };
                    _db = query.ToDictionary(t => t.Name, t=>t);
                });
            updater.Start();

        }

        public IEnumerable<object> DoSearch(string searchTerm, int maxResults, object extraInfo)
        {
            return _db.Where(t=>t.Key.Contains(searchTerm)).Select(t=>t.Value).Take(maxResults);
        }
    }
}
