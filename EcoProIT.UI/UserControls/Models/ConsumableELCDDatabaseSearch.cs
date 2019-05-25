using System.Collections.Generic;
using System.Linq;
using EcoProIT.UI.DataLayer.EcoSpold;
using FeserWard.Controls;

namespace EcoProIT.UI.UserControls.Models
{
    public class ConsumableELCDDatabaseSearch:IIntelliboxResultsProvider
    {
        Dictionary<string,string> _db = new Dictionary<string,string>();
        public ConsumableELCDDatabaseSearch()
        {
             /*
            if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
                return;
            updater = new Task(() =>
            {
                _db = ImportEcoSpold.ActivityNames;
                var db = DatabaseConnection.GetModelContext();
                    var list = db.ConsumableBase.ToList();
                    var query = from i in list
                                select
                                    new Consumable()
                                        {
                                            LciSet = new LciSet() {Emissions = i.ConsumablesEmission.Select(t=>new Emission(){EmissionName = t.Emissions.Name +" "+t.Emissions.Unit, Value = new decimal(t.Value??0)}).ToList()},
                                            Name = i.Name,
                                            PerUnit = SIUnits.kg
                                        };
                    _db = query.ToDictionary(t => t.Name, t=>t);
            });
            updater.Start();
            */
        }

        public IEnumerable<object> DoSearch(string searchTerm, int maxResults, object extraInfo)
        {
            if (ImportEcoSpold.ActivityNames != null)
                return ImportEcoSpold.ActivityNames.Where(t => t.Key.Contains(searchTerm)).Select(t => new SearchPair(t.Key,t.Value)).Take(maxResults);
            return null;
        }

        public class SearchPair
        {
            public SearchPair(string v, string id)
            {
                Visual = v;
                this.Id = id;
            }
            public string Visual { get; set; }
            public string Id { get; set; }
            public override string ToString()
            {
                return Visual;
            }
        }
    }
}
