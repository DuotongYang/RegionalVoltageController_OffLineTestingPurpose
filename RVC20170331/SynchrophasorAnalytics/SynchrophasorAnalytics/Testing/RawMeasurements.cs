﻿//******************************************************************************************************
//  PhasorMeasurement.cs
//
//  Copyright © 2013, Kevin D. Jones.  All Rights Reserved.
//
//  This file is licensed to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/01/2013 - Kevin D. Jones
//   <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18033
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
//   </auto-generated>
//
//******************************************************************************************************
using System;
using System.Xml.Serialization;
using System.IO;

// 
// This source code was auto-generated by xsd, Version=4.0.30319.17929.
// 

namespace SynchrophasorAnalytics.Testing
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class RawMeasurements
    {

        private RawMeasurementsMeasurement[] itemsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Measurement", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public RawMeasurementsMeasurement[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <summary>
        /// Deserializes the collection of raw measurement key-value pairs from the *.xml file.
        /// </summary>
        /// <param name="pathName">The path name to the file to be deserialized.</param>
        /// <returns>A <see cref="SynchrophasorAnalytics.Testing.RawMeasurements"/> object.</returns>
        public static RawMeasurements DeserializeFromXml(string pathName)
        {
            try
            {
                RawMeasurements snapshot = null;

                XmlSerializer deserializer = new XmlSerializer(typeof(RawMeasurements));

                StreamReader reader = new StreamReader(pathName);

                snapshot = (RawMeasurements)deserializer.Deserialize(reader);

                reader.Close();

                return snapshot;
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to Deserialize the Raw Measurements from the Snapshot File: " + exception.ToString());
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class RawMeasurementsMeasurement
    {

        private string keyField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }
}
