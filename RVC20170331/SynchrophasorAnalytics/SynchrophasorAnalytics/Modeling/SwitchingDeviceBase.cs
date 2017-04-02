//******************************************************************************************************
//  SwitchingDeviceBase.cs
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
//       Generated original version of source code.
//  06/08/2014 - Kevin D. Jones
//       Updated XML inline documentation.
//  07/09/2014 - Kevin D. Jones
//       Added Guid
//
//******************************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SynchrophasorAnalytics.Modeling
{
    /// <summary>
    /// The base class of switching devices in an electric power network.
    /// </summary>
    public class SwitchingDeviceBase : ITwoTerminal, INetworkDescribable
    {
        #region [ Private Constants ]

        /// <summary>
        /// Default values
        /// </summary>
        private const int DEFAULT_INTERNAL_ID = 0;
        private const int DEFAULT_NUMBER = 0;
        private const string DEFAULT_NAME = "Undefined";
        private const string DEFAULT_DESCRIPTION = "Uundefined";

        #endregion

        #region [ Private Members ]

        /// <summary>
        /// INetworkDescribable fields
        /// </summary>
        private Guid m_uniqueId;
        private int m_internalID;
        private int m_number;
        private string m_name;
        private string m_description;
        private string m_measurementKey;

        private SwitchingDeviceNormalState m_normalState;
        private SwitchingDeviceActualState m_actualState;

        private bool m_inManualOverrideMode;

        /// <summary>
        /// ITwoTerminal fields
        /// </summary>
        private Node m_fromNode;
        private int m_fromNodeID;
        private Node m_toNode;
        private int m_toNodeID;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// A statistically unique identifier for the instance of the class.
        /// </summary>
        [XmlAttribute("Uid")]
        public Guid UniqueId
        {
            get
            {
                if (m_uniqueId == Guid.Empty)
                {
                    m_uniqueId = Guid.NewGuid();
                }
                return m_uniqueId;
            }
            set
            {
                m_uniqueId = value;
            }
        }

        /// <summary>
        /// An integer identifier for each <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/> which is intended to be unique among other objects of the same type.
        /// </summary>
        [XmlAttribute("ID")]
        public int InternalID
        {
            get 
            { 
                return m_internalID; 
            }
            set
            {
                m_internalID = value; 
            }
        }

        /// <summary>
        /// A descriptive integer for the instance of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/>. There are no restrictions on uniqueness. 
        /// </summary>
        [XmlAttribute("Number")]
        public int Number
        {
            get 
            { 
                return m_number; 
            }
            set 
            { 
                m_number = value; 
            }
        }

        /// <summary>
        /// A string acronym for the instance of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/>. Will always return 'SWDV'.
        /// </summary>
        [XmlAttribute("Acronym")]
        public string Acronym
        {
            get 
            { 
                return "SWDV"; 
            }
            set 
            { 
            }
        }

        /// <summary>
        /// The string name of the instance of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/>.
        /// </summary>
        [XmlAttribute("Name")]
        public string Name
        {
            get 
            { 
                return m_name; 
            }
            set 
            { 
                m_name = value; 
            }
        }

        /// <summary>
        /// A string description of the instance of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/>.
        /// </summary>
        [XmlAttribute("Description")]
        public string Description
        {
            get 
            { 
                return m_description; 
            }
            set 
            { 
                m_description = value; 
            }
        }

        /// <summary>
        /// The output measurement key for the status of the switchable device.
        /// </summary>
        [XmlAttribute("Key")]
        public string MeasurementKey
        {
            get
            {
                return m_measurementKey;
            }
            set
            {
                m_measurementKey = value;
            }
        }

        /// <summary>
        /// Gets the type of the object as a string.
        /// </summary>
        [XmlIgnore()]
        public string ElementType
        {
            get 
            { 
                return this.GetType().ToString(); 
            }
        }

        /// <summary>
        /// The <see cref="SynchrophasorAnalytics.Modeling.Node"/> at the originating end of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/>.
        /// </summary>
        [XmlIgnore()]
        public Node FromNode
        {
            get 
            { 
                return m_fromNode; 
            }
            set 
            { 
                m_fromNode = value;
                m_fromNodeID = value.InternalID;
            }
        }

        /// <summary>
        /// The <see cref="SynchrophasorAnalytics.Modeling.Node.InternalID"/> of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase.FromNode"/>.
        /// </summary>
        [XmlAttribute("FromNode")]
        public int FromNodeID
        {
            get
            {
                return m_fromNodeID;
            }
            set
            {
                m_fromNodeID = value;
            }
        }

        /// <summary>
        /// The <see cref="SynchrophasorAnalytics.Modeling.Node"/> at the terminating end of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/>.
        /// </summary>
        [XmlIgnore()]
        public Node ToNode
        {
            get 
            {
                return m_toNode; 
            }
            set 
            { 
                m_toNode = value;
                m_toNodeID = value.InternalID;
            }
        }

        /// <summary>
        /// The <see cref="SynchrophasorAnalytics.Modeling.Node.InternalID"/> of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase.ToNode"/>.
        /// </summary>
        [XmlAttribute("ToNode")]
        public int ToNodeID
        {
            get
            {
                return m_toNodeID;
            }
            set
            {
                m_toNodeID = value;
            }
        }

        /// <summary>
        /// The normal or default state of the switch. Either <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceNormalState.Open"/> or <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceNormalState.Closed"/>
        /// </summary>
        [XmlAttribute("Normally")]
        public SwitchingDeviceNormalState NormalState
        {
            get 
            { 
                return m_normalState; 
            }
            set 
            { 
                m_normalState = value; 
            }
        }

        /// <summary>
        /// The actual current state of the switch. Either <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceActualState.Open"/> or <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceActualState.Closed"/>.
        /// </summary>
        [XmlAttribute("Actually")]
        public SwitchingDeviceActualState ActualState
        {
            get
            {
                return m_actualState;
            }
            set
            {
                m_actualState = value;
            }
        }

        /// <summary>
        /// A boolean flag which represents whether the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/> has been put into <i>manual override</i>.
        /// </summary>
        [XmlAttribute("ManualOverride")]
        public bool InManualOverrideMode
        {
            get
            {
                return m_inManualOverrideMode;
            }
        }

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// A blank constructor with default values.
        /// </summary>
        public SwitchingDeviceBase()
            :this(DEFAULT_INTERNAL_ID, DEFAULT_NUMBER, DEFAULT_NAME, DEFAULT_DESCRIPTION, SwitchingDeviceNormalState.Open)
        {
        }

        /// <summary>
        /// A constructor for the <see cref="SynchrophasorAnalytics.Modeling.SeriesBranchBase"/> which takes as input the <see cref="SynchrophasorAnalytics.Modeling.INetworkDescribable"/> parameters and the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceNormalState"/> of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/>.
        /// </summary>
        /// <param name="internalID">An integer identifier for each <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/> which is intended to be unique among other objects of the same type.</param>
        /// <param name="number">A descriptive integer for the instance of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/>. There are no restrictions on uniqueness. </param>
        /// <param name="name">The string name of the instance of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/>.</param>
        /// <param name="description">A string description of the instance of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/>.</param>
        /// <param name="normalState">The normal or default state of the switch. Either <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceNormalState.Open"/> or <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceNormalState.Closed"/></param>
        public SwitchingDeviceBase(int internalID, int number, string name, string description, SwitchingDeviceNormalState normalState)
            :this(internalID, number, name, description,  normalState, 0, 0)
        {
        }

        /// <summary>
        /// A constructor for the <see cref="SynchrophasorAnalytics.Modeling.SeriesBranchBase"/> which takes as input the <see cref="SynchrophasorAnalytics.Modeling.INetworkDescribable"/> parameters and the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceNormalState"/> of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/> as well as the internal id of the from node and to node.
        /// </summary>
        /// <param name="internalID">An integer identifier for each <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/> which is intended to be unique among other objects of the same type.</param>
        /// <param name="number">A descriptive integer for the instance of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/>. There are no restrictions on uniqueness. </param>
        /// <param name="name">The string name of the instance of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/>.</param>
        /// <param name="description">A string description of the instance of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/>.</param>
        /// <param name="normalState">The normal or default state of the switch. Either <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceNormalState.Open"/> or <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceNormalState.Closed"/></param>
        /// <param name="fromNodeID">The <see cref="SynchrophasorAnalytics.Modeling.Node.InternalID"/> of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase.FromNode"/>.</param>
        /// <param name="toNodeID">The <see cref="SynchrophasorAnalytics.Modeling.Node.InternalID"/> of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase.ToNode"/>.</param>
        public SwitchingDeviceBase(int internalID, int number, string name, string description, SwitchingDeviceNormalState normalState, int fromNodeID, int toNodeID)
        {
            m_internalID = internalID;
            m_number = number;
            m_name = name;
            m_description = description;
            m_normalState = normalState;
            m_fromNodeID = fromNodeID;
            m_toNodeID = toNodeID;

            RemoveFromManualAndRevertToDefault();
        }

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Sets the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase.ActualState"/> to the desired state and puts the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/> into <i>manual override</i>. While in <i>manual override</i> the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/> will not update its state due to measurement update.
        /// </summary>
        /// <param name="switchingDeviceActualState">The desired <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceActualState"/>.</param>
        public void ManuallySwitchTo(SwitchingDeviceActualState switchingDeviceActualState)
        {
            m_inManualOverrideMode = true;
            m_actualState = switchingDeviceActualState;
        }

        /// <summary>
        /// Toggles the state of the switching device and puts the device into manual override.
        /// </summary>
        public void ManuallyToggleActualState()
        {
            m_inManualOverrideMode = true;
            if (m_actualState == SwitchingDeviceActualState.Closed)
            {
                m_actualState = SwitchingDeviceActualState.Open;
            }
            else if (m_actualState == SwitchingDeviceActualState.Open)
            {
                m_actualState = SwitchingDeviceActualState.Closed;
            }
        }

        /// <summary>
        /// Removes the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/> from <i>manual override</i> but preserves its current state until its updated through a command or by a measurement.
        /// </summary>
        public void RemoveFromManualAndPreserveStateUntilUpdated()
        {
            m_inManualOverrideMode = false;
        }

        /// <summary>
        /// Removes the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/> from <i>manual override</i> and reverts its state back to the default state defined by <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceNormalState"/>.
        /// </summary>
        public void RemoveFromManualAndRevertToDefault()
        {
            m_inManualOverrideMode = false;

            if (m_normalState == SwitchingDeviceNormalState.Closed)
            {
                m_actualState = SwitchingDeviceActualState.Closed;
            }
            else if (m_normalState == SwitchingDeviceNormalState.Open)
            {
                m_actualState = SwitchingDeviceActualState.Open;
            }
        }

        /// <summary>
        /// A descriptive string representation of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/> class instance. The format is <i>internalId,number,name,description,measurementKey,normalState,fromNodeInternalId,toNodeInternalId</i> and can be used for a rudimentary momento design pattern
        /// </summary>
        /// <returns>A descriptive string representation of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/> class instance.</returns>
        public override string ToString()
        {
            return m_internalID.ToString() + "," + m_number.ToString() + "," + m_name + "," + m_description + "," + m_measurementKey + "," + m_normalState.ToString() + "," + m_fromNodeID.ToString() + "," + m_toNodeID.ToString();
        }

        /// <summary>
        /// A verbose string representation of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/>  and can be used for detailed text based output via a console or a text file.
        /// </summary>
        /// <returns>A verbose string representation of the <see cref="SynchrophasorAnalytics.Modeling.SwitchingDeviceBase"/>.</returns>
        public string ToVerboseString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat("----- Switching Device Base ----------------------------------------------------");
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat("      InternalID: " + InternalID.ToString() + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("          Number: " + Number.ToString() + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("            Name: " + Name + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("     Description: " + Description + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("        Normally: " + NormalState.ToString() + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("        Actually: " + ActualState.ToString() + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("          Manual: " + InManualOverrideMode.ToString() + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("        FromNode: " + FromNode.ToString() + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("          ToNode: " + ToNode.ToString() + "{0}", Environment.NewLine);
            stringBuilder.AppendLine();
            return stringBuilder.ToString();
        }

        #endregion
    }
}
