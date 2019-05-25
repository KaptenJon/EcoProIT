using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using EcoProIT.DataLayer;
using EcoProIT.UserControles;
using EcoProIT.UserControles.ViewModel;
using HelpClasses;
using Microsoft.Win32;

namespace EcoProIT.Main
{
    class Save
    {
        
        public  static  Task<bool> SaveNodes(IEnumerable<ModelNode> nodes, SaveFileDialog save, IEnumerable<Product> products)
        {
            Task<bool> t = new Task<bool>(() =>
                {
                    try
                    {
                        var resources = new List<Resource>();
                        var jobs = new List<CMSDDocumentDataSectionJob>();
                        foreach (ModelNode modelNode in nodes)
                        {
                            var res = HarvestModelNode(modelNode);
                            resources.Add(res);
                            jobs.AddRange(HarvestJobs(modelNode, res));
                        }
                        var parts = new List<PartType>();
                        foreach (var product in products)
                        {
                            parts.Add(HarvestProduct(product));
                        }

                        var defs = new CMSDDocument
                            {
                                DataSection =
                                    {
                                        PartTypes = parts.ToArray(),
                                        Resource = resources.ToArray(),
                                        Job = jobs.ToArray()
                                    }
                            };
                        var stream = File.Open(save.FileName, FileMode.Create);
                        var bformatter = new BinaryFormatter();
                        bformatter.Serialize(stream, defs);
                        stream.Close();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                });
            t.Start();
            return t;
        }

        private static PartType HarvestProduct(Product product)
        {
            PartType part = null;
            product.Dispatcher.Invoke((Action) (() =>
                {
                    part = new PartType()
                        {
                            ProductName = product.ProductName,
                            ResourceJob =
                                product.Nodes.Select(
                                    t =>
                                    new ResourceJob() { Resource = t.Key.ResourceModel.ProcessName, Job = t.Value.Name })
                                       .ToArray(),
                            Color = new byte[] {product.Color.R, product.Color.G, product.Color.B},
                            
                        };
                }));
            return part;
        }

        private static Resource HarvestModelNode(ModelNode modelNode)
        {
            ResourceProperty x=null, y=null;
            string resType = null, id = null;
            modelNode.Dispatcher.Invoke((Action)(() =>
            {
                x = new ResourceProperty() { Name = "XGrid", Value = modelNode.XGrid + "", Unit = "px" };
                y = new ResourceProperty() { Name = "YGrid", Value = modelNode.YGrid +"",Unit = "px"};
                resType = (modelNode as object).GetType().ToString();
                id = modelNode.ResourceModel.ProcessName;
            }));
            var properties = new List<ResourceProperty>() {x, y};
            if (modelNode.ResourceModel.MTBF.Distribution != null)
                properties.Add(new ResourceProperty()
                    {
                        Distribution = new []{modelNode.ResourceModel.MTBF.Distribution.CMSDDistribution},
                        Name = "MTBF",
                        Unit = modelNode.ResourceModel.MTBF.Distribution.GeneralConverter.UnitType.ToString()
                    });
            if (modelNode.ResourceModel.MTTR.Distribution != null)
                properties.Add(new ResourceProperty()
                    {
                        Distribution = new []{modelNode.ResourceModel.MTTR.Distribution.CMSDDistribution},
                        Name = "MTTR",
                        Unit = modelNode.ResourceModel.MTTR.Distribution.GeneralConverter.UnitType.ToString()
                    });
            if(modelNode.ParentNode != null)
                properties.Add(new ResourceProperty(){Name = "ParentNode",Value = modelNode.ParentNode.ResourceModel.ProcessName});
            if (modelNode.ResourceModel.IsTransport)
            {
                ResourceDefinitionModel.HarvestDistances(modelNode.ResourceModel, properties);
            }

            properties.Add(new ResourceProperty(){Name = "Capacity",Value = modelNode.ResourceModel.Capacity +""});

            if(modelNode.ResourceModel.Idle != null)
                foreach (var consumption in modelNode.ResourceModel.Idle.Consumptions)
                    HarvestConsumption(properties, consumption, "Idle");
            if (modelNode.ResourceModel.Off != null)
                foreach (var consumption in modelNode.ResourceModel.Off.Consumptions)
                    HarvestConsumption(properties, consumption, "Off");  
            if (modelNode.ResourceModel.Down != null)
                foreach (var consumption in modelNode.ResourceModel.Down.Consumptions)
                    HarvestConsumption(properties, consumption, "Down");  
            foreach (var declaredJob in modelNode.DeclaredJobs)
                foreach (var consumption in declaredJob.State.Consumptions)
                    HarvestConsumption(properties, consumption, declaredJob.Name);
            return new Resource()
                {
                    Name = id,
                    Identifier = id,
                    ResourceType = resType,
                    Property = properties.ToArray()
                    
                };
        }

        private static void HarvestConsumption(List<ResourceProperty> properties, Consumption consumption, string state)
        {
            properties.Add(new ResourceProperty()
                {
                    Name =
                        state + ":" + consumption.Consumable.Name + ":" + (consumption.AllocationPerTime ? "PerTime:" : "PerUsed:") +
                        (consumption.Static ? "OneTime:" : "TimeDependent"),
                    Value = consumption.Amount.ToString(CultureInfo.InvariantCulture),
                    Unit = consumption.UnitType
                });
        }

        private static void AddConsumption(IEnumerable<ResourceProperty> properties, State state)
        {
            if(properties == null)
                return;
            foreach (var property in properties)
            {
                var split = property.Name.Split(':');
                if (split.Count() > 1)
                {
                    var consumable = ConnectLCIDB.GetFromName(property.Name.Split(':')[1]);
                    state.Consumptions.Add(new Consumption()
                        {
                            AllocationPerTime = property.Name.Contains("PerTime"),
                            Static = property.Name.Contains("OneTime"),
                            Amount = decimal.Parse(property.Value, CultureInfo.InvariantCulture),
                            Consumable = consumable
                        });
                }
            }

        }

        public static List<CMSDDocumentDataSectionJob> HarvestJobs(ModelNode node, Resource res)
        {
            var list = new List<CMSDDocumentDataSectionJob>();
            foreach (Job declaredJob in node.DeclaredJobs)
            {
                var subjobs = new List<CMSDDocumentDataSectionJob>();
                int i = 1;
                foreach (SubJob subjob in declaredJob.Subjobs)
                {

                    subjobs.Add(new CMSDDocumentDataSectionJob()
                        {
                            PlannedEffort =
                                new CMSDDocumentDataSectionJobPlannedEffort()
                                    {
                                        PartType =
                                            node.ResourceModel.RelatedProducts.Select(
                                                t => new CMSDDocumentDataSectionJobPlannedEffortPartType()
                                                    {
                                                        PartTypeIdentifier = t.ProductName
                                                    }).ToArray(),
                                        ProcessingTime = new CMSDDocumentDataSectionJobPlannedEffortProcessingTime()
                                            {
                                                TimeUnit = subjob.Distribution.GeneralConverter.UnitType.ToString(),
                                                Distribution = subjob.Distribution.CMSDDistribution
                                            }
                                    },
                            Description = subjob.Description,
                            Identifier = res.Identifier + declaredJob.Name + i++
                        });
                }
                list.AddRange(subjobs);
                
                list.Add(new CMSDDocumentDataSectionJob()
                    {
                        SubJob = subjobs.Select(t => new CMSDDocumentDataSectionJobSubJob()
                            {
                                JobIdentifier = t.Identifier
                            }).ToArray(),
                        Identifier = "BaseJob" + res.Identifier + declaredJob.Name,
                        PlannedEffort = new CMSDDocumentDataSectionJobPlannedEffort()
                            {
                                PartType =
                                    node.ResourceModel.RelatedProducts.Select(
                                        t => new CMSDDocumentDataSectionJobPlannedEffortPartType()
                                            {
                                                PartTypeIdentifier = t.ProductName
                                            }).ToArray(),
                                ResourcesRequired = new Resource[] {res},
                            }
                    });
            }
            return list;
        }

        public static IEnumerable<Product> LoadProducts(CMSDDocument doc, IEnumerable<ModelNode> nodes )
        {
            if (nodes == null || doc == null )
                return null;
            var products = new ObservableCollection<Product>();
            var modelNodes = nodes as IList<ModelNode> ?? nodes.ToList();
          
            foreach (var partType in doc.DataSection.PartTypes)
            {

                var jobstodo = new ObservableCollection<Pair<ModelNode, Job>>();
                foreach (var p in partType.ResourceJob)
                {
                   var first = modelNodes.FirstOrDefault(t => t.ResourceModel.ProcessName == p.Resource);

                    if (first != null)
                    {
                        var first1 = first.ResourceModel.Job.FirstOrDefault(t => t.Name == p.Job);
                        if (first1 != null)
                        {
                            jobstodo.Add(new Pair<ModelNode, Job>(first, first1));
                     }
                    }
                }
                var product = new Product(jobstodo)
                        {
                            ProductName = partType.ProductName,
                            Editmode = false
                        };
                if (partType.Color != null)
                    product.Color = Color.FromRgb(partType.Color[0], partType.Color[1], partType.Color[2]);

                product.IsChecked = true;
                products.Add(product);

                foreach (var pair in jobstodo.ToList())
                {
                    //product.AddNextMachine(pair.Key,pair.Value);
                    pair.Key.ResourceModel.RelatedProducts.Add(product);
                }
            }

            return products;
        }

        public static bool LoadModel(Stream load, List<ModelNode> nodes, List<Product> products)
        {
             
            var bformatter = new BinaryFormatter();
            var defs = bformatter.Deserialize(load) as CMSDDocument;
            load.Close();

            if (defs != null)
            {
                nodes.AddRange(defs.DataSection.Resource.Select(resource => LoadModelNode(resource, defs.DataSection.Job.Where(t => t.Identifier != null && (t.Identifier.StartsWith(resource.Identifier) || t.Identifier.StartsWith("BaseJob" + resource.Identifier))))));
                var jobs = new List<Job>();
                foreach (var modelNode in nodes)
                {
                    jobs.AddRange(modelNode.DeclaredJobs);
                }
                
                products.AddRange(LoadProducts(defs,nodes));
                foreach (var resource in defs.DataSection.Resource)
                {
                    AddParent(nodes, resource);
                }
                ResourceDefinitionModel.LoadDistances(nodes.Select(t=>t.ResourceModel), defs.DataSection.Resource);    
            }
            return true;
        }

        private static void AddParent(List<ModelNode> modelNodes, Resource resource)
        {
            var prop = resource.Property.FirstOrDefault(t => t.Name == "ParentNode");
            if (prop != null)
            {
                var child = modelNodes.FirstOrDefault(t => t.ResourceModel.ProcessName == resource.Name);
                var parent = modelNodes.FirstOrDefault(t => t.ResourceModel.ProcessName == prop.Value);
                if (child != null && parent != null)
                    parent.AddChild(child);
            }
        }

        private static ModelNode LoadModelNode(Resource resource, IEnumerable<CMSDDocumentDataSectionJob> CMSDjobs)
        {
            var modelnode = new ModelNode();
            if (resource.ResourceType.Contains("Machine"))
                modelnode = new Machine();
            else if(resource.ResourceType.Contains("Facility"))
                modelnode = new Facility();
            else if (resource.ResourceType.Contains("Buffer"))
                modelnode = new UserControles.Buffer();
            else if (resource.ResourceType.Contains("Transport"))
                modelnode = new Transport();

            double left = 0, top = 0;
            foreach (ResourceProperty resourceProperty in resource.Property)
            {
                
                switch (resourceProperty.Name)
                {
                    case "XGrid":
                        left = double.Parse(resourceProperty.Value);
                        break;

                    case "YGrid":
                        top = double.Parse(resourceProperty.Value);
                        break;

                    case "MTTR":
                        var distsource = resourceProperty.Distribution.First();
                        modelnode.ResourceModel.MTTR.Distribution = DataLayer.IDistribution.GetFromCmsd(distsource);
                        modelnode.ResourceModel.MTTR.Distribution.GeneralConverter = GeneralConverter.GetInstance(resourceProperty.Unit);
                        break;
                    case "MTBF":
                        distsource = resourceProperty.Distribution.First();
                        modelnode.ResourceModel.MTBF.Distribution = DataLayer.IDistribution.GetFromCmsd(distsource);
                        modelnode.ResourceModel.MTBF.Distribution.GeneralConverter = GeneralConverter.GetInstance(resourceProperty.Unit);
                        break;
                    case "Capacity":
                        try
                        {
                            modelnode.ResourceModel.Capacity = int.Parse(resourceProperty.Value);
                        }
                        catch{}
                        break;
                    case "Speed":
                        try
                        {
                            modelnode.ResourceModel.Speed = int.Parse(resourceProperty.Value, CultureInfo.InvariantCulture);
                        }
                        catch
                        {}
                        break;
                    case "DistanceToNode":
                        try
                        {
                            modelnode.ResourceModel.DistansToNode = int.Parse(resourceProperty.Value, CultureInfo.InvariantCulture);
                        }
                        catch
                        { }
                        break;

                }
            }

            modelnode.Margin = new Thickness(left, top, 0, 0);
            modelnode.XGrid = left;
            modelnode.YGrid = top;
            modelnode.ResourceModel.ProcessName = resource.Name;

            AddConsumption(resource.Property.Where(t => t.Name.StartsWith("Idle")), modelnode.ResourceModel.Idle);
            AddConsumption(resource.Property.Where(t => t.Name.StartsWith("Off")), modelnode.ResourceModel.Off);
            AddConsumption(resource.Property.Where(t => t.Name.StartsWith("Down")), modelnode.ResourceModel.Down);

            modelnode.ResourceModel.Job.Clear();
            var cmsdDocumentDataSectionJobs = CMSDjobs as IList<CMSDDocumentDataSectionJob> ?? CMSDjobs.ToList();
            foreach (var sectionJob in cmsdDocumentDataSectionJobs.Where(t => t.Identifier.StartsWith("BaseJob")))
            {
                var job = new Job()
                    {
                        Name = sectionJob.Identifier.Remove(0, 7).Replace(modelnode.ResourceModel.ProcessName, "")
                    };

                AddConsumption(resource.Property.Where(t => t.Name.StartsWith(job.Name)), job.State);
                job.Subjobs.Clear();
                foreach (var pointSubJob in sectionJob.SubJob)
                {
                    var cmsdSubJob = cmsdDocumentDataSectionJobs.FirstOrDefault(t => t.Identifier == pointSubJob.JobIdentifier);
                    if (cmsdSubJob != null)
                    {
                        var sub = new SubJob()
                            {
                                Description = cmsdSubJob.Description,
                                Distribution = IDistribution.GetFromCmsd(cmsdSubJob.PlannedEffort.ProcessingTime.Distribution)

                            };
                        sub.Distribution.GeneralConverter = GeneralConverter.TimeConverters.FirstOrDefault(
                                t => t.UnitType.ToString() == cmsdSubJob.PlannedEffort.ProcessingTime.TimeUnit);
                        job.Subjobs.Add(sub);
                    }
                }
                
                modelnode.ResourceModel.Job.Add(job);
            }
            return modelnode;
        }
    }
}
