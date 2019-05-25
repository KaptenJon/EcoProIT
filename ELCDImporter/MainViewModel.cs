using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Xml.Serialization;
using EcoProIT.DataLayer;
using GalaSoft.MvvmLight;
using GalaSoft;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;

namespace ELCDImporter
{
    class MainViewModel:ViewModelBase
    {
        private RelayCommand _import;
        private int _progress;
        private int _maxProgress;
        private ObservableCollection<ProcessDataSetType> _processesList = new ObservableCollection<ProcessDataSetType>();

        public RelayCommand Import
        {
            get { return _import ?? (_import = new RelayCommand(ImportELCD)); }
        }

        private readonly object _progressLock = new object();
        private object dblock = new object();

        public int Progress
        {
            get
            {
                lock(_progressLock)
                return _progress;
            }
            private set
            {
                lock (_progressLock)
                {
                    _progress = value;
                    RaisePropertyChanged("Progress");
                }
            }
        }

        public int MaxProgress
        {
            get { return _maxProgress; }
            private set { _maxProgress = value;
            RaisePropertyChanged("MaxProgress");}
        }
        public ObservableCollection<ProcessDataSetType> ProcessesList
        {
            get { return _processesList; }
            set { _processesList = value; }
        }

        private async void ImportELCD()
        {
            var folder = new FolderBrowserDialog();
            if (folder.ShowDialog() != DialogResult.OK)
                return;

            Progress = 0;
            var processes = new DirectoryInfo(folder.SelectedPath + "\\processes");
            if (!processes.Exists)
                return;
            var flowdic = new DirectoryInfo(folder.SelectedPath + "\\flows");
            if (!flowdic.Exists)
                return;
            MaxProgress = processes.EnumerateFiles("*.xml").Count()*3;
            ProcessesList.Clear();
            var processSerializer = new XmlSerializer(typeof (ProcessDataSetType));
            var flowSerializer = new XmlSerializer(typeof (FlowDataSetType));
            var dis = Dispatcher.CurrentDispatcher;
            var db = DatabaseConnection.GetModelContext();
            db.ConsumablesEmission.DeleteAllOnSubmit(db.ConsumablesEmission);
            db.SubmitChanges();
            db.ConsumableBase.DeleteAllOnSubmit(db.ConsumableBase);
            db.Emissions.DeleteAllOnSubmit(db.Emissions);
            db.SubmitChanges();
            var consumablesEmissionLocal = db.ConsumablesEmission.ToList();
            var emissionLocal = db.Emissions.ToList();
            var consumableLocal = db.ConsumableBase.ToList();
            int emissionIndex = emissionLocal.Any() ? emissionLocal.Max(t => t.Id) + 1 : 1;
            int consumableIndex = consumableLocal.Any() ? consumableLocal.Max(t => t.Id) + 1 : 1;
            
            var task = new Task(() =>
                {
                    
                   ConcurrentDictionary<string, FlowDataSetType> flows = new ConcurrentDictionary<string, FlowDataSetType>(); 
                    Parallel.ForEach(processes.EnumerateFiles("*.xml"), (file) =>
                        {
                            var deserializedProcess = ProcessDataSetType(file, processSerializer);
                            dis.Invoke(() =>
                            {
                                Progress += 1;
                                ProcessesList.Add(deserializedProcess);
                            });
                            foreach (var exchangeType in deserializedProcess.exchanges.exchange)
                            {
                                if (!flows.ContainsKey(exchangeType.referenceToFlowDataSet.refObjectId))
                                    flows.TryAdd(exchangeType.referenceToFlowDataSet.refObjectId,
                                                 FlowSetType(flowdic + exchangeType.referenceToFlowDataSet.uri.Remove(0,8),flowSerializer) );
                            }
                            

                        });
                    foreach (var flowDataSetType in flows)
                    {


                        Emissions emission = null;
                        emission = new Emissions()
                            {
                                Name =
                                    flowDataSetType.Value.flowInformation.dataSetInformation.name.baseName
                                                   .FirstOrDefault().Value,
                                Id = emissionIndex++,
                                Unit = flowDataSetType.Value.flowInformation.dataSetInformation.sumFormula
                            };
                        if (!emissionLocal.AsParallel().Any(t => t.Name == emission.Name))
                            emissionLocal.Add(emission);
                    }
                    foreach (var deserializedProcess in ProcessesList)
                    {
                        var name =
                            deserializedProcess.processInformation.dataSetInformation.name.baseName.FirstOrDefault();
                        string unit = "";
                        try
                        {
                            unit =
                                deserializedProcess.processInformation.dataSetInformation.name
                                                   .functionalUnitFlowProperties
                                                   .FirstOrDefault().Value;
                        }
                        catch
                        {
                        }
                        dis.Invoke(() =>
                            {
                                Progress += 1;
                            });
                        ConsumableBase cons = null;
                        
                        {
                            cons = consumableLocal.FirstOrDefault(t => name != null && t.Name == name.Value);
                            if (cons == null)
                            {

                                cons = new ConsumableBase()
                                    {
                                        Name = name == null ? "" : name.Value,
                                        Unit = unit,
                                        Id = consumableIndex++
                                    };
                                consumableLocal.Add(cons);
                            }
                        }
                        dis.Invoke(() =>
                            {
                                Progress += 1;
                            });
                        foreach (var exchange in deserializedProcess.exchanges.exchange)
                        {
                            var emissionN = exchange.referenceToFlowDataSet.shortDescription.FirstOrDefault();
                            if (emissionN != null)
                            {
                                var emissionName = emissionN.Value;

                                Emissions emission = emissionLocal.AsParallel().FirstOrDefault(t=>t.Name == emissionName);
                                if (emission == default(Emissions))
                                {
                                    emission = new Emissions()
                                        {
                                            Name = emissionName,
                                            Id = emissionIndex++
                                        };
                                    emissionLocal.Add(emission);

                                 }
                                if (
                                    !consumablesEmissionLocal.AsParallel().Any(
                                        t =>
                                        t.Consumable == cons.Id &&
                                        t.Emission == emission.Id))
                                    consumablesEmissionLocal.Add(new ConsumablesEmission()
                                        {
                                            Consumable = cons.Id,
                                            Emission = emission.Id,
                                            Value = exchange.resultingAmount
                                        });


                            }
                        }
                    }
                });
            task.Start();
            await task;
            db.ConsumablesEmission.DeleteAllOnSubmit(db.ConsumablesEmission);
            db.SubmitChanges();
            db.ConsumableBase.DeleteAllOnSubmit(db.ConsumableBase);
            db.Emissions.DeleteAllOnSubmit(db.Emissions);
            db.SubmitChanges();
            db.ConsumableBase.InsertAllOnSubmit(consumableLocal);
            db.Emissions.InsertAllOnSubmit(emissionLocal);
            db.SubmitChanges();
            db.ConsumablesEmission.InsertAllOnSubmit(consumablesEmissionLocal);
            db.SubmitChanges();
        }

        private FlowDataSetType FlowSetType(string file, XmlSerializer serializer)
        {
            var f = new FileInfo(file);
            if (!f.Exists)
                return null;
            var stream = f.OpenText();
            var deserializedProcess = (FlowDataSetType)serializer.Deserialize(stream);
            stream.Close();
            return deserializedProcess;
        }

        private static ProcessDataSetType ProcessDataSetType(FileInfo file, XmlSerializer serializer)
        {
            var f = file;
            var stream = f.OpenText();
            var deserializedProcess = (ProcessDataSetType) serializer.Deserialize(stream);
            stream.Close();
            return deserializedProcess;
        }
    }
}
