using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Serialization;
using HelpClasses;


namespace EcoProIT.DataLayer.EcoSpold
{
    public class ImportEcoSpold: INotifyPropertyChanged
    {
        
        private static Dictionary<string, string> _activityNames;
        
        private static TaskFactory taskFactory = new TaskFactory();
        private static object  running = new object();
        private static string _connectString=null;
        private static int _filesLeft;
        public static ImportEcoSpold BindingObj = null;

        public ImportEcoSpold()
        {
            BindingObj = this;
        }

        public static Dictionary<string, string> ActivityNames 
        {
            get
            {
                lock (running)
                {
                    return _activityNames;
                }
            }
        }

        public int FilesProcessed
        {
            get { return _filesLeft; }
            set
            {
                _filesLeft = value;
                OnPropertyChanged();
                OnPropertyChanged("Runing");
            }
        }

        public int FilesToBeProcessed
        {
            get { return _filesLeft; }
            set
            {
                _filesLeft = value;
                FilesProcessed = 0;
                OnPropertyChanged();
                OnPropertyChanged("Runing");
            }
        }

        public static string DataDir
        {
            get
            {
                if (_connectString == null)
                {
                    _connectString = "";
                    try
                    {
                        _connectString += ApplicationDeployment.CurrentDeployment.DataDirectory;

                    }
                    catch
                    {
                        _connectString += Environment.CurrentDirectory + "\\xml";
                    }
                    Directory.CreateDirectory(_connectString);
                }
                
                return _connectString;
            }
        }

        public Boolean Runing   
        {
            get { return FilesProcessed - FilesToBeProcessed!= 0; }
        }

        public static Dictionary<string,string> LoadNewLCIFiles(Stream[] fileStreams)
        {
            
            lock (running)
            {
                BindingObj.FilesToBeProcessed = fileStreams.Count();
               
                foreach (var fileInfo in fileStreams)
                {

                    TEcoSpold spold = null;
                    try
                    {
                        var m = new XmlSerializer(typeof(TEcoSpold));
                        spold = (TEcoSpold) m.Deserialize(fileInfo);
                    }
                    catch
                    {
                    }
                    if (spold != null)
                    {
                        var ecoitem = spold.Items.FirstOrDefault();
                        {
                            var referenceFlow = ecoitem.flowData.intermediateExchange.FirstOrDefault(t =>
                                   (t.Item is TIntermediateExchangeOutputGroup
                                       ? (TIntermediateExchangeOutputGroup)t.Item
                                       : TIntermediateExchangeOutputGroup.Item2)
                                   == TIntermediateExchangeOutputGroup.Item0);
                            if (referenceFlow != null)
                            {
                                var name = referenceFlow.name.FirstOrDefault().ToString();
                                
                                try
                                {
                                    if (_activityNames.ContainsKey(name))
                                        _activityNames[name] = ecoitem.activityDescription.activity.id;
                                    else
                                        _activityNames.Add(name, ecoitem.activityDescription.activity.id);

                                    //Copy to data file
                                    TActivityDataset ecoitem1 = ecoitem;
                                    taskFactory.StartNew(() =>
                                    {
                                        
                                        FileInfo file = new FileInfo(Path.Combine(
                                            DataDir,
                                            ecoitem1.activityDescription.activity
                                                .id + ".spold"));
                                        using (var filestream = file.Open(FileMode.Create))
                                        {
                                            var m = new XmlSerializer(typeof(TEcoSpold));
                                            m.Serialize(filestream, spold);
                                            filestream.Close();
                                        }
                                           
                                    });
                                }
                                catch { }
                            }
                            BindingObj.FilesProcessed++;
                        }
                    }
                    

                }
                
                return _activityNames;
            }
        }


        public async static void LoadLCINames()
        {
            
            var m = new XmlSerializer(typeof (TEcoSpold));
            var dir = new DirectoryInfo(DataDir);
            var tasks = new List<Task<Pair<string, string>>>();
            BindingObj.FilesToBeProcessed = dir.EnumerateFiles().Count();
            foreach (var fileInfo in dir.EnumerateFiles())
            {

                var info = fileInfo;
                var task = new Task<Pair<string, string>>(() =>
                {
                    TEcoSpold ecospold = null;
                    try
                    {
                        using (var f = info.OpenRead())
                        {
                            ecospold = (TEcoSpold) m.Deserialize(f);
                            f.Close();
                        }

                        if (ecospold != null)
                        {
                            var activity = ecospold.Items.FirstOrDefault();
                            if (activity != null)
                            {
                                var referenceFlow = activity.flowData.intermediateExchange.FirstOrDefault(t =>
                                    (t.Item is TIntermediateExchangeOutputGroup
                                        ? (TIntermediateExchangeOutputGroup) t.Item
                                        : TIntermediateExchangeOutputGroup.Item2)
                                    == TIntermediateExchangeOutputGroup.Item0);
                                if (referenceFlow != null)
                                {
                                    string name = referenceFlow.name.FirstOrDefault().ToString();
                                    BindingObj.FilesProcessed++;
                                    if (name != null)
                                        return new Pair<string, string>(name.ToString(),
                                            activity.activityDescription.activity.id);
                                }

                            }
                        }
                    }
                    catch
                    {
                        return null;
                    }
                    return null;
                });
                task.Start();
                tasks.Add(task);
            }
            ;
            _activityNames = new Dictionary<string, string>();
            var results = await Task.WhenAll(tasks);
            foreach (var result in results.Where(t=>t!= null))
            {
                
                if (!_activityNames.ContainsKey(result.Key))
                    _activityNames.Add(result.Key, result.Value);
            }
            
        }
        public async static Task<Consumable> LoadLCISet(string id)
        {
            var task = new Task<Consumable>(() =>
            {
                var m = new XmlSerializer(typeof (TEcoSpold));


                FileInfo file = new FileInfo(Path.Combine(DataDir, id + ".spold"));
                {
                    TEcoSpold ecoSpold = null;
                    try
                    {
                        using (var f = file.OpenRead())
                        {
                            ecoSpold = (TEcoSpold) m.Deserialize(f);
                            f.Close();
                            var activity = ecoSpold.Items.FirstOrDefault();
                            var referenceFlow = activity.flowData.intermediateExchange.FirstOrDefault(t =>
                                (t.Item is TIntermediateExchangeOutputGroup
                                    ? (TIntermediateExchangeOutputGroup) t.Item
                                    : TIntermediateExchangeOutputGroup.Item2)
                                == TIntermediateExchangeOutputGroup.Item0);
                            if (referenceFlow == null)
                                return null;
                            decimal convertvalue;
                            var consum = new Consumable()
                            {
                                Name = referenceFlow.name.First().ToString(),
                                ecospoldId = activity.activityDescription.activity.id,
                                PerUnit = GetUnit(referenceFlow.unitId, out convertvalue)
                            };

                            var lciset = new LciSet();
                            lciset.Emissions = new List<Emission>();
                            foreach (var flow in activity.flowData.elementaryExchange)
                            {
                                var e = new Emission();

                                e.SIUnit = GetUnit(flow.unitId, out convertvalue);
                                e.Value = (decimal) flow.amount*convertvalue;
                                e.EmissionName = flow.name.ToString();

                                lciset.Emissions.Add(e);
                            }
                            consum.LciSet = lciset;
                            return consum;
                        }
                    }
                    catch
                    {
                    }

                }
                return null;
            });
            task.Start();
            return await task;
        }

        private static SIUnits GetUnit(string unitid, out decimal convertvalue)
        {
            convertvalue = 1;
            switch (unitid)
            {
                case "f2aaa4ba-fb58-49a7-a552-e7f64366f379":
                    return SIUnits.m;
                case "e32b56ef-fa80-4487-9796-f3c1476c27b3":
                    return SIUnits.h;
                case "5b972631-34e3-4db7-a615-f6931770a0cb":
                    return SIUnits.Piece;
                case "86bbe475-8a8f-44d8-914c-e398787e7121":
                    return SIUnits.ha;
                case "1017f68a-f818-4f9d-a34d-a31e7386f628":
                    return SIUnits.m2;
                case "ae252091-811b-461b-8e89-8f3075639eb1":
                    return SIUnits.km;
                case "77ae64fa-7e74-4252-9c3b-889c1cd20bfc":
                    return SIUnits.kWh;
                case "487df68b-4994-4027-8fdc-a4dc298257b7":
                    return SIUnits.kg;
                case "2cf92850-0f92-4004-9dba-a6ceb6a414c2":
                    return SIUnits.year;
                case "980b811e-3905-4797-82a5-173f5568bc7e":
                    convertvalue = (decimal)(1/3.6);
                    return SIUnits.kWh;
                case "4923348e-591b-4772-b224-d19df86f04c9":
                    return SIUnits.kBq;
                case "de5b3c87-0e35-4fb0-9765-4f3ba34c99e5":
                    return SIUnits.m3;
                case "86b0e4a2-57e5-48b8-a43d-22c7f8490dca":
                    return SIUnits.L;
                case "9dbdef14-e661-41e7-a878-e1f380b24d5a":
                    return SIUnits.MW;
                default:
                    return SIUnits.Piece;

            }
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual  void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
}
