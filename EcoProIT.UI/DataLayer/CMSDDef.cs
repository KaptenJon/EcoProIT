﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.261
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

namespace EcoProIT.UI.DataLayer
{ // 
// This source code was auto-generated by xsd, Version=4.0.30319.1.
// 


    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]
    public partial class Distribution {
    
        private string nameField;
    
        private DistributionDistributionParameter[] distributionParameterField;
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement("DistributionParameter", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DistributionDistributionParameter[] DistributionParameter {
            get {
                return this.distributionParameterField;
            }
            set {
                this.distributionParameterField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    public partial class DistributionDistributionParameter {
    
        private string nameField;
    
        private string valueField;
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]
    public partial class Resource
    {
    
        private string resourceIdentifierField;
    
        private string identifierField;
    
        private string nameField;
    
        private string resourceTypeField;
    
        private string resourceDescriptionField;
    
        private ResourceProperty[] propertyField;
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ResourceIdentifier {
            get {
                return this.resourceIdentifierField;
            }
            set {
                this.resourceIdentifierField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Identifier {
            get {
                return this.identifierField;
            }
            set {
                this.identifierField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ResourceType {
            get {
                return this.resourceTypeField;
            }
            set {
                this.resourceTypeField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ResourceDescription {
            get {
                return this.resourceDescriptionField;
            }
            set {
                this.resourceDescriptionField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement("Property", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ResourceProperty[] Property {
            get {
                return this.propertyField;
            }
            set {
                this.propertyField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    public partial class ResourceProperty {
    
        private string nameField;
    
        private string descriptionField;
    
        private string unitField;
    
        private string valueField;
    
        private Distribution[] distributionField;
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Description {
            get {
                return this.descriptionField;
            }
            set {
                this.descriptionField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Unit {
            get {
                return this.unitField;
            }
            set {
                this.unitField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement("Distribution")]
        public Distribution[] Distribution {
            get {
                return this.distributionField;
            }
            set {
                this.distributionField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]
    public partial class CMSDDocument {

        private CMSDDocumentDataSection dataSection = new CMSDDocumentDataSection();
    
        /// <remarks/>
        [XmlElement("DataSection", typeof(CMSDDocumentDataSection), Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public CMSDDocumentDataSection DataSection
        {
            get {
                return this.dataSection;
            }
            set {
                this.dataSection = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    public partial class CMSDDocumentDataSection {
    
        private CMSDDocumentDataSectionJob[] jobField;

        private Resource[] resourceField;

        private PartType[] partTypes;

        /// <remarks/>
        [XmlElement("Job", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public CMSDDocumentDataSectionJob[] Job {
            get {
                return this.jobField;
            }
            set {
                this.jobField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement("PartTypes")]
        public PartType[] PartTypes
        {
            get {
                return this.partTypes;
            }
            set {
                this.partTypes = value;
            }
        }
        /// <remarks/>
        [XmlElement("Resource")]
        public Resource[] Resource
        {
            get
            {
                return this.resourceField;
            }
            set
            {
                this.resourceField = value;
            }
        }

    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [XmlType(AnonymousType = true)]
    public partial class PartType
    {
        private ResourceJob[] resourceField;
        private string productName;

        /// <remarks/>
        [XmlElement("Resource")]
        public ResourceJob[] ResourceJob
        {
            get
            {
                return this.resourceField;
            }
            set
            {
                this.resourceField = value;
            }
        }

    
        /// <remarks/>
        [XmlAttribute("ProductName")]
        public string ProductName
        {
            get
            {
                return this.productName;
            }
            set
            {
                this.productName = value;
            }
        }
        [XmlAttribute("Color")]
        public byte[] Color { get; set; }
    }

    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [XmlType(AnonymousType = true)]
    public partial class ResourceJob
    {
        private string resource;
        private string job;

        /// <remarks/>
        [XmlElement("Resource")]
        public string Resource
        {
            get
            {
                return this.resource;
            }
            set
            {
                this.resource = value;
            }
        }


        /// <remarks/>
        [XmlAttribute("Job")]
        public string Job
        {
            get
            {
                return this.job;
            }
            set
            {
                this.job = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    public partial class CMSDDocumentDataSectionJob {
    
        private string identifierField;
    
        private string descriptionField;
    
        private CMSDDocumentDataSectionJobSubJob[] subJobField;
    
        private CMSDDocumentDataSectionJobPlannedEffort plannedEffortField;
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Identifier {
            get {
                return this.identifierField;
            }
            set {
                this.identifierField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Description {
            get {
                return this.descriptionField;
            }
            set {
                this.descriptionField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement("SubJob", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public CMSDDocumentDataSectionJobSubJob[] SubJob {
            get {
                return this.subJobField;
            }
            set {
                this.subJobField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement("PlannedEffort", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public CMSDDocumentDataSectionJobPlannedEffort PlannedEffort {
            get {
                return this.plannedEffortField;
            }
            set {
                this.plannedEffortField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    public partial class CMSDDocumentDataSectionJobSubJob {
    
        private string jobIdentifierField;
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string JobIdentifier {
            get {
                return this.jobIdentifierField;
            }
            set {
                this.jobIdentifierField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    public partial class CMSDDocumentDataSectionJobPlannedEffort {
    
        private CMSDDocumentDataSectionJobPlannedEffortProcessingTime processingTimeField;
    
        private CMSDDocumentDataSectionJobPlannedEffortPartType[] partTypeField;

        private Resource[] resourcesRequiredField;
    
        /// <remarks/>
        [XmlElement("ProcessingTime", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public CMSDDocumentDataSectionJobPlannedEffortProcessingTime ProcessingTime {
            get {
                return this.processingTimeField;
            }
            set {
                this.processingTimeField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement("PartType", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public CMSDDocumentDataSectionJobPlannedEffortPartType[] PartType {
            get {
                return this.partTypeField;
            }
            set {
                this.partTypeField = value;
            }
        }
    
        /// <remarks/>
        [XmlArray(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlArrayItem("Resource", typeof(Resource), IsNullable=false)]
        public Resource[] ResourcesRequired
        {
            get {
                return this.resourcesRequiredField;
            }
            set {
                this.resourcesRequiredField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    public partial class CMSDDocumentDataSectionJobPlannedEffortProcessingTime {
    
        private string timeUnitField;
    
        private Distribution distributionField;
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TimeUnit {
            get {
                return this.timeUnitField;
            }
            set {
                this.timeUnitField = value;
            }
        }
    
        /// <remarks/>
        [XmlElement("Distribution")]
        public Distribution Distribution {
            get {
                return this.distributionField;
            }
            set {
                this.distributionField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    public partial class CMSDDocumentDataSectionJobPlannedEffortPartType {
    
        private string partTypeIdentifierField;
    
        /// <remarks/>
        [XmlElement(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PartTypeIdentifier {
            get {
                return this.partTypeIdentifierField;
            }
            set {
                this.partTypeIdentifierField = value;
            }
        }
    }
}