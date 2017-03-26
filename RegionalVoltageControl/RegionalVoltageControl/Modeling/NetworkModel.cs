//******************************************************************************************************
//  NetworkModel.cs
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
//  07/10/2013 - Kevin D. Jones
//       Fixed issue with substations modeled without either direct or indirect observability. Before 
//       would cause a matrix singularity which is wrong. Correct LSE should never have a singularity. 
//       The observability analysis now removes those nodes from the problem. 
//  07/20/2013 - Kevin D. Jones
//       Added InputMeasurementKeys property for validating input measurement keys in the LSE.
//       Added AcceptsEstimates and AcceptsMeasurements for measurement mapping optimizations and
//       InputMeasurementKeys property. Added optimization to measurement mapping in Insert() methods.
//  08/10/2013 - Kevin D. Jones
//       Added I2-I1 ratio measurement keys to output measurements property.
//  06/23/2014 - Kevin D. Jones
//       Added 'CurrentFlowPostProcessingSetting' property.
//       Added 'IncludedCurrentPhasors' property with getter method.
//
//******************************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SynchrophasorAnalytics.Measurements;
using System.Xml.Serialization;
using SynchrophasorAnalytics.Networks;
using SynchrophasorAnalytics.Matrices;
using SynchrophasorAnalytics.Calibration;

namespace SynchrophasorAnalytics.Modeling
{
    /// <summary>
    /// A class which encapsulates the network elements and their hierarchy.
    /// </summary>
    [Serializable()]
    public class NetworkModel : INetworkDescribable
    {
        #region [ Private Members ]

        #region [ INetworkDescribable fields ]

        private Guid m_uniqueId;
        private int m_internalId;
        private int m_number;
        private string m_acronym;
        private string m_name;
        private string m_description;

        #endregion

        private Dictionary<string, double> m_rawMeasurementKeyValuePairs;
        private PhaseSelection m_phaseSelection;
        private InputOutputSettings m_inputOutputSettings;
        private CurrentFlowPostProcessingSetting m_currentFlowPostProcessingSetting;
        private CurrentInjectionPostProcessingSetting m_currentInjectionPostProcessingSetting;

        /// <summary>
        /// Network Components
        /// </summary>
        private List<Company> m_companies;
        private List<Division> m_divisions;
        private List<Substation> m_substations;
        private List<TransmissionLine> m_transmissionLines;
        private List<Node> m_nodes;
        private List<Switch> m_switches;
        private List<CircuitBreaker> m_circuitBreakers;
        private List<ShuntCompensator> m_shuntCompensators;
        private List<SeriesCompensator> m_seriesCompensators;
        private List<LineSegment> m_lineSegments;
        private List<VoltageLevel> m_voltageLevels;
        private List<Transformer> m_transformers;
        private List<ObservedBus> m_observedBusses;
        private List<SwitchingDeviceBase> m_switchingDevices;
        private List<TapConfiguration> m_tapConfigurations;

        /// <summary>
        /// Network Measurements
        /// </summary>
        private List<VoltagePhasorGroup> m_voltages;
        private List<CurrentFlowPhasorGroup> m_currentFlows;
        private List<CurrentInjectionPhasorGroup> m_currentInjections;
        private List<BreakerStatus> m_breakerStatuses;
        private List<StatusWord> m_statusWords;
        private List<CurrentFlowPhasorGroup> m_activeCurrentFlows;
        private List<CurrentInjectionPhasorGroup> m_activeCurrentInjections;

        /// <summary>
        /// Parent
        /// </summary>
        private Network m_parentNetwork;

        #endregion

        #region [ Properties ]

        #region [ INetworkDescribable Properties ]

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
        /// An integer identifier for each <see cref="SynchrophasorAnalytics.Modeling.NetworkModel"/> which is intended to be unique among other objects of the same type.
        /// </summary>
        [XmlAttribute("InternalID")]
        public int InternalID
        {
            get
            {
                return m_internalId;
            }
            set
            {
                m_internalId = value;
            }
        }

        /// <summary>
        /// A descriptive integer for the instance of the <see cref="SynchrophasorAnalytics.Modeling.NetworkModel"/>. There are no restrictions on uniqueness. 
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
        /// A string acronym for the instance of the <see cref="SynchrophasorAnalytics.Modeling.NetworkModel"/>.
        /// </summary>
        [XmlAttribute("Acronym")]
        public string Acronym
        {
            get
            {
                return m_acronym;
            }
            set
            {
                m_acronym = value;
            }
        }

        /// <summary>
        /// The string name of the instance of the <see cref="SynchrophasorAnalytics.Modeling.NetworkModel"/>.
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
        /// A string description of the instance of the <see cref="SynchrophasorAnalytics.Modeling.NetworkModel"/>.
        /// </summary>
        [XmlAttribute("Decsription")]
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
        /// Gets the type of the object as a string
        /// </summary>
        [XmlIgnore()]
        public string ElementType
        {
            get 
            {
                return this.GetType().ToString();
            }
        }

        #endregion

        #region [ I/O ]

        /// <summary>
        /// A Dictionary with the raw measurements and other inputs paired with their respective measurement keys.
        /// </summary>
        [XmlIgnore()]
        public Dictionary<string, double> InputKeyValuePairs
        {
            get
            {
                return m_rawMeasurementKeyValuePairs;
            }
            set
            {
                m_rawMeasurementKeyValuePairs = value;
            }
        }

        /// <summary>
        /// A Dictionary with the raw estimates and other output values paired with their respective measurement keys.
        /// </summary>
        [XmlIgnore()]
        public Dictionary<string, double> OutputKeyValuePairs
        {
            get
            {
                Dictionary<string, double> rawEstimateKeyValuePairs = new Dictionary<string, double>();

                if (m_inputOutputSettings.ReturnsStateEstimate)
                {
                    AddVoltageEstimatesToOutput(rawEstimateKeyValuePairs);
                }

                if (m_inputOutputSettings.ReturnsCurrentFlow)
                {
                    AddCurrentFlowEstimatesToOutput(rawEstimateKeyValuePairs);
                }

                if (m_inputOutputSettings.ReturnsCurrentInjection)
                {
                    AddCurrentInjectionsToOutput(rawEstimateKeyValuePairs);
                }

                if (m_inputOutputSettings.ReturnsVoltageResiduals)
                {
                    AddVoltageMeasurementResidualsToOutput(rawEstimateKeyValuePairs);
                }

                if (m_inputOutputSettings.ReturnsCurrentResiduals)
                {
                    AddCurrentMeasurementResidualsToOutput(rawEstimateKeyValuePairs);
                }

                if (m_inputOutputSettings.ReturnsCircuitBreakerStatus)
                {
                    AddCircuitBreakerStatusesToOutput(rawEstimateKeyValuePairs);
                }

                if (m_inputOutputSettings.ReturnsSwitchStatus)
                {
                    AddSwitchStatusesToOutput(rawEstimateKeyValuePairs);
                }

                return rawEstimateKeyValuePairs;
            }
        }

        /// <summary>
        /// A list of input measurements keys that are expectd by the modeled network elements.
        /// </summary>
        [XmlIgnore()]
        public List<string> InputMeasurementKeys
        {
            get
            {
                return GetInputMeasurementKeys();
            }
        }

        /// <summary>
        /// A list of the output measurement keys the network elements will use for output values.
        /// </summary>
        [XmlIgnore()]
        public string OutputMeasurementKeys
        {
            get
            {
                string outputMeasurementKeys = "outputMeasurements={";
                List<string> rawEstimateKeys = OutputKeyValuePairs.Keys.ToList();

                foreach (string key in rawEstimateKeys)
                {
                    if (!key.Equals("Undefined"))
                    {
                        outputMeasurementKeys += key + ";";
                    }
                }
                outputMeasurementKeys += "}";

                return outputMeasurementKeys;
            }
        }

        #endregion

        #region [ Settings ]

        /// <summary>
        /// Determines whether to treat the network as a <see cref="PhaseSelection.PositiveSequence"/> approximation or as a full <see cref="PhaseSelection.ThreePhase"/> representation.
        /// </summary>
        [XmlAttribute("PhaseConfiguration")]
        public PhaseSelection PhaseConfiguration
        {
            get
            {
                return m_phaseSelection;
            }
            set
            {
                m_phaseSelection = value;
            }
        }

        /// <summary>
        /// Determines whether to compute estimated current flow for only branches with at least 1 measured value or based on node observability irrespective of breaker status fidelity.
        /// </summary>
        [XmlAttribute("CurrentFlowPostProcessingSetting")]
        public CurrentFlowPostProcessingSetting CurrentFlowPostProcessingSetting
        {
            get
            {
                return m_currentFlowPostProcessingSetting;
            }
            set
            {
                m_currentFlowPostProcessingSetting = value;
            }
        }

        /// <summary>
        /// Determines whether to compute estimated current injections for only shunts where the measured value is of sufficient quality or based on node observability irrespective of breaker status fidelity.
        /// </summary>
        [XmlAttribute("CurrentInjectionPostProcessingSetting")]
        public CurrentInjectionPostProcessingSetting CurrentInjectionPostProcessingSetting
        {
            get
            {
                return m_currentInjectionPostProcessingSetting;
            }
            set
            {
                m_currentInjectionPostProcessingSetting = value;
            }
        }

        /// <summary>
        /// A flag that determines whether the model accepts measured values during OnNewMeasurements()
        /// </summary>
        [XmlAttribute("AcceptsMeasurements")]
        public bool AcceptsMeasurements
        {
            get
            {
                return m_inputOutputSettings.AcceptsMeasurements;
            }
            set
            {
                m_inputOutputSettings.AcceptsMeasurements = value;
            }
        }

        /// <summary>
        /// A flag that determines whether the model accepts estimated values during OnNewMeasurements()
        /// </summary>
        [XmlAttribute("AcceptsEstimates")]
        public bool AcceptsEstimates
        {
            get
            {
                return m_inputOutputSettings.AcceptsEstimates;
            }
            set
            {
                m_inputOutputSettings.AcceptsEstimates = value;
            }
        }

        /// <summary>
        /// A flag that determines whether the model returns state estimated values
        /// </summary>
        [XmlAttribute("ReturnsStateEstimate")]
        public bool ReturnsStateEstimate
        {
            get
            {
                return m_inputOutputSettings.ReturnsStateEstimate;
            }
            set
            {
                m_inputOutputSettings.ReturnsStateEstimate = value;
            }
        }

        /// <summary>
        /// A flag that determines whether the model returns estimated current flow values
        /// </summary>
        [XmlAttribute("ReturnsCurrentFlow")]
        public bool ReturnsCurrentFlow
        {
            get
            {
                return m_inputOutputSettings.ReturnsCurrentFlow;
            }
            set
            {
                m_inputOutputSettings.ReturnsCurrentFlow = value;
            }
        }

        /// <summary>
        /// A flag that determines whether the model returns estimated current injection values
        /// </summary>
        [XmlAttribute("ReturnsCurrentInjection")]
        public bool ReturnsCurrentInjection
        {
            get
            {
                return m_inputOutputSettings.ReturnsCurrentInjection;
            }
            set
            {
                m_inputOutputSettings.ReturnsCurrentInjection = value;
            }
        }

        /// <summary>
        /// A flag that determines whether the model returns voltage measurement residual values
        /// </summary>
        [XmlAttribute("ReturnsVoltageResiduals")]
        public bool ReturnsVoltageResiduals
        {
            get
            {
                return m_inputOutputSettings.ReturnsVoltageResiduals;
            }
            set
            {
                m_inputOutputSettings.ReturnsVoltageResiduals = value;
            }
        }

        /// <summary>
        /// A flag that determines whether the model returns current measurement residual values
        /// </summary>
        [XmlAttribute("ReturnsCurrentResiduals")]
        public bool ReturnsCurrentResiduals
        {
            get
            {
                return m_inputOutputSettings.ReturnsCurrentResiduals;
            }
            set
            {
                m_inputOutputSettings.ReturnsCurrentResiduals = value;
            }
        }

        /// <summary>
        /// A flag that determines whether the model returns circuit breaker statuses
        /// </summary>
        [XmlAttribute("ReturnsCircuitBreakerStatus")]
        public bool ReturnsCircuitBreakerStatus
        {
            get
            {
                return m_inputOutputSettings.ReturnsCircuitBreakerStatus;
            }
            set
            {
                m_inputOutputSettings.ReturnsCircuitBreakerStatus = value;
            }
        }

        /// <summary>
        /// A flag that determines whether the model returns switch status values
        /// </summary>
        [XmlAttribute("ReturnsSwitchStatus")]
        public bool ReturnsSwitchStatus
        {
            get
            {
                return m_inputOutputSettings.ReturnsSwitchStatus;
            }
            set
            {
                m_inputOutputSettings.ReturnsSwitchStatus = value;
            }
        }

        /// <summary>
        /// A flag that determines whether the model returns tap position values
        /// </summary>
        [XmlAttribute("ReturnsTapPositions")]
        public bool ReturnsTapPositions
        {
            get
            {
                return m_inputOutputSettings.ReturnsTapPositions;
            }
            set
            {
                m_inputOutputSettings.ReturnsTapPositions = value;
            }
        }

        /// <summary>
        /// A flag that determines whether the model returns the status of the series compensators.
        /// </summary>
        [XmlAttribute("ReturnsSeriesCompensatorStatus")]
        public bool ReturnsSeriesCompensatorStatus
        {
            get
            {
                return m_inputOutputSettings.ReturnsSeriesCompensatorStatus;
            }
            set
            {
                m_inputOutputSettings.ReturnsSeriesCompensatorStatus = value;
            }
        }

        #endregion

        #region [ Network Components ]

        /// <summary>
        /// A list of <see cref="Company"/> objects in the <see cref="NetworkModel"/>
        /// </summary>
        [XmlArray("Companies")]
        public List<Company> Companies
        {
            get 
            { 
                return m_companies; 
            }
            set 
            { 
                m_companies = value; 
            }
        }

        /// <summary>
        /// A list of <see cref="SynchrophasorAnalytics.Modeling.Division"/> objects in the <see cref="SynchrophasorAnalytics.Modeling.NetworkModel"/>.
        /// </summary>
        [XmlIgnore()]
        public List<Division> Divisions
        {
            get
            {
                return m_divisions;
            }
            set
            {
                m_divisions = value;
            }
        }

        /// <summary>
        /// A list of <see cref="SynchrophasorAnalytics.Modeling.Substation"/> objects in the <see cref="SynchrophasorAnalytics.Modeling.NetworkModel"/>.
        /// </summary>
        [XmlIgnore()]
        public List<Substation> Substations
        {
            get
            {
                return m_substations;
            }
            set
            {
                m_substations = value;
            }
        }

        /// <summary>
        /// A list of <see cref="SynchrophasorAnalytics.Modeling.TransmissionLine"/> objects in the <see cref="SynchrophasorAnalytics.Modeling.NetworkModel"/>.
        /// </summary>
        [XmlIgnore()]
        public List<TransmissionLine> TransmissionLines
        {
            get
            {
                return m_transmissionLines;
            }
            set
            {
                m_transmissionLines = value;
            }
        }

        /// <summary>
        /// A list of <see cref="SynchrophasorAnalytics.Modeling.Node"/> objects in the <see cref="SynchrophasorAnalytics.Modeling.NetworkModel"/>.
        /// </summary>
        [XmlIgnore()]
        public List<Node> Nodes
        {
            get
            {
                return m_nodes;
            }
            set
            {
                m_nodes = value;
            }
        }

        /// <summary>
        /// A list of <see cref="SynchrophasorAnalytics.Modeling.Switch"/> objects in the <see cref="SynchrophasorAnalytics.Modeling.NetworkModel"/>.
        /// </summary>
        [XmlIgnore()]
        public List<Switch> Switches
        {
            get
            {
                return m_switches;
            }
            set
            {
                m_switches = value;
            }
        }

        /// <summary>
        /// A list of <see cref="SynchrophasorAnalytics.Modeling.CircuitBreaker"/> objects in the <see cref="SynchrophasorAnalytics.Modeling.NetworkModel"/>.
        /// </summary>
        [XmlIgnore()]
        public List<CircuitBreaker> CircuitBreakers
        {
            get
            {
                return m_circuitBreakers;
            }
            set
            {
                m_circuitBreakers = value;
            }
        }

        /// <summary>
        /// A list of <see cref="SynchrophasorAnalytics.Modeling.Transformer"/> objects in the <see cref="SynchrophasorAnalytics.Modeling.NetworkModel"/>.
        /// </summary>
        [XmlIgnore()]
        public List<Transformer> Transformers
        {
            get
            {
                return m_transformers;
            }
            set
            {
                m_transformers = value;
            }
        }

        /// <summary>
        /// A list of <see cref="SynchrophasorAnalytics.Modeling.ShuntCompensator"/> objects in the <see cref="SynchrophasorAnalytics.Modeling.NetworkModel"/>.
        /// </summary>
        [XmlIgnore()]
        public List<ShuntCompensator> ShuntCompensators
        {
            get
            {
                return m_shuntCompensators;
            }
            set
            {
                m_shuntCompensators = value;
            }
        }

        /// <summary>
        /// A list of <see cref="SynchrophasorAnalytics.Modeling.LineSegment"/> objects in the <see cref="SynchrophasorAnalytics.Modeling.NetworkModel"/>.
        /// </summary>
        [XmlIgnore()]
        public List<LineSegment> LineSegments
        {
            get
            {
                return m_lineSegments;
            }
            set
            {
                m_lineSegments = value;
            }
        }

        /// <summary>
        /// A list of <see cref="SynchrophasorAnalytics.Modeling.SeriesCompensator"/> objects in the <see cref="SynchrophasorAnalytics.Modeling.NetworkModel"/>.
        /// </summary>
        [XmlIgnore()]
        public List<SeriesCompensator> SeriesCompensators
        {
            get
            {
                return m_seriesCompensators;
            }
            set
            {
                m_seriesCompensators = value;
            }
        }

        /// <summary>
        /// A list of <see cref="VoltageLevels"/> in the <see cref="NetworkModel"/>
        /// </summary>
        [XmlArray("VoltageLevels")]
        public List<VoltageLevel> VoltageLevels
        {
            get
            {
                return m_voltageLevels;
            }
            set
            {
                m_voltageLevels = value;
            }
        }

        /// <summary>
        /// A list of the tap models used by the transformers in the network model.
        /// </summary>
        [XmlArray("TapConfigurations")]
        public List<TapConfiguration> TapConfigurations
        {
            get
            {
                return m_tapConfigurations;
            }
            set
            {
                m_tapConfigurations = value;
            }
        }

        /// <summary>
        /// A list of the present observed busses (collections of nodes) in the network based on topology processing and observability analysis.
        /// </summary>
        [XmlIgnore()]
        public List<ObservedBus> ObservedBusses
        {
            get
            {
                return m_observedBusses;
            }
            set
            {
                m_observedBusses = value;
            }
        }

        #endregion

        #region [ Network Measurements ]

        /// <summary>
        /// A list of all of the collections of voltage phasors modeled in the network.
        /// </summary>
        [XmlIgnore()]
        public List<VoltagePhasorGroup> Voltages
        {
            get
            {
                return m_voltages;
            }
            set
            {
                m_voltages = value;
            }
        }

        /// <summary>
        /// A list of all of the current flow measurements modeled in the network.
        /// </summary>
        [XmlIgnore()]
        public List<CurrentFlowPhasorGroup> CurrentFlows
        {
            get
            {
                return m_currentFlows;
            }
            set
            {
                m_currentFlows = value;
            }
        }

        /// <summary>
        /// A list of all of the shunt current injections modeled in the network.
        /// </summary>
        [XmlIgnore()]
        public List<CurrentInjectionPhasorGroup> CurrentInjections
        {
            get
            {
                return m_currentInjections;
            }
            set
            {
                m_currentInjections = value;
            }
        }

        /// <summary>
        /// A list of all of the breaker statuses modeled in the network.
        /// </summary>
        [XmlArray("BreakerStatuses")]
        public List<BreakerStatus> BreakerStatuses
        {
            get
            {
                return m_breakerStatuses;
            }
            set
            {
                m_breakerStatuses = value;
            }
        }

        /// <summary>
        /// A list of all of the status words from all of the devices streaming phasor measurements in the network.
        /// </summary>
        [XmlArray("StatusWords")]
        public List<StatusWord> StatusWords
        {
            get
            {
                return m_statusWords;
            }
            set
            {
                m_statusWords = value;
            }
        }

        /// <summary>
        /// A list of all of the voltage phasor measurements which have been modeled to accept measurements. (measurement keys not equal to the 'Undefined' keyword)
        /// </summary>
        [XmlIgnore()]
        public List<VoltagePhasorGroup> ExpectedVoltages
        {
            get
            {
                if (m_phaseSelection == PhaseSelection.PositiveSequence)
                {
                    return m_voltages.FindAll(voltage => voltage.PositiveSequence.Measurement.MagnitudeKey != "Undefined" &&
                                                         voltage.PositiveSequence.Measurement.AngleKey != "Undefined");
                }
                else if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    return m_voltages.FindAll(voltage => voltage.PhaseA.Measurement.MagnitudeKey != "Undefined" &&
                                                         voltage.PhaseA.Measurement.AngleKey != "Undefined" &&
                                                         voltage.PhaseB.Measurement.MagnitudeKey != "Undefined" &&
                                                         voltage.PhaseB.Measurement.AngleKey != "Undefined" &&
                                                         voltage.PhaseC.Measurement.MagnitudeKey != "Undefined" &&
                                                         voltage.PhaseC.Measurement.AngleKey != "Undefined");
                }
                else
                {
                    return new List<VoltagePhasorGroup>();
                }
            }
        }

        /// <summary>
        /// A list of all of the current flow phasor measurements which have been modeled to accept measurements. (measurement keys not equal to the 'Undefined' keyword)
        /// </summary>
        [XmlIgnore()]
        public List<CurrentFlowPhasorGroup> ExpectedCurrentFlows
        {
            get
            {
                if (m_phaseSelection == PhaseSelection.PositiveSequence)
                {
                    return m_currentFlows.FindAll(current => current.PositiveSequence.Measurement.MagnitudeKey != "Undefined" &&
                                                             current.PositiveSequence.Measurement.AngleKey != "Undefined");
                }
                else if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    return m_currentFlows.FindAll(current => current.PhaseA.Measurement.MagnitudeKey != "Undefined" &&
                                                             current.PhaseA.Measurement.AngleKey != "Undefined" &&
                                                             current.PhaseB.Measurement.MagnitudeKey != "Undefined" &&
                                                             current.PhaseB.Measurement.AngleKey != "Undefined" &&
                                                             current.PhaseC.Measurement.MagnitudeKey != "Undefined" &&
                                                             current.PhaseC.Measurement.AngleKey != "Undefined");
                }
                else
                {
                    return new List<CurrentFlowPhasorGroup>();
                }
            }
        }

        /// <summary>
        /// A list of all of the current injection phasor measurements which have been modeled to accept measurements. (measurement keys not equal to the 'Undefined' keyword)
        /// </summary>
        [XmlIgnore()]
        public List<CurrentInjectionPhasorGroup> ExpectedCurrentInjections
        {
            get
            {
                if (m_phaseSelection == PhaseSelection.PositiveSequence)
                {
                    return m_currentInjections.FindAll(current => current.PositiveSequence.Measurement.MagnitudeKey != "Undefined" &&
                                                                  current.PositiveSequence.Measurement.AngleKey != "Undefined");
                }
                else if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    return m_currentInjections.FindAll(current => current.PhaseA.Measurement.MagnitudeKey != "Undefined" &&
                                                                  current.PhaseA.Measurement.AngleKey != "Undefined" &&
                                                                  current.PhaseB.Measurement.MagnitudeKey != "Undefined" &&
                                                                  current.PhaseB.Measurement.AngleKey != "Undefined" &&
                                                                  current.PhaseC.Measurement.MagnitudeKey != "Undefined" &&
                                                                  current.PhaseC.Measurement.AngleKey != "Undefined");
                }
                else
                {
                    return new List<CurrentInjectionPhasorGroup>();
                }
            }
        }

        /// <summary>
        /// A list of all of the voltage phasor measurements which are of sufficient quality to be considered for inclusion in the state estimation problem. A subset of the total list of voltage phasor measurements.
        /// </summary>
        [XmlIgnore()]
        public List<VoltagePhasorGroup> ActiveVoltages
        {
            get
            {
                if (m_phaseSelection == PhaseSelection.PositiveSequence)
                {
                    return m_voltages.FindAll(voltage => voltage.IncludeInPositiveSequenceEstimator);
                }
                else if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    return m_voltages.FindAll(voltage => voltage.IncludeInEstimator);
                }
                else
                {
                    return new List<VoltagePhasorGroup>();
                }
            }
        }

        /// <summary>
        /// A list of all of the current flow phasor measurements which are of sufficient quality to be considered for inclusion in the state estimation problem. A subset of the total list of current flow phasor measurements.
        /// </summary>
        [XmlIgnore()]
        public List<CurrentFlowPhasorGroup> ActiveCurrentFlows
        {
            get
            {
                return m_activeCurrentFlows;
            }
            set
            {
                m_activeCurrentFlows = value;
            }
        }

        /// <summary>
        /// A list of all of the current flow phasor measurements which satisfy necessary criteria to be included in the state estimation problem. Quality and observability is taken into consideration. It is a subset of the <see cref="SynchrophasorAnalytics.Modeling.NetworkModel.ActiveCurrentFlows"/>.
        /// </summary>
        [XmlIgnore()]
        public List<CurrentFlowPhasorGroup> IncludedCurrentFlows
        {
            get
            {
                return ActiveCurrentFlows.FindAll(x => x.MeasuredFromNode.IsObserved);
            }
        }

        /// <summary>
        /// A list of all of the current injection phasor measurements which satisfy necessary crieteria to be included in the state estimation problem. A subst of the tital list of current injection phasor measurements.
        /// </summary>
        [XmlIgnore()]
        public List<CurrentInjectionPhasorGroup> ActiveCurrentInjections
        {
            get
            {
                if (m_phaseSelection == PhaseSelection.PositiveSequence)
                {
                    return m_currentInjections.FindAll(current => current.IncludeInPositiveSequenceEstimator);
                }
                else if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    return m_currentInjections.FindAll(current => current.IncludeInEstimator);
                }
                else
                {
                    return new List<CurrentInjectionPhasorGroup>();
                }
                //return m_activeCurrentInjections;
            }
            set
            {
                m_activeCurrentInjections = value;
            }
        }

        #endregion

        #region [ Parent ]

        /// <summary>
        /// The parent of the network model
        /// </summary>
        [XmlIgnore()]
        public Network ParentNetwork
        {
            get
            {
                return m_parentNetwork;
            }
            set
            {
                m_parentNetwork = value;
            }
        }

        #endregion

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// The designated constructor method for the <see cref="SynchrophasorAnalytics.Modeling.NetworkModel"/> class.
        /// </summary>
        public NetworkModel()
        {
            m_internalId = 1;
            m_number = 1;
            m_name = "Model";
            m_acronym = "MODEL";
            m_description = "Network Model Description";
            m_rawMeasurementKeyValuePairs = new Dictionary<string, double>();
            InitializeComponentLists();
        }

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Reconstitues the references between parent, child, and sibling objects in 
        /// the <see cref="NetworkModel"/> as well as several other initialization functions.
        /// </summary>
        public void Initialize()
        {
            LinkTransmissionLineReferencesToSubstations();
            LinkHierarchicalReferences();
            LinkVoltageLevelReferences();
            LinkTapConfigurationReferences();
            ListNetworkComponents();
            ListNetworkMeasurements();
            LinkBreakerStatusToCircuitBreakers();
            LinkStatusWordsToPhasorGroups();
            LinkVoltageLevelsToPhasorGroups();
            InitializeComplexPowerCalculations();
            m_observedBusses = new List<ObservedBus>();
            m_activeCurrentFlows = new List<CurrentFlowPhasorGroup>();
        }

        /// <summary>
        /// Clears the values before receiving a new set of measurements
        /// </summary>
        public void ClearValues()
        {
            foreach (VoltagePhasorGroup voltagePhasorGroup in m_voltages)
            {
                voltagePhasorGroup.ClearValues();
            }

            foreach (CurrentFlowPhasorGroup currentPhasorGroup in m_currentFlows)
            {
                currentPhasorGroup.ClearValues();
            }

            foreach (CurrentInjectionPhasorGroup currentInjectionPhasorGroup in m_currentInjections)
            {
                currentInjectionPhasorGroup.ClearValues();
            }

            foreach (BreakerStatus breakerStatus in m_breakerStatuses)
            {
                breakerStatus.ClearValues();
            }

            foreach (StatusWord statusWord in m_statusWords)
            {
                statusWord.ClearValues();
            }
        }

        /// <summary>
        /// Called when a full set of measurements has been passed to the <see cref="NetworkModel"/>
        /// </summary>
        public void OnNewMeasurements()
        {
            InsertVoltageMeasurements();
            InsertCurrentFlowMeasurements();
            InsertCurrentInjectionMeasurements();
            InsertBreakerStatuses();
            InsertStatusWords();
            InsertTransformerTapPositions();
        }

        /// <summary>
        /// The key value pairs of the measurements which were successfully assigned to their network element.
        /// </summary>
        /// <returns>A dictionary of the key value pairs.</returns>
        public Dictionary<string, double> GetReceivedMeasurements()
        {
            Dictionary<string, double> receivedMeasurements = new Dictionary<string, double>();

            AppendReceivedVoltageMeasurements(receivedMeasurements);
            AppendReceivedCurrentFlowMeasurements(receivedMeasurements);
            AppendReceivedCurrentInjectionMeasurements(receivedMeasurements);

            return receivedMeasurements;
        }

        /// <summary>
        /// Resolves the network from breaker/switch/node to bus/branch.
        /// </summary>
        /// <returns></returns>
        public void ResolveToObservedBusses()
        {
            m_observedBusses.Clear();

            foreach (Substation substation in m_substations)
            {
                substation.InitializeSubstationGraph();
                substation.Graph.ResolveDirectlyConnectedAdjacencies();
                List<ObservedBus> substationObservedBusses = CheckObservability(substation.Graph.ResolveToObservedBusses());
                if (substationObservedBusses != null)
                {
                    foreach (ObservedBus observedBus in substationObservedBusses)
                    {
                        m_observedBusses.Add(observedBus);
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void ResolveToSingleFlowBranches()
        {
            foreach (TransmissionLine transmissionLine in m_transmissionLines)
            {
                transmissionLine.RunTopologyProcessing();
                transmissionLine.ComputeRealTimePositiveSequenceImpedance();
                transmissionLine.RunSeriesCompensatorStatusInference();
                transmissionLine.SetFinalImpedanceValues();
            }
        }

        /// <summary>
        /// Determines which phasors are eligible for state computation.
        /// </summary>
        /// <returns></returns>
        public void DetermineActiveCurrentFlows()
        {
            m_activeCurrentFlows.Clear();

            if (m_phaseSelection == PhaseSelection.PositiveSequence)
            {
                foreach (CurrentFlowPhasorGroup currentPhasorGroup in m_currentFlows)
                {
                    if (currentPhasorGroup.IncludeInPositiveSequenceEstimator)
                    {
                        m_activeCurrentFlows.Add(currentPhasorGroup);
                    }
                }
            }
            else if (m_phaseSelection == PhaseSelection.ThreePhase)
            {
                foreach (CurrentFlowPhasorGroup currentPhasorGroup in m_currentFlows)
                {
                    if (currentPhasorGroup.IncludeInEstimator)
                    {
                        m_activeCurrentFlows.Add(currentPhasorGroup);
                    }
                }
            }
        }

        /// <summary>
        /// Determines which phasors are eligible for state computation.
        /// </summary>
        /// <returns></returns>
        public void DetermineActiveCurrentInjections()
        {
            m_activeCurrentInjections.Clear();

            if (m_phaseSelection == PhaseSelection.PositiveSequence)
            {
                foreach (CurrentInjectionPhasorGroup currentPhasorGroup in m_currentInjections)
                {
                    if (currentPhasorGroup.IncludeInPositiveSequenceEstimator)
                    {
                        m_activeCurrentInjections.Add(currentPhasorGroup);
                    }
                }
            }
            else if (m_phaseSelection == PhaseSelection.ThreePhase)
            {
                foreach (CurrentInjectionPhasorGroup currentPhasorGroup in m_currentInjections)
                {
                    if (currentPhasorGroup.IncludeInEstimator)
                    {
                        m_activeCurrentInjections.Add(currentPhasorGroup);
                    }
                }
            }
        }

        /// <summary>
        /// Computes the sequence component values for the measured and estimated phasors
        /// </summary>
        public void ComputeSequenceValues()
        {
            ComputeVoltageSequenceValues();
            ComputeCurrentFlowSequenceValues();
            ComputeCurrentInjectionSequenceValues();
        }
        
        /// <summary>
        /// Computes the current flows on series branches based on the results of the linear state estimator.
        /// </summary>
        public void ComputeEstimatedCurrentFlows()
        {
            foreach (Transformer transformer in m_transformers)
            {
                if (m_phaseSelection == PhaseSelection.PositiveSequence)
                {
                    transformer.ComputePositiveSequenceEstimatedCurrentFlow();
                }
                else if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    transformer.ComputeThreePhaseEstimatedCurrentFlow();
                }
            }

            foreach (TransmissionLine transmissionLine in m_transmissionLines)
            {
                if (m_phaseSelection == PhaseSelection.PositiveSequence)
                {
                    transmissionLine.ComputePositiveSequenceEstimatedCurrentFlow();
                }
                else if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    transmissionLine.ComputeThreePhaseEstimatedCurrentFlow();
                }
            }
        }

        /// <summary>
        /// Computes the current injections of shunt devices based on the results from the linear state estimator.
        /// </summary>
        public void ComputeEstimatedCurrentInjections()
        {
            foreach (ShuntCompensator shunt in m_shuntCompensators)
            {
                if (m_phaseSelection == PhaseSelection.PositiveSequence)
                {
                    shunt.ComputePositiveSequenceEstimatedCurrentInjection();
                }
                else if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    shunt.ComputeThreePhaseEstimatedCurrentInjection();
                }
            }
        }

        /// <summary>
        /// Changes the status of a specified switching device to a specified state.
        /// </summary>
        /// <param name="switchingDeviceName">The name of the switching device.</param>
        /// <param name="desiredActualState">The desired state for the switching device.</param>
        /// <returns>A boolean flag representing the success of the action.</returns>
        public bool ToggleSwitchingDeviceStatus(string switchingDeviceName, SwitchingDeviceActualState desiredActualState)
        {
            Dictionary<string, SwitchingDeviceBase> switchingDevices = m_switchingDevices.ToDictionary(x => x.Name, x => x);

            SwitchingDeviceBase switchingDevice = null;

            if (switchingDevices.TryGetValue(switchingDeviceName, out switchingDevice))
            {
                switchingDevice.ManuallySwitchTo(desiredActualState);

                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Removes the specified switching device from manual override status and specifies either to revert to a default/normal state or to remain in its present state until otherwise updated by measurements.
        /// </summary>
        /// <param name="switchingDeviceName">The specified switching device to be removed from manual override status.</param>
        /// <param name="revertToDefaultState">A boolean flag which represents whether to revert the switching device into a default/normal state or not.</param>
        /// <returns>A boolean flag representing the success of the action.</returns>
        public bool RemoveSwitchingDeviceFromManualWithPreserveOrDefault(string switchingDeviceName, bool revertToDefaultState)
        {
            Dictionary<string, SwitchingDeviceBase> switchingDevices = m_switchingDevices.ToDictionary(x => x.Name, x => x);

            SwitchingDeviceBase switchingDevice = null;

            if (switchingDevices.TryGetValue(switchingDeviceName, out switchingDevice))
            {
                if (revertToDefaultState)
                {
                    switchingDevice.RemoveFromManualAndRevertToDefault();
                }
                else
                {
                    switchingDevice.RemoveFromManualAndPreserveStateUntilUpdated();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a list of the number of each network component and measurement.
        /// </summary>
        /// <returns>Returns the list.</returns>
        public string ComponentList()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("    ---- Component List ---- ");
            stringBuilder.AppendLine("           Companies: " + m_companies.Count.ToString());
            stringBuilder.AppendLine("           Divisions: " + m_divisions.Count.ToString());
            stringBuilder.AppendLine("         Substations: " + m_substations.Count.ToString());
            stringBuilder.AppendLine("  Transmission Lines: " + m_transmissionLines.Count.ToString());
            stringBuilder.AppendLine("               Nodes: " + m_nodes.Count.ToString());
            stringBuilder.AppendLine("            Switches: " + m_switches.Count.ToString());
            stringBuilder.AppendLine("    Circuit Breakers: " + m_circuitBreakers.Count.ToString());
            stringBuilder.AppendLine("        Transformers: " + m_transformers.Count.ToString());
            stringBuilder.AppendLine("  Shunt Compensators: " + m_shuntCompensators.Count.ToString());
            stringBuilder.AppendLine("       Line Segments: " + m_lineSegments.Count.ToString());
            stringBuilder.AppendLine(" Series Compensators: " + m_seriesCompensators.Count.ToString());
            stringBuilder.AppendLine("      Voltage Levels: " + m_voltageLevels.Count.ToString());
            stringBuilder.AppendLine("  Tap Configurations: " + m_tapConfigurations.Count.ToString());
            stringBuilder.AppendLine("            Voltages: " + m_voltages.Count.ToString());
            stringBuilder.AppendLine("       Current Flows: " + m_currentFlows.Count.ToString());
            stringBuilder.AppendLine("  Current Injections: " + m_currentInjections.Count.ToString());
            stringBuilder.AppendLine("      Breaker Status: " + m_breakerStatuses.Count.ToString());
            stringBuilder.AppendLine("        Status Words: " + m_statusWords.Count.ToString());

            return stringBuilder.ToString();
        }

        /// <summary>
        /// A summary of the measurement inclusion for the most recent set of network measurements.
        /// </summary>
        /// <returns>A string summary of each of the three phasor group types.</returns>
        public string MeasurementInclusionStatusList()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("      ---- Voltage Phasor Group Status ---- ");
            stringBuilder.AppendLine("  Modeled Voltages: " + Voltages.Count.ToString() + " groups");
            stringBuilder.AppendLine(" Expected Voltages: " + ExpectedVoltages.Count.ToString() + " groups");
            stringBuilder.AppendLine("   Active Voltages: " + ActiveVoltages.Count.ToString() + " groups");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("    ---- Current Flow Phasor Group Status ---- ");
            stringBuilder.AppendLine("  Modeled Currents: " + CurrentFlows.Count.ToString() + " groups");
            stringBuilder.AppendLine(" Expected Currents: " + ExpectedCurrentFlows.Count.ToString() + " groups");
            stringBuilder.AppendLine("   Active Currents: " + ActiveCurrentFlows.Count.ToString() + " groups");
            stringBuilder.AppendLine(" Included Currents: " + IncludedCurrentFlows.Count.ToString() + " groups");
            stringBuilder.AppendLine("");
            stringBuilder.AppendLine("  ---- Current Injection Phasor Group Status ---- ");
            stringBuilder.AppendLine("  Modeled Currents: " + CurrentInjections.Count.ToString() + " groups");
            stringBuilder.AppendLine(" Expected Currents: " + ExpectedCurrentInjections.Count.ToString() + " groups");
            stringBuilder.AppendLine("   Active Currents: " + ActiveCurrentInjections.Count.ToString() + " groups");
            stringBuilder.AppendLine("");

            return stringBuilder.ToString();
        }

        #endregion

        #region [ Private Methods ]

        private void InitializeComponentLists()
        {
            // Network Components
            m_companies = new List<Company>();
            m_divisions = new List<Division>();
            m_substations = new List<Substation>();
            m_transmissionLines = new List<TransmissionLine>();
            m_nodes = new List<Node>();
            m_switches = new List<Switch>();
            m_circuitBreakers = new List<CircuitBreaker>();
            m_switchingDevices = new List<SwitchingDeviceBase>();
            m_shuntCompensators = new List<ShuntCompensator>();
            m_transformers = new List<Transformer>();
            m_lineSegments = new List<LineSegment>();
            m_seriesCompensators = new List<SeriesCompensator>();
            m_voltageLevels = new List<VoltageLevel>();
            m_tapConfigurations = new List<TapConfiguration>();

            // Network Measurements
            m_voltages = new List<VoltagePhasorGroup>();
            m_currentFlows = new List<CurrentFlowPhasorGroup>();
            m_currentInjections = new List<CurrentInjectionPhasorGroup>();
            m_breakerStatuses = new List<BreakerStatus>();
            m_statusWords = new List<StatusWord>();
            m_activeCurrentFlows = new List<CurrentFlowPhasorGroup>();
            m_activeCurrentInjections = new List<CurrentInjectionPhasorGroup>();

            // Network Settings
            m_inputOutputSettings = new InputOutputSettings();
        }

        /// <summary>
        /// Uses the <see cref="SynchrophasorAnalytics.Modeling.TransmissionLine.FromSubstationID"/> and <see cref="SynchrophasorAnalytics.Modeling.TransmissionLine.ToSubstationID"/>
        /// and the <see cref="SynchrophasorAnalytics.Modeling.INetworkDescribable.InternalID"/> to link the <see cref="SynchrophasorAnalytics.Modeling.Substation"/> records with their associated <see cref="SynchrophasorAnalytics.Modeling.TransmissionLine"/>.
        /// </summary>
        private void LinkTransmissionLineReferencesToSubstations()
        {
            // Transmission Line From and To Substations
            Dictionary<int, Substation> substations = new Dictionary<int, Substation>();

            foreach (Company company in m_companies)
            {
                foreach (Division division in company.Divisions)
                {
                    foreach (Substation substation in division.Substations)
                    {
                        substations.Add(substation.InternalID, substation);
                    }
                }
            }

            foreach (Company company in m_companies)
            {
                foreach (Division division in company.Divisions)
                {
                    foreach (TransmissionLine transmissionLine in division.TransmissionLines)
                    {
                        Substation value = null;
                        if (substations.TryGetValue(transmissionLine.FromSubstationID, out value))
                        {
                            transmissionLine.FromSubstation = value;
                            value = null;
                        }
                        if (substations.TryGetValue(transmissionLine.ToSubstationID, out value))
                        {
                            transmissionLine.ToSubstation = value;
                            value = null;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Establishes the parent/child relationship between entities in the <see cref="NetworkModel"/>.
        /// </summary>
        private void LinkHierarchicalReferences()
        {
            Dictionary<int, Node> substationNodes = new Dictionary<int, Node>();
            foreach (Company company in m_companies)
            {
                company.ParentModel = this;
                foreach (Division division in company.Divisions)
                {
                    division.ParentCompany = company;
                    foreach (Substation substation in division.Substations)
                    {
                        substation.ParentDivision = division;
                        foreach (Node node in substation.Nodes)
                        {
                            node.ParentSubstation = substation;
                            node.Voltage.MeasuredNode = node;
                            substationNodes.Add(node.InternalID, node);
                        }

                        Dictionary<int, Node> localNodes = substation.Nodes.ToDictionary(x => x.InternalID, x => x);
                        foreach (CircuitBreaker circuitBreaker in substation.CircuitBreakers)
                        {
                            circuitBreaker.ParentSubstation = substation;

                            Node value = null;
                            if (localNodes.TryGetValue(circuitBreaker.FromNodeID, out value))
                            {
                                circuitBreaker.FromNode = value;
                                value = null;
                            }
                            if (localNodes.TryGetValue(circuitBreaker.ToNodeID, out value))
                            {
                                circuitBreaker.ToNode = value;
                                value = null;
                            }
                        }

                        foreach (Switch circuitSwitch in substation.Switches)
                        {
                            circuitSwitch.ParentSubstation = substation;

                            Node value = null;
                            if (localNodes.TryGetValue(circuitSwitch.FromNodeID, out value))
                            {
                                circuitSwitch.FromNode = value;
                                value = null;
                            }
                            if (localNodes.TryGetValue(circuitSwitch.ToNodeID, out value))
                            {
                                circuitSwitch.ToNode = value;
                                value = null;
                            }
                        }

                        foreach (Transformer transformer in substation.Transformers)
                        {
                            transformer.ParentSubstation = substation;

                            Node value = null;
                            if (localNodes.TryGetValue(transformer.FromNodeID, out value))
                            {
                                transformer.FromNode = value;
                                transformer.FromNodeCurrent.MeasuredFromNode = value;
                                transformer.ToNodeCurrent.MeasuredToNode = value;
                                value = null;
                            }
                            if (localNodes.TryGetValue(transformer.ToNodeID, out value))
                            {
                                transformer.ToNode = value;
                                transformer.ToNodeCurrent.MeasuredFromNode = value;
                                transformer.FromNodeCurrent.MeasuredToNode = value;
                                value = null;
                            }
                        }

                        foreach (ShuntCompensator shunt in substation.Shunts)
                        {
                            shunt.ParentSubstation = substation;

                            Node value = null;
                            if (localNodes.TryGetValue(shunt.ConnectedNodeID, out value))
                            {
                                shunt.ConnectedNode = value;
                                value = null;
                            }
                        }
                    }
                }
            }

            foreach (Company company in m_companies)
            {
                foreach (Division division in company.Divisions)
                {
                    foreach (TransmissionLine transmissionLine in division.TransmissionLines)
                    {
                        transmissionLine.ParentDivision = division;
                        //transmissionLine.SetFinalImpedanceValues();
                        Dictionary<int, Node> transmissionLineNodes = transmissionLine.Nodes.ToDictionary(x => x.InternalID, x => x);
                        Node value = null;
                        if (substationNodes.TryGetValue(transmissionLine.FromNodeID, out value))
                        {
                            transmissionLine.FromNode = value;
                            transmissionLine.FromNode.ParentTransmissionLine = transmissionLine;
                            transmissionLine.FromSubstationCurrent.MeasuredFromNode = value;
                            transmissionLine.ToSubstationCurrent.MeasuredToNode = value;
                            transmissionLineNodes.Add(value.InternalID, value);
                            value = null;
                        }

                        if (substationNodes.TryGetValue(transmissionLine.ToNodeID, out value))
                        {
                            transmissionLine.ToNode = value;
                            transmissionLine.ToNode.ParentTransmissionLine = transmissionLine;
                            transmissionLine.ToSubstationCurrent.MeasuredFromNode = value;
                            transmissionLine.FromSubstationCurrent.MeasuredToNode = value;
                            transmissionLineNodes.Add(value.InternalID, value);
                            value = null;
                        }

                        foreach (Node node in transmissionLine.Nodes)
                        {
                            node.ParentTransmissionLine = transmissionLine;
                        }


                        foreach (Switch lineSwitch in transmissionLine.Switches)
                        {
                            lineSwitch.ParentTransmissionLine = transmissionLine;

                            value = null;
                            if (transmissionLineNodes.TryGetValue(lineSwitch.FromNodeID, out value))
                            {
                                lineSwitch.FromNode = value;
                                value = null;
                            }
                            if (transmissionLineNodes.TryGetValue(lineSwitch.ToNodeID, out value))
                            {
                                lineSwitch.ToNode = value;
                                value = null;
                            }
                        }

                        foreach (LineSegment lineSegment in transmissionLine.LineSegments)
                        {
                            lineSegment.ParentTransmissionLine = transmissionLine;

                            value = null;
                            if (transmissionLineNodes.TryGetValue(lineSegment.FromNodeID, out value))
                            {
                                lineSegment.FromNode = value;
                                value = null;
                            }
                            if (transmissionLineNodes.TryGetValue(lineSegment.ToNodeID, out value))
                            {
                                lineSegment.ToNode = value;
                                value = null;
                            }
                        }

                        foreach (SeriesCompensator seriesCompensator in transmissionLine.SeriesCompensators)
                        {
                            seriesCompensator.ParentTransmissionLine = transmissionLine;

                            value = null;
                            if (transmissionLineNodes.TryGetValue(seriesCompensator.FromNodeID, out value))
                            {
                                seriesCompensator.FromNode = value;
                                value = null;
                            }
                            if (transmissionLineNodes.TryGetValue(seriesCompensator.ToNodeID, out value))
                            {
                                seriesCompensator.ToNode = value;
                                value = null;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Uses the <see cref="Node.VoltageLevelID"/> and the <see cref="VoltageLevel.InternalID"/> to link
        /// the <see cref="VoltageLevel"/> records with their associated <see cref="Node"/>.
        /// </summary>
        private void LinkVoltageLevelReferences()
        {
            Dictionary<int, VoltageLevel> voltageLevels = m_voltageLevels.ToDictionary(x => x.InternalID, x => x);

            foreach (Company company in m_companies)
            {
                foreach (Division division in company.Divisions)
                {
                    foreach (Substation substation in division.Substations)
                    {
                        foreach (Node node in substation.Nodes)
                        {
                            VoltageLevel value = null;
                            if (voltageLevels.TryGetValue(node.VoltageLevelID, out value))
                            {
                                node.BaseKV = value;
                                value = null;
                            }
                        }
                    }

                    foreach (TransmissionLine transmissionLine in division.TransmissionLines)
                    {
                        foreach (Node node in transmissionLine.Nodes)
                        {
                            VoltageLevel value = null;
                            if (voltageLevels.TryGetValue(node.VoltageLevelID, out value))
                            {
                                node.BaseKV = value;
                                value = null;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Uses the <see cref="Transformer.TapConfigurationID"/> and the <see cref="TapConfiguration.InternalID"/> to link
        /// the <see cref="TapConfiguration"/> objects with their associated <see cref="Transformer.Tap"/>.
        /// </summary>
        private void LinkTapConfigurationReferences()
        {
            Dictionary<int, TapConfiguration> tapConfigurations = m_tapConfigurations.ToDictionary(x => x.InternalID, x => x);

            foreach (Company company in m_companies)
            {
                foreach (Division division in company.Divisions)
                {
                    foreach (Substation substation in division.Substations)
                    {
                        foreach (Transformer transformer in substation.Transformers)
                        {
                            TapConfiguration value = null;
                            if (tapConfigurations.TryGetValue(transformer.TapConfigurationID, out value))
                            {
                                transformer.Tap = value;
                                value = null;
                            }
                        }
                    }
                }
            }
        }

        private void ListNetworkComponents()
        {
            foreach (Company company in m_companies)
            {
                foreach (Division division in company.Divisions)
                {
                    m_divisions.Add(division);

                    foreach (Substation substation in division.Substations)
                    {
                        m_substations.Add(substation);

                        foreach (Node node in substation.Nodes)
                        {
                            m_nodes.Add(node);
                        }

                        foreach (Switch disconnect in substation.Switches)
                        {
                            m_switches.Add(disconnect);
                            m_switchingDevices.Add(disconnect as SwitchingDeviceBase);
                        }

                        foreach (CircuitBreaker circuitBreaker in substation.CircuitBreakers)
                        {
                            m_circuitBreakers.Add(circuitBreaker);
                            m_switchingDevices.Add(circuitBreaker as SwitchingDeviceBase);
                        }

                        foreach (ShuntCompensator shuntCompensator in substation.Shunts)
                        {
                            m_shuntCompensators.Add(shuntCompensator);
                        }

                        foreach (Transformer transformer in substation.Transformers)
                        {
                            m_transformers.Add(transformer);
                        }
                    }

                    foreach (TransmissionLine transmissionLine in division.TransmissionLines)
                    {
                        m_transmissionLines.Add(transmissionLine);

                        foreach (Node node in transmissionLine.Nodes)
                        {
                            m_nodes.Add(node);
                        }

                        foreach (Switch disconnect in transmissionLine.Switches)
                        {
                            m_switches.Add(disconnect);
                            m_switchingDevices.Add(disconnect as SwitchingDeviceBase);
                        }

                        foreach (LineSegment lineSegment in transmissionLine.LineSegments)
                        {
                            m_lineSegments.Add(lineSegment);
                        }

                        foreach (SeriesCompensator seriesCompensator in transmissionLine.SeriesCompensators)
                        {
                            m_seriesCompensators.Add(seriesCompensator);
                        }
                    }
                }
            }
        }

        private void ListNetworkMeasurements()
        {
            foreach (Company company in m_companies)
            {
                foreach (Division division in company.Divisions)
                {
                    foreach (Substation substation in division.Substations)
                    {
                        foreach (Node node in substation.Nodes)
                        {
                            m_voltages.Add(node.Voltage);
                        }

                        foreach (Transformer transformer in substation.Transformers)
                        {
                            // Add the current phasor groups to the list
                            m_currentFlows.Add(transformer.FromNodeCurrent);
                            m_currentFlows.Add(transformer.ToNodeCurrent);

                            // Set the measured branch of each current phasor group
                            transformer.FromNodeCurrent.MeasuredBranch = transformer;
                            transformer.ToNodeCurrent.MeasuredBranch = transformer;
                        }

                        foreach (ShuntCompensator shunt in substation.Shunts)
                        {
                            m_currentInjections.Add(shunt.Current);

                            shunt.Current.MeasuredBranch = shunt;
                        }
                    }
                    foreach (TransmissionLine transmissionLine in division.TransmissionLines)
                    {
                        // Add the current phasor groups to the list
                        m_currentFlows.Add(transmissionLine.FromSubstationCurrent);
                        m_currentFlows.Add(transmissionLine.ToSubstationCurrent);

                        // Set the measured branch of each current phasor group
                        transmissionLine.FromSubstationCurrent.MeasuredBranch = transmissionLine;
                        transmissionLine.ToSubstationCurrent.MeasuredBranch = transmissionLine;

                        foreach (Node node in transmissionLine.Nodes)
                        {
                            if (!m_voltages.Contains(node.Voltage))
                            {
                                m_voltages.Add(node.Voltage);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Links the <see cref="BreakerStatus"/> objects to their parent <see cref="CircuitBreaker"/>.
        /// </summary>
        private void LinkBreakerStatusToCircuitBreakers()
        {
            Dictionary<int, BreakerStatus> breakerStatuses = m_breakerStatuses.ToDictionary(x => x.InternalID, x => x);
            foreach (Company company in m_companies)
            {
                foreach (Division division in company.Divisions)
                {
                    foreach (Substation substation in division.Substations)
                    {
                        foreach (CircuitBreaker circuitBreaker in substation.CircuitBreakers)
                        {
                            BreakerStatus value = null;
                            if (circuitBreaker.StatusID != 0 && breakerStatuses.TryGetValue(circuitBreaker.StatusID, out value))
                            {
                                circuitBreaker.Status = value;
                                circuitBreaker.Status.ParentCircuitBreaker = circuitBreaker;
                                value = null;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Uses the <see cref="PhasorGroup.StatusWordID"/> and the <see cref="StatusWord.InternalID"/> to link
        /// the <see cref="StatusWord"/> measurements with their associated <see cref="PhasorGroup"/>.
        /// </summary>
        private void LinkStatusWordsToPhasorGroups()
        {
            Dictionary<int, StatusWord> statusWords = m_statusWords.ToDictionary(x => x.InternalID, x => x);

            foreach (VoltagePhasorGroup voltagePhasorGroup in m_voltages)
            {
                StatusWord value = null;

                if (voltagePhasorGroup.StatusWordID != 0 && statusWords.TryGetValue(voltagePhasorGroup.StatusWordID, out value))
                {
                    voltagePhasorGroup.Status = value;
                    value = null;
                }
                else
                {
                    voltagePhasorGroup.Status = new StatusWord();
                }
            }

            foreach (CurrentFlowPhasorGroup currentPhasorGroup in m_currentFlows)
            {
                StatusWord value = null;
                if (currentPhasorGroup.StatusWordID != 0 && statusWords.TryGetValue(currentPhasorGroup.StatusWordID, out value))
                {
                    currentPhasorGroup.Status = value;
                    value = null;
                }
                else
                {
                    currentPhasorGroup.Status = new StatusWord();
                }
            }
        }

        /// <summary>
        /// Uses the <see cref="PhasorBase.VoltageLevelID"/> and the <see cref="VoltageLevel.InternalID"/> to link
        /// the <see cref="VoltageLevel"/> records with their associated <see cref="PhasorBase"/>.
        /// </summary>
        private void LinkVoltageLevelsToPhasorGroups()
        {
            Dictionary<int, VoltageLevel> voltageLevels = m_voltageLevels.ToDictionary(x => x.InternalID, x => x);

            foreach (VoltagePhasorGroup voltagePhasorGroup in m_voltages)
            {
                VoltageLevel value = null;
                // Positive Sequence
                if (voltageLevels.TryGetValue(voltagePhasorGroup.PositiveSequence.Measurement.VoltageLevelID, out value))
                {
                    voltagePhasorGroup.PositiveSequence.Measurement.BaseKV = value;
                    value = null;
                }
                if (voltageLevels.TryGetValue(voltagePhasorGroup.PositiveSequence.Estimate.VoltageLevelID, out value))
                {
                    voltagePhasorGroup.PositiveSequence.Estimate.BaseKV = value;
                    value = null;
                }

                if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    // Phase A
                    if (voltageLevels.TryGetValue(voltagePhasorGroup.PhaseA.Measurement.VoltageLevelID, out value))
                    {
                        voltagePhasorGroup.PhaseA.Measurement.BaseKV = value;
                        value = null;
                    }
                    if (voltageLevels.TryGetValue(voltagePhasorGroup.PhaseA.Estimate.VoltageLevelID, out value))
                    {
                        voltagePhasorGroup.PhaseA.Estimate.BaseKV = value;
                        value = null;
                    }

                    // Phase B
                    if (voltageLevels.TryGetValue(voltagePhasorGroup.PhaseB.Measurement.VoltageLevelID, out value))
                    {
                        voltagePhasorGroup.PhaseB.Measurement.BaseKV = value;
                        value = null;
                    }
                    if (voltageLevels.TryGetValue(voltagePhasorGroup.PhaseB.Estimate.VoltageLevelID, out value))
                    {
                        voltagePhasorGroup.PhaseB.Estimate.BaseKV = value;
                        value = null;
                    }

                    // Phase C
                    if (voltageLevels.TryGetValue(voltagePhasorGroup.PhaseC.Measurement.VoltageLevelID, out value))
                    {
                        voltagePhasorGroup.PhaseC.Measurement.BaseKV = value;
                        value = null;
                    }
                    if (voltageLevels.TryGetValue(voltagePhasorGroup.PhaseC.Estimate.VoltageLevelID, out value))
                    {
                        voltagePhasorGroup.PhaseC.Estimate.BaseKV = value;
                        value = null;
                    }
                }

                foreach (VoltagePhasorGroup voltage in m_voltages)
                {
                    voltage.PositiveSequence.Measurement.BaseKV = voltage.MeasuredNode.BaseKV;
                    voltage.PositiveSequence.Estimate.BaseKV = voltage.MeasuredNode.BaseKV;
                    voltage.PhaseA.Measurement.BaseKV = voltage.MeasuredNode.BaseKV;
                    voltage.PhaseA.Estimate.BaseKV = voltage.MeasuredNode.BaseKV;
                    voltage.PhaseB.Measurement.BaseKV = voltage.MeasuredNode.BaseKV;
                    voltage.PhaseB.Estimate.BaseKV = voltage.MeasuredNode.BaseKV;
                    voltage.PhaseC.Measurement.BaseKV = voltage.MeasuredNode.BaseKV;
                    voltage.PhaseC.Estimate.BaseKV = voltage.MeasuredNode.BaseKV;
                }

                foreach (CurrentFlowPhasorGroup current in m_currentFlows)
                {
                    current.PositiveSequence.Measurement.BaseKV = current.MeasuredFromNode.BaseKV;
                    current.PositiveSequence.Estimate.BaseKV = current.MeasuredFromNode.BaseKV;
                    current.PhaseA.Measurement.BaseKV = current.MeasuredFromNode.BaseKV;
                    current.PhaseA.Estimate.BaseKV = current.MeasuredFromNode.BaseKV;
                    current.PhaseB.Measurement.BaseKV = current.MeasuredFromNode.BaseKV;
                    current.PhaseB.Estimate.BaseKV = current.MeasuredFromNode.BaseKV;
                    current.PhaseC.Measurement.BaseKV = current.MeasuredFromNode.BaseKV;
                    current.PhaseC.Estimate.BaseKV = current.MeasuredFromNode.BaseKV;
                }

                foreach (CurrentInjectionPhasorGroup current in m_currentInjections)
                {
                    current.PositiveSequence.Measurement.BaseKV = current.MeasuredConnectedNode.BaseKV;
                    current.PositiveSequence.Estimate.BaseKV = current.MeasuredConnectedNode.BaseKV;
                    current.PhaseA.Measurement.BaseKV = current.MeasuredConnectedNode.BaseKV;
                    current.PhaseA.Estimate.BaseKV = current.MeasuredConnectedNode.BaseKV;
                    current.PhaseB.Measurement.BaseKV = current.MeasuredConnectedNode.BaseKV;
                    current.PhaseB.Estimate.BaseKV = current.MeasuredConnectedNode.BaseKV;
                    current.PhaseC.Measurement.BaseKV = current.MeasuredConnectedNode.BaseKV;
                    current.PhaseC.Estimate.BaseKV = current.MeasuredConnectedNode.BaseKV;
                }
            }

            foreach (CurrentFlowPhasorGroup currentPhasorGroup in m_currentFlows)
            {
                VoltageLevel value = null;
                // Positive Sequence
                if (voltageLevels.TryGetValue(currentPhasorGroup.PositiveSequence.Measurement.VoltageLevelID, out value))
                {
                    currentPhasorGroup.PositiveSequence.Measurement.BaseKV = value;
                    value = null;
                }
                if (voltageLevels.TryGetValue(currentPhasorGroup.PositiveSequence.Estimate.VoltageLevelID, out value))
                {
                    currentPhasorGroup.PositiveSequence.Estimate.BaseKV = value;
                    value = null;
                }

                if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    // Phase A
                    if (voltageLevels.TryGetValue(currentPhasorGroup.PhaseA.Measurement.VoltageLevelID, out value))
                    {
                        currentPhasorGroup.PhaseA.Measurement.BaseKV = value;
                        value = null;
                    }
                    if (voltageLevels.TryGetValue(currentPhasorGroup.PhaseA.Estimate.VoltageLevelID, out value))
                    {
                        currentPhasorGroup.PhaseA.Estimate.BaseKV = value;
                        value = null;
                    }

                    // Phase B
                    if (voltageLevels.TryGetValue(currentPhasorGroup.PhaseB.Measurement.VoltageLevelID, out value))
                    {
                        currentPhasorGroup.PhaseB.Measurement.BaseKV = value;
                        value = null;
                    }
                    if (voltageLevels.TryGetValue(currentPhasorGroup.PhaseB.Estimate.VoltageLevelID, out value))
                    {
                        currentPhasorGroup.PhaseB.Estimate.BaseKV = value;
                        value = null;
                    }

                    // Phase C
                    if (voltageLevels.TryGetValue(currentPhasorGroup.PhaseC.Measurement.VoltageLevelID, out value))
                    {
                        currentPhasorGroup.PhaseC.Measurement.BaseKV = value;
                        value = null;
                    }
                    if (voltageLevels.TryGetValue(currentPhasorGroup.PhaseC.Estimate.VoltageLevelID, out value))
                    {
                        currentPhasorGroup.PhaseC.Estimate.BaseKV = value;
                        value = null;
                    }
                }
            }
        }

        public List<string> GetInputMeasurementKeys() //bug private List<string> GetInputMeasurementKeys()
        {
            List<string> inputMeasurementKeys = new List<string>();

            inputMeasurementKeys.AddRange(GetVoltagePhasorGroupInputKeys());
            inputMeasurementKeys.AddRange(GetCurrentFlowPhasorGroupInputKeys());
            inputMeasurementKeys.AddRange(GetCurrentInjectionPhasorGroupInputKeys());
            inputMeasurementKeys.AddRange(GetBreakerStatusInputKeys());
            inputMeasurementKeys.AddRange(GetStatusWordInputKeys());
            inputMeasurementKeys.AddRange(GetTransformerTapInputKeys());

            return inputMeasurementKeys;
        }

        public List<string> GetVoltagePhasorGroupInputKeys()
        {
            List<string> inputMeasurementKeys = new List<string>();

            foreach (VoltagePhasorGroup voltagePhasorGroup in m_voltages)
            {
                if (m_inputOutputSettings.AcceptsMeasurements)
                {
                    inputMeasurementKeys.Add(voltagePhasorGroup.PositiveSequence.Measurement.MagnitudeKey);
                    inputMeasurementKeys.Add(voltagePhasorGroup.PositiveSequence.Measurement.AngleKey);

                    if (m_phaseSelection == PhaseSelection.ThreePhase)
                    {
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseA.Measurement.MagnitudeKey);
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseA.Measurement.AngleKey);
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseB.Measurement.MagnitudeKey);
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseB.Measurement.AngleKey);
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseC.Measurement.MagnitudeKey);
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseC.Measurement.AngleKey);
                    }
                }

                if (m_inputOutputSettings.AcceptsEstimates)
                {
                    inputMeasurementKeys.Add(voltagePhasorGroup.PositiveSequence.Estimate.MagnitudeKey);
                    inputMeasurementKeys.Add(voltagePhasorGroup.PositiveSequence.Estimate.AngleKey);

                    if (m_phaseSelection == PhaseSelection.ThreePhase)
                    {
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseA.Estimate.MagnitudeKey);
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseA.Estimate.AngleKey);
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseB.Estimate.MagnitudeKey);
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseB.Estimate.AngleKey);
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseC.Estimate.MagnitudeKey);
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseC.Estimate.AngleKey);
                    }
                }
            }

            return inputMeasurementKeys;
        }

        private List<string> GetCurrentFlowPhasorGroupInputKeys()
        {
            List<string> inputMeasurementKeys = new List<string>();

            foreach (CurrentFlowPhasorGroup currentFlowPhasorGroup in m_currentFlows)
            {
                if (m_inputOutputSettings.AcceptsMeasurements)
                {
                    inputMeasurementKeys.Add(currentFlowPhasorGroup.PositiveSequence.Measurement.MagnitudeKey);
                    inputMeasurementKeys.Add(currentFlowPhasorGroup.PositiveSequence.Measurement.AngleKey);

                    if (m_phaseSelection == PhaseSelection.ThreePhase)
                    {
                        inputMeasurementKeys.Add(currentFlowPhasorGroup.PhaseA.Measurement.MagnitudeKey);
                        inputMeasurementKeys.Add(currentFlowPhasorGroup.PhaseA.Measurement.AngleKey);
                        inputMeasurementKeys.Add(currentFlowPhasorGroup.PhaseB.Measurement.MagnitudeKey);
                        inputMeasurementKeys.Add(currentFlowPhasorGroup.PhaseB.Measurement.AngleKey);
                        inputMeasurementKeys.Add(currentFlowPhasorGroup.PhaseC.Measurement.MagnitudeKey);
                        inputMeasurementKeys.Add(currentFlowPhasorGroup.PhaseC.Measurement.AngleKey);
                    }
                }

                if (m_inputOutputSettings.AcceptsEstimates)
                {
                    inputMeasurementKeys.Add(currentFlowPhasorGroup.PositiveSequence.Estimate.MagnitudeKey);
                    inputMeasurementKeys.Add(currentFlowPhasorGroup.PositiveSequence.Estimate.AngleKey);

                    if (m_phaseSelection == PhaseSelection.ThreePhase)
                    {
                        inputMeasurementKeys.Add(currentFlowPhasorGroup.PhaseA.Estimate.MagnitudeKey);
                        inputMeasurementKeys.Add(currentFlowPhasorGroup.PhaseA.Estimate.AngleKey);
                        inputMeasurementKeys.Add(currentFlowPhasorGroup.PhaseB.Estimate.MagnitudeKey);
                        inputMeasurementKeys.Add(currentFlowPhasorGroup.PhaseB.Estimate.AngleKey);
                        inputMeasurementKeys.Add(currentFlowPhasorGroup.PhaseC.Estimate.MagnitudeKey);
                        inputMeasurementKeys.Add(currentFlowPhasorGroup.PhaseC.Estimate.AngleKey);
                    }
                }
            }

            return inputMeasurementKeys;
        }

        private List<string> GetCurrentInjectionPhasorGroupInputKeys()
        {
            List<string> inputMeasurementKeys = new List<string>();

            foreach (CurrentInjectionPhasorGroup currentInjectionPhasorGroup in m_currentInjections)
            {
                if (m_inputOutputSettings.AcceptsMeasurements)
                {
                    inputMeasurementKeys.Add(currentInjectionPhasorGroup.PositiveSequence.Measurement.MagnitudeKey);
                    inputMeasurementKeys.Add(currentInjectionPhasorGroup.PositiveSequence.Measurement.AngleKey);

                    if (m_phaseSelection == PhaseSelection.ThreePhase)
                    {
                        inputMeasurementKeys.Add(currentInjectionPhasorGroup.PhaseA.Measurement.MagnitudeKey);
                        inputMeasurementKeys.Add(currentInjectionPhasorGroup.PhaseA.Measurement.AngleKey);
                        inputMeasurementKeys.Add(currentInjectionPhasorGroup.PhaseB.Measurement.MagnitudeKey);
                        inputMeasurementKeys.Add(currentInjectionPhasorGroup.PhaseB.Measurement.AngleKey);
                        inputMeasurementKeys.Add(currentInjectionPhasorGroup.PhaseC.Measurement.MagnitudeKey);
                        inputMeasurementKeys.Add(currentInjectionPhasorGroup.PhaseC.Measurement.AngleKey);
                    }
                }

                if (m_inputOutputSettings.AcceptsEstimates)
                {
                    inputMeasurementKeys.Add(currentInjectionPhasorGroup.PositiveSequence.Estimate.MagnitudeKey);
                    inputMeasurementKeys.Add(currentInjectionPhasorGroup.PositiveSequence.Estimate.AngleKey);

                    if (m_phaseSelection == PhaseSelection.ThreePhase)
                    {
                        inputMeasurementKeys.Add(currentInjectionPhasorGroup.PhaseA.Estimate.MagnitudeKey);
                        inputMeasurementKeys.Add(currentInjectionPhasorGroup.PhaseA.Estimate.AngleKey);
                        inputMeasurementKeys.Add(currentInjectionPhasorGroup.PhaseB.Estimate.MagnitudeKey);
                        inputMeasurementKeys.Add(currentInjectionPhasorGroup.PhaseB.Estimate.AngleKey);
                        inputMeasurementKeys.Add(currentInjectionPhasorGroup.PhaseC.Estimate.MagnitudeKey);
                        inputMeasurementKeys.Add(currentInjectionPhasorGroup.PhaseC.Estimate.AngleKey);
                    }
                }
            }

            return inputMeasurementKeys;
        }

        private List<string> GetBreakerStatusInputKeys()
        {
            List<string> inputMeasurementKeys = new List<string>();

            foreach (BreakerStatus breakerStatus in m_breakerStatuses)
            {
                inputMeasurementKeys.Add(breakerStatus.Key);
            }

            return inputMeasurementKeys;
        }

        private List<string> GetStatusWordInputKeys()
        {
            List<string> inputMeasurementKeys = new List<string>();

            foreach (StatusWord statusWord in m_statusWords)
            {
                inputMeasurementKeys.Add(statusWord.Key);
            }

            return inputMeasurementKeys;
        }

        private List<string> GetTransformerTapInputKeys()
        {
            List<string> inputMeasurementKeys = new List<string>();

            foreach (Transformer transformer in m_transformers)
            {
                inputMeasurementKeys.Add(transformer.TapPositionInputKey);
            }

            return inputMeasurementKeys;
        }

        /// <summary>
        /// Determines the <see cref="ObservationState"/> of each of the <see cref="Node"/> objects in the <see cref="NetworkModel"/>
        /// </summary>
        /// <param name="observedBusses">The set of <see cref="ObservedBus"/> objects to check the observability of.</param>
        /// <returns>The set of <see cref="ObservedBus"/> objects passed to function updated with the present <see cref="ObservationState"/>.</returns>
        private List<ObservedBus> CheckObservability(List<ObservedBus> observedBusses)
        {
            int numberOfDirectlyObservedNodes = 0;
            int numberOfIndirectlyObservedNodes = 0;

            // Mark the nodes that are directly observed by a voltage phasor
            foreach (ObservedBus observedBus in observedBusses)
            {
                if (m_phaseSelection == PhaseSelection.PositiveSequence)
                {
                    foreach (Node node in observedBus.Nodes)
                    {
                        if (node.Voltage.IncludeInPositiveSequenceEstimator)
                        {
                            node.Observability = ObservationState.DirectlyObserved;
                            numberOfDirectlyObservedNodes++;
                        }
                        else
                        {
                            node.Observability = ObservationState.Unobserved;
                        }
                    }
                }
                else if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    foreach (Node node in observedBus.Nodes)
                    {
                        if (node.Voltage.IncludeInEstimator)
                        {
                            node.Observability = ObservationState.DirectlyObserved;
                            numberOfDirectlyObservedNodes++;
                        }
                        else
                        {
                            node.Observability = ObservationState.Unobserved;
                        }
                    }
                }
            }

            // Mark the remaining nodes in the coherent group based on if they are observed with at least one current phasor
            foreach (ObservedBus observedBus in observedBusses)
            {
                foreach (Node node in observedBus.Nodes)
                {
                    if (node.Observability == ObservationState.Unobserved)
                    {
                        foreach (CurrentFlowPhasorGroup currentPhasorGroup in m_activeCurrentFlows)
                        {
                            if (currentPhasorGroup.MeasuredToNodeID == node.InternalID)
                            {
                                node.Observability = ObservationState.IndirectlyObserved;
                                numberOfIndirectlyObservedNodes++;
                            }
                        }
                    }
                }
            }

            // Mark the remaining nodes in the coherent group based on if their is at least one voltage phasor in on the bus
            foreach (ObservedBus observedBus in observedBusses)
            {
                foreach (Node node in observedBus.Nodes)
                {
                    if (numberOfDirectlyObservedNodes > 0 || numberOfIndirectlyObservedNodes > 0)
                    {
                        if (node.Observability != ObservationState.DirectlyObserved && node.Observability != ObservationState.IndirectlyObserved)
                        {
                            node.Observability = ObservationState.IndirectlyObserved;
                            numberOfIndirectlyObservedNodes++;
                        }
                    }
                }
            }

            if (numberOfDirectlyObservedNodes == 0 && numberOfIndirectlyObservedNodes == 0)
            {
                return null;
            }
            else
            {
                return observedBusses;
            }
        }

        /// <summary>
        /// Establishes the necessesary references for complex power calculations to occurr.
        /// </summary>
        private void InitializeComplexPowerCalculations()
        {
            foreach (Company company in m_companies)
            {
                foreach (Division division in company.Divisions)
                {
                    foreach (Substation substation in division.Substations)
                    {
                        foreach (Transformer transformer in substation.Transformers)
                        {
                            transformer.InitializeComplexPower();
                        }
                    }

                    foreach (TransmissionLine transmissionLine in division.TransmissionLines)
                    {
                        transmissionLine.InitializeComplexPower();
                    }
                }
            }
        }

        /// <summary>
        /// Takes the <see cref="Node.Voltage"/> from all of the <see cref="Node"/> objects in the model
        /// and collects them into one list to make updating values much easier.
        /// </summary>
        private void ListVoltagePhasors()
        {
            foreach (Company company in m_companies)
            {
                foreach (Division division in company.Divisions)
                {
                    foreach (Substation substation in division.Substations)
                    {
                        foreach (Node node in substation.Nodes)
                        {
                            m_voltages.Add(node.Voltage);
                        }
                    }
                    foreach (TransmissionLine transmissionLine in division.TransmissionLines)
                    {
                        foreach (Node node in transmissionLine.Nodes)
                        {
                            if (!m_voltages.Contains(node.Voltage))
                            {
                                m_voltages.Add(node.Voltage);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Takes the <see cref="Transformer.FromNodeCurrent"/>, <see cref="Transformer.ToNodeCurrent"/>,
        /// <see cref="TransmissionLine.FromSubstationCurrent"/>, and <see cref="TransmissionLine.ToSubstationCurrent"/>
        /// from all of the <see cref="Transformer"/> and <see cref="TransmissionLine"/> objects in the model
        /// and collects them into one list to make updating values much easier.
        /// </summary>
        private void ListCurrentPhasors()
        {
            foreach (Company company in m_companies)
            {
                foreach (Division division in company.Divisions)
                {
                    foreach (Substation substation in division.Substations)
                    {
                        foreach (Transformer transformer in substation.Transformers)
                        {
                            // Add the current phasor groups to the list
                            m_currentFlows.Add(transformer.FromNodeCurrent);
                            m_currentFlows.Add(transformer.ToNodeCurrent);

                            // Set the measured branch of each current phasor group
                            transformer.FromNodeCurrent.MeasuredBranch = transformer;
                            transformer.ToNodeCurrent.MeasuredBranch = transformer;
                        }
                        foreach (ShuntCompensator shunt in substation.Shunts)
                        {
                            m_currentInjections.Add(shunt.Current);

                            shunt.Current.MeasuredBranch = shunt;
                        }
                    }
                    foreach (TransmissionLine transmissionLine in division.TransmissionLines)
                    {

                        // Add the current phasor groups to the list
                        m_currentFlows.Add(transmissionLine.FromSubstationCurrent);
                        m_currentFlows.Add(transmissionLine.ToSubstationCurrent);

                        // Set the measured branch of each current phasor group
                        transmissionLine.FromSubstationCurrent.MeasuredBranch = transmissionLine;
                        transmissionLine.ToSubstationCurrent.MeasuredBranch = transmissionLine;
                    }
                }
            }
        }

        /// <summary>
        /// Maps the raw voltage measurement values to their virtual counterpart
        /// </summary>
        private void InsertVoltageMeasurements()
        {
            foreach (VoltagePhasorGroup voltagePhasorGroup in m_voltages)
            {
                double value = 0;

                if (m_inputOutputSettings.AcceptsMeasurements)
                {
                    //                                    Positive Sequence Magnitude Measurement
                    if (m_rawMeasurementKeyValuePairs.TryGetValue(voltagePhasorGroup.PositiveSequence.Measurement.MagnitudeKey, out value))
                    {
                        voltagePhasorGroup.PositiveSequence.Measurement.Magnitude = value;
                        value = 0;
                    }

                    //                                    Positive Sequence Angle Measurement
                    if (m_rawMeasurementKeyValuePairs.TryGetValue(voltagePhasorGroup.PositiveSequence.Measurement.AngleKey, out value))
                    {
                        voltagePhasorGroup.PositiveSequence.Measurement.AngleInDegrees = value;
                        value = 0;
                    }

                    if (m_phaseSelection == PhaseSelection.ThreePhase)
                    {
                        //                                    Phase A Magnitude Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(voltagePhasorGroup.PhaseA.Measurement.MagnitudeKey, out value))
                        {
                            voltagePhasorGroup.PhaseA.Measurement.Magnitude = value;
                            value = 0;
                        }

                        //                                    Phase A Angle Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(voltagePhasorGroup.PhaseA.Measurement.AngleKey, out value))
                        {
                            voltagePhasorGroup.PhaseA.Measurement.AngleInDegrees = value;
                            value = 0;
                        }

                        //                                    Phase B Magnitude Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(voltagePhasorGroup.PhaseB.Measurement.MagnitudeKey, out value))
                        {
                            voltagePhasorGroup.PhaseB.Measurement.Magnitude = value;
                            value = 0;
                        }

                        //                                    Phase B Angle Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(voltagePhasorGroup.PhaseB.Measurement.AngleKey, out value))
                        {
                            voltagePhasorGroup.PhaseB.Measurement.AngleInDegrees = value;
                            value = 0;
                        }

                        //                                    Phase C Magnitude Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(voltagePhasorGroup.PhaseC.Measurement.MagnitudeKey, out value))
                        {
                            voltagePhasorGroup.PhaseC.Measurement.Magnitude = value;
                            value = 0;
                        }

                        //                                    Phase C Angle Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(voltagePhasorGroup.PhaseC.Measurement.AngleKey, out value))
                        {
                            voltagePhasorGroup.PhaseC.Measurement.AngleInDegrees = value;
                            value = 0;
                        }
                    }
                }

                if (m_inputOutputSettings.AcceptsEstimates)
                {
                    //                                    Positive Sequence Magnitude Estimate
                    if (m_rawMeasurementKeyValuePairs.TryGetValue(voltagePhasorGroup.PositiveSequence.Estimate.MagnitudeKey, out value))
                    {
                        voltagePhasorGroup.PositiveSequence.Estimate.Magnitude = value;
                        value = 0;
                    }

                    //                                    Positive Sequence Angle Estimate
                    if (m_rawMeasurementKeyValuePairs.TryGetValue(voltagePhasorGroup.PositiveSequence.Estimate.AngleKey, out value))
                    {
                        voltagePhasorGroup.PositiveSequence.Estimate.AngleInDegrees = value;
                        value = 0;
                    }

                    if (m_phaseSelection == PhaseSelection.ThreePhase)
                    {
                        //                                    Phase A Magnitude Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(voltagePhasorGroup.PhaseA.Estimate.MagnitudeKey, out value))
                        {
                            voltagePhasorGroup.PhaseA.Estimate.Magnitude = value;
                            value = 0;
                        }


                        //                                    Phase A Angle Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(voltagePhasorGroup.PhaseA.Estimate.AngleKey, out value))
                        {
                            voltagePhasorGroup.PhaseA.Estimate.AngleInDegrees = value;
                            value = 0;
                        }


                        //                                    Phase B Magnitude Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(voltagePhasorGroup.PhaseB.Estimate.MagnitudeKey, out value))
                        {
                            voltagePhasorGroup.PhaseB.Estimate.Magnitude = value;
                            value = 0;
                        }



                        //                                    Phase B Angle Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(voltagePhasorGroup.PhaseB.Estimate.AngleKey, out value))
                        {
                            voltagePhasorGroup.PhaseB.Estimate.AngleInDegrees = value;
                            value = 0;
                        }


                        //                                    Phase C Magnitude Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(voltagePhasorGroup.PhaseC.Estimate.MagnitudeKey, out value))
                        {
                            voltagePhasorGroup.PhaseC.Estimate.Magnitude = value;
                            value = 0;
                        }


                        //                                    Phase C Angle Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(voltagePhasorGroup.PhaseC.Estimate.AngleKey, out value))
                        {
                            voltagePhasorGroup.PhaseC.Estimate.AngleInDegrees = value;
                            value = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Maps the raw current measurement values to their virtual counterpart
        /// </summary>
        private void InsertCurrentFlowMeasurements()
        {
            foreach (CurrentFlowPhasorGroup currentPhasorGroup in m_currentFlows)
            {
                double value = 0;

                if (m_inputOutputSettings.AcceptsMeasurements)
                {
                    //                                    Positive Sequence Magnitude Measurement
                    if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PositiveSequence.Measurement.MagnitudeKey, out value))
                    {
                        currentPhasorGroup.PositiveSequence.Measurement.Magnitude = value;
                        value = 0;
                    }

                    //                                    Positive Sequence Angle Measurement
                    if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PositiveSequence.Measurement.AngleKey, out value))
                    {
                        currentPhasorGroup.PositiveSequence.Measurement.AngleInDegrees = value;
                        value = 0;
                    }

                    if (m_phaseSelection == PhaseSelection.ThreePhase)
                    {
                        //                                    Phase A Magnitude Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseA.Measurement.MagnitudeKey, out value))
                        {
                            currentPhasorGroup.PhaseA.Measurement.Magnitude = value;
                            value = 0;
                        }

                        //                                    Phase A Angle Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseA.Measurement.AngleKey, out value))
                        {
                            currentPhasorGroup.PhaseA.Measurement.AngleInDegrees = value;
                            value = 0;
                        }

                        //                                    Phase B Magnitude Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseB.Measurement.MagnitudeKey, out value))
                        {
                            currentPhasorGroup.PhaseB.Measurement.Magnitude = value;
                            value = 0;
                        }

                        //                                    Phase B Angle Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseB.Measurement.AngleKey, out value))
                        {
                            currentPhasorGroup.PhaseB.Measurement.AngleInDegrees = value;
                            value = 0;
                        }

                        //                                    Phase C Magnitude Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseC.Measurement.MagnitudeKey, out value))
                        {
                            currentPhasorGroup.PhaseC.Measurement.Magnitude = value;
                            value = 0;
                        }

                        //                                    Phase C Angle Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseC.Measurement.AngleKey, out value))
                        {
                            currentPhasorGroup.PhaseC.Measurement.AngleInDegrees = value;
                            value = 0;
                        }
                    }
                }


                if (m_inputOutputSettings.AcceptsEstimates)
                {
                //                                    Positive Sequence Magnitude Estimate
                    if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PositiveSequence.Estimate.MagnitudeKey, out value))
                    {
                        currentPhasorGroup.PositiveSequence.Estimate.Magnitude = value;
                        value = 0;
                    }

                    //                                    Positive Sequence Angle Estimate
                    if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PositiveSequence.Estimate.AngleKey, out value))
                    {
                        currentPhasorGroup.PositiveSequence.Estimate.AngleInDegrees = value;
                        value = 0;
                    }


                    if (m_phaseSelection == PhaseSelection.ThreePhase)
                    {
                        //                                    Phase A Magnitude Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseA.Estimate.MagnitudeKey, out value))
                        {
                            currentPhasorGroup.PhaseA.Estimate.Magnitude = value;
                            value = 0;
                        }

                        //                                    Phase A Angle Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseA.Estimate.AngleKey, out value))
                        {
                            currentPhasorGroup.PhaseA.Estimate.AngleInDegrees = value;
                            value = 0;
                        }

                        //                                    Phase B Magnitude Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseB.Estimate.MagnitudeKey, out value))
                        {
                            currentPhasorGroup.PhaseB.Estimate.Magnitude = value;
                            value = 0;
                        }

                        //                                    Phase B Angle Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseB.Estimate.AngleKey, out value))
                        {
                            currentPhasorGroup.PhaseB.Estimate.AngleInDegrees = value;
                            value = 0;
                        }

                        //                                    Phase C Magnitude Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseC.Estimate.MagnitudeKey, out value))
                        {
                            currentPhasorGroup.PhaseC.Estimate.Magnitude = value;
                            value = 0;
                        }

                        //                                    Phase C Angle Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseC.Estimate.AngleKey, out value))
                        {
                            currentPhasorGroup.PhaseC.Estimate.AngleInDegrees = value;
                            value = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Maps the raw current measurement values to their virtual counterpart
        /// </summary>
        private void InsertCurrentInjectionMeasurements()
        {
            foreach (CurrentInjectionPhasorGroup currentPhasorGroup in m_currentInjections)
            {
                double value = 0;

                if (m_inputOutputSettings.AcceptsMeasurements)
                {
                    //                                    Positive Sequence Magnitude Measurement
                    if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PositiveSequence.Measurement.MagnitudeKey, out value))
                    {
                        currentPhasorGroup.PositiveSequence.Measurement.Magnitude = value;
                        value = 0;
                    }

                    //                                    Positive Sequence Angle Measurement
                    if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PositiveSequence.Measurement.AngleKey, out value))
                    {
                        currentPhasorGroup.PositiveSequence.Measurement.AngleInDegrees = value;
                        value = 0;
                    }

                    if (m_phaseSelection == PhaseSelection.ThreePhase)
                    {
                        //                                    Phase A Magnitude Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseA.Measurement.MagnitudeKey, out value))
                        {
                            currentPhasorGroup.PhaseA.Measurement.Magnitude = value;
                            value = 0;
                        }

                        //                                    Phase A Angle Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseA.Measurement.AngleKey, out value))
                        {
                            currentPhasorGroup.PhaseA.Measurement.AngleInDegrees = value;
                            value = 0;
                        }

                        //                                    Phase B Magnitude Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseB.Measurement.MagnitudeKey, out value))
                        {
                            currentPhasorGroup.PhaseB.Measurement.Magnitude = value;
                            value = 0;
                        }

                        //                                    Phase B Angle Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseB.Measurement.AngleKey, out value))
                        {
                            currentPhasorGroup.PhaseB.Measurement.AngleInDegrees = value;
                            value = 0;
                        }

                        //                                    Phase C Magnitude Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseC.Measurement.MagnitudeKey, out value))
                        {
                            currentPhasorGroup.PhaseC.Measurement.Magnitude = value;
                            value = 0;
                        }

                        //                                    Phase C Angle Measurement
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseC.Measurement.AngleKey, out value))
                        {
                            currentPhasorGroup.PhaseC.Measurement.AngleInDegrees = value;
                            value = 0;
                        }
                    }
                }


                if (m_inputOutputSettings.AcceptsEstimates)
                {
                    //                                    Positive Sequence Magnitude Estimate
                    if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PositiveSequence.Estimate.MagnitudeKey, out value))
                    {
                        currentPhasorGroup.PositiveSequence.Estimate.Magnitude = value;
                        value = 0;
                    }

                    //                                    Positive Sequence Angle Estimate
                    if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PositiveSequence.Estimate.AngleKey, out value))
                    {
                        currentPhasorGroup.PositiveSequence.Estimate.AngleInDegrees = value;
                        value = 0;
                    }


                    if (m_phaseSelection == PhaseSelection.ThreePhase)
                    {
                        //                                    Phase A Magnitude Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseA.Estimate.MagnitudeKey, out value))
                        {
                            currentPhasorGroup.PhaseA.Estimate.Magnitude = value;
                            value = 0;
                        }

                        //                                    Phase A Angle Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseA.Estimate.AngleKey, out value))
                        {
                            currentPhasorGroup.PhaseA.Estimate.AngleInDegrees = value;
                            value = 0;
                        }

                        //                                    Phase B Magnitude Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseB.Estimate.MagnitudeKey, out value))
                        {
                            currentPhasorGroup.PhaseB.Estimate.Magnitude = value;
                            value = 0;
                        }

                        //                                    Phase B Angle Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseB.Estimate.AngleKey, out value))
                        {
                            currentPhasorGroup.PhaseB.Estimate.AngleInDegrees = value;
                            value = 0;
                        }

                        //                                    Phase C Magnitude Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseC.Estimate.MagnitudeKey, out value))
                        {
                            currentPhasorGroup.PhaseC.Estimate.Magnitude = value;
                            value = 0;
                        }

                        //                                    Phase C Angle Estimate
                        if (m_rawMeasurementKeyValuePairs.TryGetValue(currentPhasorGroup.PhaseC.Estimate.AngleKey, out value))
                        {
                            currentPhasorGroup.PhaseC.Estimate.AngleInDegrees = value;
                            value = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Maps the raw digital values to their virtual counterpart
        /// </summary>
        private void InsertBreakerStatuses()
        {
            foreach (BreakerStatus breakerStatus in m_breakerStatuses)
            {
                double value = 0;
                // Breaker Statuses
                if (m_rawMeasurementKeyValuePairs.TryGetValue(breakerStatus.Key, out value))
                {
                    breakerStatus.BinaryValue = Convert.ToInt32(value);
                    value = 0;
                }
            }
        }

        /// <summary>
        /// Maps the raw status word values to their virtual counterpart
        /// </summary>
        private void InsertStatusWords()
        {
            foreach (StatusWord statusWord in m_statusWords)
            {
                double value = 0;

                if (m_rawMeasurementKeyValuePairs.TryGetValue(statusWord.Key, out value))
                {
                    statusWord.BinaryValue = value;
                    value = 0;
                }
            }
        }

        /// <summary>
        /// Maps the raw tap position values to their virtual counterpart
        /// </summary>
        private void InsertTransformerTapPositions()
        {
            foreach (Transformer transformer in m_transformers)
            {
                double value = 0;

                if (m_rawMeasurementKeyValuePairs.TryGetValue(transformer.TapPositionInputKey, out value))
                {
                    transformer.TapPositionMeasurement = Convert.ToInt32(value);
                }
            }
        }

        /// <summary>
        /// Adds the estimated voltage phasor components to the output with their associated keys.
        /// </summary>
        /// <param name="output">The Dictionary that contains the output keys and values.</param>
        private void AddVoltageEstimatesToOutput(Dictionary<string, double> output)
        {
            foreach (VoltagePhasorGroup voltagePhasorGroup in m_voltages)
            {
                if (!output.ContainsKey(voltagePhasorGroup.NegativeSequenceToPositiveSequenceRatioMeasurementKey))
                {
                    output.Add(voltagePhasorGroup.NegativeSequenceToPositiveSequenceRatioMeasurementKey, voltagePhasorGroup.EstimatedNegativeSequenceToPositiveSequenceRatio);
                }

                if (!output.ContainsKey(voltagePhasorGroup.PositiveSequence.Estimate.MagnitudeKey))
                {
                    output.Add(voltagePhasorGroup.PositiveSequence.Estimate.MagnitudeKey, voltagePhasorGroup.PositiveSequence.Estimate.Magnitude);
                }
                if (!output.ContainsKey(voltagePhasorGroup.PositiveSequence.Estimate.AngleKey))
                {
                    output.Add(voltagePhasorGroup.PositiveSequence.Estimate.AngleKey, voltagePhasorGroup.PositiveSequence.Estimate.AngleInDegrees);
                }

                if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    if (!output.ContainsKey(voltagePhasorGroup.PhaseA.Estimate.MagnitudeKey))
                    {
                        output.Add(voltagePhasorGroup.PhaseA.Estimate.MagnitudeKey, voltagePhasorGroup.PhaseA.Estimate.Magnitude);
                    }
                    if (!output.ContainsKey(voltagePhasorGroup.PhaseA.Estimate.AngleKey))
                    {
                        output.Add(voltagePhasorGroup.PhaseA.Estimate.AngleKey, voltagePhasorGroup.PhaseA.Estimate.AngleInDegrees);
                    }

                    if (!output.ContainsKey(voltagePhasorGroup.PhaseB.Estimate.MagnitudeKey))
                    {
                        output.Add(voltagePhasorGroup.PhaseB.Estimate.MagnitudeKey, voltagePhasorGroup.PhaseB.Estimate.Magnitude);
                    }
                    if (!output.ContainsKey(voltagePhasorGroup.PhaseB.Estimate.AngleKey))
                    {
                        output.Add(voltagePhasorGroup.PhaseB.Estimate.AngleKey, voltagePhasorGroup.PhaseB.Estimate.AngleInDegrees);
                    }

                    if (!output.ContainsKey(voltagePhasorGroup.PhaseC.Estimate.MagnitudeKey))
                    {
                        output.Add(voltagePhasorGroup.PhaseC.Estimate.MagnitudeKey, voltagePhasorGroup.PhaseC.Estimate.Magnitude);
                    }
                    if (!output.ContainsKey(voltagePhasorGroup.PhaseC.Estimate.AngleKey))
                    {
                        output.Add(voltagePhasorGroup.PhaseC.Estimate.AngleKey, voltagePhasorGroup.PhaseC.Estimate.AngleInDegrees);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the estimated current flow phasor components to the output with their associated keys.
        /// </summary>
        /// <param name="output">The Dictionary that contains the output keys and values.</param>
        private void AddCurrentFlowEstimatesToOutput(Dictionary<string, double> output)
        {
            foreach (CurrentFlowPhasorGroup currentFlowPhasorGroup in m_currentFlows)
            {
                if (!output.ContainsKey(currentFlowPhasorGroup.NegativeSequenceToPositiveSequenceRatioMeasurementKey))
                {
                    output.Add(currentFlowPhasorGroup.NegativeSequenceToPositiveSequenceRatioMeasurementKey, currentFlowPhasorGroup.EstimatedNegativeSequenceToPositiveSequenceRatio);
                }

                if (!output.ContainsKey(currentFlowPhasorGroup.PositiveSequence.Estimate.MagnitudeKey))
                {
                    output.Add(currentFlowPhasorGroup.PositiveSequence.Estimate.MagnitudeKey, currentFlowPhasorGroup.PositiveSequence.Estimate.Magnitude);
                }
                if (!output.ContainsKey(currentFlowPhasorGroup.PositiveSequence.Estimate.AngleKey))
                {
                    output.Add(currentFlowPhasorGroup.PositiveSequence.Estimate.AngleKey, currentFlowPhasorGroup.PositiveSequence.Estimate.AngleInDegrees);
                }

                if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    if (!output.ContainsKey(currentFlowPhasorGroup.PhaseA.Estimate.MagnitudeKey))
                    {
                        output.Add(currentFlowPhasorGroup.PhaseA.Estimate.MagnitudeKey, currentFlowPhasorGroup.PhaseA.Estimate.Magnitude);
                    }
                    if (!output.ContainsKey(currentFlowPhasorGroup.PhaseA.Estimate.AngleKey))
                    {
                        output.Add(currentFlowPhasorGroup.PhaseA.Estimate.AngleKey, currentFlowPhasorGroup.PhaseA.Estimate.AngleInDegrees);
                    }

                    if (!output.ContainsKey(currentFlowPhasorGroup.PhaseB.Estimate.MagnitudeKey))
                    {
                        output.Add(currentFlowPhasorGroup.PhaseB.Estimate.MagnitudeKey, currentFlowPhasorGroup.PhaseB.Estimate.Magnitude);
                    }
                    if (!output.ContainsKey(currentFlowPhasorGroup.PhaseB.Estimate.AngleKey))
                    {
                        output.Add(currentFlowPhasorGroup.PhaseB.Estimate.AngleKey, currentFlowPhasorGroup.PhaseB.Estimate.AngleInDegrees);
                    }

                    if (!output.ContainsKey(currentFlowPhasorGroup.PhaseC.Estimate.MagnitudeKey))
                    {
                        output.Add(currentFlowPhasorGroup.PhaseC.Estimate.MagnitudeKey, currentFlowPhasorGroup.PhaseC.Estimate.Magnitude);
                    }
                    if (!output.ContainsKey(currentFlowPhasorGroup.PhaseC.Estimate.AngleKey))
                    {
                        output.Add(currentFlowPhasorGroup.PhaseC.Estimate.AngleKey, currentFlowPhasorGroup.PhaseC.Estimate.AngleInDegrees);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the estimated current injection phasor components to the output with their associated keys.
        /// </summary>
        /// <param name="output">The Dictionary that contains the output keys and values.</param>
        private void AddCurrentInjectionsToOutput(Dictionary<string, double> output)
        {
            foreach (CurrentInjectionPhasorGroup currentInjectionPhasorGroup in m_currentInjections)
            {
                if (!output.ContainsKey(currentInjectionPhasorGroup.NegativeSequenceToPositiveSequenceRatioMeasurementKey))
                {
                    output.Add(currentInjectionPhasorGroup.NegativeSequenceToPositiveSequenceRatioMeasurementKey, currentInjectionPhasorGroup.EstimatedNegativeSequenceToPositiveSequenceRatio);
                }

                if (!output.ContainsKey(currentInjectionPhasorGroup.PositiveSequence.Estimate.MagnitudeKey))
                {
                    output.Add(currentInjectionPhasorGroup.PositiveSequence.Estimate.MagnitudeKey, currentInjectionPhasorGroup.PositiveSequence.Estimate.Magnitude);
                }
                if (!output.ContainsKey(currentInjectionPhasorGroup.PositiveSequence.Estimate.AngleKey))
                {
                    output.Add(currentInjectionPhasorGroup.PositiveSequence.Estimate.AngleKey, currentInjectionPhasorGroup.PositiveSequence.Estimate.AngleInDegrees);
                }

                if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    if (!output.ContainsKey(currentInjectionPhasorGroup.PhaseA.Estimate.MagnitudeKey))
                    {
                        output.Add(currentInjectionPhasorGroup.PhaseA.Estimate.MagnitudeKey, currentInjectionPhasorGroup.PhaseA.Estimate.Magnitude);
                    }
                    if (!output.ContainsKey(currentInjectionPhasorGroup.PhaseA.Estimate.AngleKey))
                    {
                        output.Add(currentInjectionPhasorGroup.PhaseA.Estimate.AngleKey, currentInjectionPhasorGroup.PhaseA.Estimate.AngleInDegrees);
                    }

                    if (!output.ContainsKey(currentInjectionPhasorGroup.PhaseB.Estimate.MagnitudeKey))
                    {
                        output.Add(currentInjectionPhasorGroup.PhaseB.Estimate.MagnitudeKey, currentInjectionPhasorGroup.PhaseB.Estimate.Magnitude);
                    }
                    if (!output.ContainsKey(currentInjectionPhasorGroup.PhaseB.Estimate.AngleKey))
                    {
                        output.Add(currentInjectionPhasorGroup.PhaseB.Estimate.AngleKey, currentInjectionPhasorGroup.PhaseB.Estimate.AngleInDegrees);
                    }

                    if (!output.ContainsKey(currentInjectionPhasorGroup.PhaseC.Estimate.MagnitudeKey))
                    {
                        output.Add(currentInjectionPhasorGroup.PhaseC.Estimate.MagnitudeKey, currentInjectionPhasorGroup.PhaseC.Estimate.Magnitude);
                    }
                    if (!output.ContainsKey(currentInjectionPhasorGroup.PhaseC.Estimate.AngleKey))
                    {
                        output.Add(currentInjectionPhasorGroup.PhaseC.Estimate.AngleKey, currentInjectionPhasorGroup.PhaseC.Estimate.AngleInDegrees);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the voltage measurement residual components to the output with their associated keys.
        /// </summary>
        /// <param name="output">The Dictionary that contains the output keys and values.</param>
        private void AddVoltageMeasurementResidualsToOutput(Dictionary<string, double> output)
        {
            foreach (VoltagePhasorGroup voltagePhasorGroup in m_voltages)
            {
                if (!output.ContainsKey(voltagePhasorGroup.PositiveSequence.MagnitudeResidualKey))
                {
                    output.Add(voltagePhasorGroup.PositiveSequence.MagnitudeResidualKey, voltagePhasorGroup.PositiveSequence.MagnitudeResidual);
                }
                if (!output.ContainsKey(voltagePhasorGroup.PositiveSequence.AngleResidualKey))
                {
                    output.Add(voltagePhasorGroup.PositiveSequence.AngleResidualKey, voltagePhasorGroup.PositiveSequence.AngleResidualInDegrees);
                }

                if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    if (!output.ContainsKey(voltagePhasorGroup.PhaseA.MagnitudeResidualKey))
                    {
                        output.Add(voltagePhasorGroup.PhaseA.MagnitudeResidualKey, voltagePhasorGroup.PhaseA.MagnitudeResidual);
                    }
                    if (!output.ContainsKey(voltagePhasorGroup.PhaseA.AngleResidualKey))
                    {
                        output.Add(voltagePhasorGroup.PhaseA.AngleResidualKey, voltagePhasorGroup.PhaseA.AngleResidualInDegrees);
                    }

                    if (!output.ContainsKey(voltagePhasorGroup.PhaseB.MagnitudeResidualKey))
                    {
                        output.Add(voltagePhasorGroup.PhaseB.MagnitudeResidualKey, voltagePhasorGroup.PhaseB.MagnitudeResidual);
                    }
                    if (!output.ContainsKey(voltagePhasorGroup.PhaseB.AngleResidualKey))
                    {
                        output.Add(voltagePhasorGroup.PhaseB.AngleResidualKey, voltagePhasorGroup.PhaseB.AngleResidualInDegrees);
                    }

                    if (!output.ContainsKey(voltagePhasorGroup.PhaseC.MagnitudeResidualKey))
                    {
                        output.Add(voltagePhasorGroup.PhaseC.MagnitudeResidualKey, voltagePhasorGroup.PhaseC.MagnitudeResidual);
                    }
                    if (!output.ContainsKey(voltagePhasorGroup.PhaseC.AngleResidualKey))
                    {
                        output.Add(voltagePhasorGroup.PhaseC.AngleResidualKey, voltagePhasorGroup.PhaseC.AngleResidualInDegrees);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the current measurement residual components to the output with their associated keys.
        /// </summary>
        /// <param name="output">The Dictionary that contains the output keys and values.</param>
        private void AddCurrentMeasurementResidualsToOutput(Dictionary<string, double> output)
        {
            foreach (CurrentFlowPhasorGroup currentPhasorGroup in m_currentFlows)
            {
                if (!output.ContainsKey(currentPhasorGroup.PositiveSequence.MagnitudeResidualKey))
                {
                    output.Add(currentPhasorGroup.PositiveSequence.MagnitudeResidualKey, currentPhasorGroup.PositiveSequence.MagnitudeResidual);
                }
                if (!output.ContainsKey(currentPhasorGroup.PositiveSequence.AngleResidualKey))
                {
                    output.Add(currentPhasorGroup.PositiveSequence.AngleResidualKey, currentPhasorGroup.PositiveSequence.AngleResidualInDegrees);
                }

                if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    if (!output.ContainsKey(currentPhasorGroup.PhaseA.MagnitudeResidualKey))
                    {
                        output.Add(currentPhasorGroup.PhaseA.MagnitudeResidualKey, currentPhasorGroup.PhaseA.MagnitudeResidual);
                    }
                    if (!output.ContainsKey(currentPhasorGroup.PhaseA.AngleResidualKey))
                    {
                        output.Add(currentPhasorGroup.PhaseA.AngleResidualKey, currentPhasorGroup.PhaseA.AngleResidualInDegrees);
                    }

                    if (!output.ContainsKey(currentPhasorGroup.PhaseB.MagnitudeResidualKey))
                    {
                        output.Add(currentPhasorGroup.PhaseB.MagnitudeResidualKey, currentPhasorGroup.PhaseB.MagnitudeResidual);
                    }
                    if (!output.ContainsKey(currentPhasorGroup.PhaseB.AngleResidualKey))
                    {
                        output.Add(currentPhasorGroup.PhaseB.AngleResidualKey, currentPhasorGroup.PhaseB.AngleResidualInDegrees);
                    }

                    if (!output.ContainsKey(currentPhasorGroup.PhaseC.MagnitudeResidualKey))
                    {
                        output.Add(currentPhasorGroup.PhaseC.MagnitudeResidualKey, currentPhasorGroup.PhaseC.MagnitudeResidual);
                    }
                    if (!output.ContainsKey(currentPhasorGroup.PhaseC.AngleResidualKey))
                    {
                        output.Add(currentPhasorGroup.PhaseC.AngleResidualKey, currentPhasorGroup.PhaseC.AngleResidualInDegrees);
                    }
                }
            }
        }
        
        /// <summary>
        /// Adds the status of the circuit breakers with their associated keys.
        /// </summary>
        /// <param name="output">The Dictionary that contains the output keys and values.</param>
        private void AddCircuitBreakerStatusesToOutput(Dictionary<string, double> output)
        {
            foreach (SwitchingDeviceBase switchingDevice in m_switchingDevices)
            {
                if (switchingDevice is CircuitBreaker)
                {
                    if (!output.ContainsKey(switchingDevice.MeasurementKey))
                    {
                        output.Add(switchingDevice.MeasurementKey, (int)switchingDevice.ActualState);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the status of the circuit switches with their associated keys.
        /// </summary>
        /// <param name="output">The Dictionary that contains the output keys and values.</param>
        private void AddSwitchStatusesToOutput(Dictionary<string, double> output)
        {
            foreach (SwitchingDeviceBase switchingDevice in m_switchingDevices)
            {
                if (switchingDevice is Switch)
                {
                    if (!output.ContainsKey(switchingDevice.MeasurementKey))
                    {
                        output.Add(switchingDevice.MeasurementKey, (int)switchingDevice.ActualState);
                    }
                }
            }
        }

        private void ComputeVoltageSequenceValues()
        {
            foreach (VoltagePhasorGroup voltagePhasorGroup in m_voltages)
            {
                voltagePhasorGroup.ComputeSequenceComponents();
            }
        }

        private void ComputeCurrentFlowSequenceValues()
        {
            foreach (CurrentFlowPhasorGroup currentFlowPhasorGroup in m_currentFlows)
            {
                currentFlowPhasorGroup.ComputeSequenceComponents();
            }
        }

        private void ComputeCurrentInjectionSequenceValues()
        {
            foreach (CurrentInjectionPhasorGroup currentInjectionPhasorGroup in m_currentInjections)
            {
                currentInjectionPhasorGroup.ComputeSequenceComponents();
            }
        }

        private void AppendReceivedVoltageMeasurements(Dictionary<string, double> receivedMeasurements)
        {
            foreach (VoltagePhasorGroup voltagePhasorGroup in m_voltages)
            {
                if (m_phaseSelection == PhaseSelection.PositiveSequence)
                {
                    if (voltagePhasorGroup.PositiveSequence.Measurement.MagnitudeValueWasReported)
                    {
                        receivedMeasurements.Add(voltagePhasorGroup.PositiveSequence.Measurement.MagnitudeKey, voltagePhasorGroup.PositiveSequence.Measurement.Magnitude);
                    }
                    if (voltagePhasorGroup.PositiveSequence.Measurement.AngleValueWasReported)
                    {
                        receivedMeasurements.Add(voltagePhasorGroup.PositiveSequence.Measurement.AngleKey, voltagePhasorGroup.PositiveSequence.Measurement.AngleInDegrees);
                    }
                }
                else if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    if (voltagePhasorGroup.PhaseA.Measurement.MagnitudeValueWasReported)
                    {
                        receivedMeasurements.Add(voltagePhasorGroup.PositiveSequence.Measurement.MagnitudeKey, voltagePhasorGroup.PositiveSequence.Measurement.Magnitude);
                    }
                    if (voltagePhasorGroup.PhaseA.Measurement.AngleValueWasReported)
                    {
                        receivedMeasurements.Add(voltagePhasorGroup.PositiveSequence.Measurement.AngleKey, voltagePhasorGroup.PositiveSequence.Measurement.AngleInDegrees);
                    }
                    if (voltagePhasorGroup.PhaseB.Measurement.MagnitudeValueWasReported)
                    {
                        receivedMeasurements.Add(voltagePhasorGroup.PositiveSequence.Measurement.MagnitudeKey, voltagePhasorGroup.PositiveSequence.Measurement.Magnitude);
                    }
                    if (voltagePhasorGroup.PhaseB.Measurement.AngleValueWasReported)
                    {
                        receivedMeasurements.Add(voltagePhasorGroup.PositiveSequence.Measurement.AngleKey, voltagePhasorGroup.PositiveSequence.Measurement.AngleInDegrees);
                    }
                    if (voltagePhasorGroup.PhaseC.Measurement.MagnitudeValueWasReported)
                    {
                        receivedMeasurements.Add(voltagePhasorGroup.PositiveSequence.Measurement.MagnitudeKey, voltagePhasorGroup.PositiveSequence.Measurement.Magnitude);
                    }
                    if (voltagePhasorGroup.PhaseC.Measurement.AngleValueWasReported)
                    {
                        receivedMeasurements.Add(voltagePhasorGroup.PositiveSequence.Measurement.AngleKey, voltagePhasorGroup.PositiveSequence.Measurement.AngleInDegrees);
                    }
                }
            }
        }

        private void AppendReceivedCurrentFlowMeasurements(Dictionary<string, double> receivedMeasurements)
        {
            foreach (CurrentFlowPhasorGroup currentFlowPhasorGroup in m_currentFlows)
            {
                if (m_phaseSelection == PhaseSelection.PositiveSequence)
                {
                    if (currentFlowPhasorGroup.PositiveSequence.Measurement.MagnitudeValueWasReported)
                    {
                        receivedMeasurements.Add(currentFlowPhasorGroup.PositiveSequence.Measurement.MagnitudeKey, currentFlowPhasorGroup.PositiveSequence.Measurement.Magnitude);
                    }
                    if (currentFlowPhasorGroup.PositiveSequence.Measurement.AngleValueWasReported)
                    {
                        receivedMeasurements.Add(currentFlowPhasorGroup.PositiveSequence.Measurement.AngleKey, currentFlowPhasorGroup.PositiveSequence.Measurement.AngleInDegrees);
                    }
                }
                else if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    if (currentFlowPhasorGroup.PhaseA.Measurement.MagnitudeValueWasReported)
                    {
                        receivedMeasurements.Add(currentFlowPhasorGroup.PositiveSequence.Measurement.MagnitudeKey, currentFlowPhasorGroup.PositiveSequence.Measurement.Magnitude);
                    }
                    if (currentFlowPhasorGroup.PhaseA.Measurement.AngleValueWasReported)
                    {
                        receivedMeasurements.Add(currentFlowPhasorGroup.PositiveSequence.Measurement.AngleKey, currentFlowPhasorGroup.PositiveSequence.Measurement.AngleInDegrees);
                    }
                    if (currentFlowPhasorGroup.PhaseB.Measurement.MagnitudeValueWasReported)
                    {
                        receivedMeasurements.Add(currentFlowPhasorGroup.PositiveSequence.Measurement.MagnitudeKey, currentFlowPhasorGroup.PositiveSequence.Measurement.Magnitude);
                    }
                    if (currentFlowPhasorGroup.PhaseB.Measurement.AngleValueWasReported)
                    {
                        receivedMeasurements.Add(currentFlowPhasorGroup.PositiveSequence.Measurement.AngleKey, currentFlowPhasorGroup.PositiveSequence.Measurement.AngleInDegrees);
                    }
                    if (currentFlowPhasorGroup.PhaseC.Measurement.MagnitudeValueWasReported)
                    {
                        receivedMeasurements.Add(currentFlowPhasorGroup.PositiveSequence.Measurement.MagnitudeKey, currentFlowPhasorGroup.PositiveSequence.Measurement.Magnitude);
                    }
                    if (currentFlowPhasorGroup.PhaseC.Measurement.AngleValueWasReported)
                    {
                        receivedMeasurements.Add(currentFlowPhasorGroup.PositiveSequence.Measurement.AngleKey, currentFlowPhasorGroup.PositiveSequence.Measurement.AngleInDegrees);
                    }
                }
            }
        }

        private void AppendReceivedCurrentInjectionMeasurements(Dictionary<string, double> receivedMeasurements)
        {
            foreach (CurrentInjectionPhasorGroup currentInjectionPhasorGroup in m_currentInjections)
            {
                if (m_phaseSelection == PhaseSelection.PositiveSequence)
                {
                    if (currentInjectionPhasorGroup.PositiveSequence.Measurement.MagnitudeValueWasReported)
                    {
                        receivedMeasurements.Add(currentInjectionPhasorGroup.PositiveSequence.Measurement.MagnitudeKey, currentInjectionPhasorGroup.PositiveSequence.Measurement.Magnitude);
                    }
                    if (currentInjectionPhasorGroup.PositiveSequence.Measurement.AngleValueWasReported)
                    {
                        receivedMeasurements.Add(currentInjectionPhasorGroup.PositiveSequence.Measurement.AngleKey, currentInjectionPhasorGroup.PositiveSequence.Measurement.AngleInDegrees);
                    }
                }
                else if (m_phaseSelection == PhaseSelection.ThreePhase)
                {
                    if (currentInjectionPhasorGroup.PhaseA.Measurement.MagnitudeValueWasReported)
                    {
                        receivedMeasurements.Add(currentInjectionPhasorGroup.PositiveSequence.Measurement.MagnitudeKey, currentInjectionPhasorGroup.PositiveSequence.Measurement.Magnitude);
                    }
                    if (currentInjectionPhasorGroup.PhaseA.Measurement.AngleValueWasReported)
                    {
                        receivedMeasurements.Add(currentInjectionPhasorGroup.PositiveSequence.Measurement.AngleKey, currentInjectionPhasorGroup.PositiveSequence.Measurement.AngleInDegrees);
                    }
                    if (currentInjectionPhasorGroup.PhaseB.Measurement.MagnitudeValueWasReported)
                    {
                        receivedMeasurements.Add(currentInjectionPhasorGroup.PositiveSequence.Measurement.MagnitudeKey, currentInjectionPhasorGroup.PositiveSequence.Measurement.Magnitude);
                    }
                    if (currentInjectionPhasorGroup.PhaseB.Measurement.AngleValueWasReported)
                    {
                        receivedMeasurements.Add(currentInjectionPhasorGroup.PositiveSequence.Measurement.AngleKey, currentInjectionPhasorGroup.PositiveSequence.Measurement.AngleInDegrees);
                    }
                    if (currentInjectionPhasorGroup.PhaseC.Measurement.MagnitudeValueWasReported)
                    {
                        receivedMeasurements.Add(currentInjectionPhasorGroup.PositiveSequence.Measurement.MagnitudeKey, currentInjectionPhasorGroup.PositiveSequence.Measurement.Magnitude);
                    }
                    if (currentInjectionPhasorGroup.PhaseC.Measurement.AngleValueWasReported)
                    {
                        receivedMeasurements.Add(currentInjectionPhasorGroup.PositiveSequence.Measurement.AngleKey, currentInjectionPhasorGroup.PositiveSequence.Measurement.AngleInDegrees);
                    }
                }
            }
        }

        #endregion

        #region [ Xml Serialization/Deserialization ]

        /// <summary>
        /// Creates a new <see cref="NetworkModel"/> by deserializing the configuration file from the specified location.
        /// </summary>
        /// <param name="pathName">The location of the configuration file including the file name.</param>
        /// <returns>A new <see cref="NetworkModel"/> based on the configuration file.</returns>
        public static NetworkModel DeserializeFromXml(string pathName)
        {
            try
            {
                // Create an empy NetworkModel object reference.
                NetworkModel networkModel = null;

                // Create an XmlSerializer with the type of NetworkModel.
                XmlSerializer deserializer = new XmlSerializer(typeof(NetworkModel));

                // Read the data in from the file.
                StreamReader reader = new StreamReader(pathName);

                // Cast the deserialized data as a NetworkModel object.
                networkModel = (NetworkModel)deserializer.Deserialize(reader);

                // Close the connection.
                reader.Close();

                return networkModel;
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to Deserialize the NetworkModel from the Configuration File: " + exception.ToString());
            }
        }

        /// <summary>
        /// Serialized the <see cref="NetworkModel"/> to the specified file.
        /// </summary>
        /// <param name="pathName">The directory name included the file name of the desired location for Xml Serialization.</param>
        public void SerializeToXml(string pathName)
        {
            try
            {
                // Create an XmlSerializer with the type of NetworkModel
                XmlSerializer serializer = new XmlSerializer(typeof(NetworkModel));

                // Open a connection to the file and path.
                TextWriter writer = new StreamWriter(pathName);

                // Serialize this instance of NetworkModel
                serializer.Serialize(writer, this);

                // Close the connection
                writer.Close();
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to Serialize the NetworkModel to the Configuration File: " + exception.ToString());
            }
        }

        #endregion
    }
}
