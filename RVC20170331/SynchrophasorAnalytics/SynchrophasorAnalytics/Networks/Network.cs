//******************************************************************************************************
//  Network.cs
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
//  05/16/2014 - Kevin D. Jones
//       Added System.Numerics for complex numbers
//  05/17/2014 - Kevin D. Jones
//       Added Math.NET for complex matrices
//
//******************************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Complex;
using SynchrophasorAnalytics.Graphs;
using SynchrophasorAnalytics.Modeling;
using SynchrophasorAnalytics.Measurements;
using SynchrophasorAnalytics.Matrices;
using SynchrophasorAnalytics.Calibration;

namespace SynchrophasorAnalytics.Networks
{
    /// <summary>
    /// The class representation of an electric power system network with all of the methods needed to perform linear state estimation and other analytical functions.
    /// </summary>
    [Serializable()]
    public class Network
    {
        #region [ Private Members ]

        private SystemMatrix m_systemMatrix;
        private NetworkModel m_networkModel;
        private bool m_hasChangedSincePreviousFrame;
        private bool[] m_pastDiscreteVoltagePhasorState;
        private bool[] m_pastDiscreteCurrentPhasorState;
        private bool[] m_pastDiscreteShuntCurrentPhasorState;
        private int[] m_pastBreakerStatusState;
        private double[] m_pastStatusWordState;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Determines whether to treat the network as a <see cref="PhaseSelection.PositiveSequence"/> approximation or as a full <see cref="PhaseSelection.ThreePhase"/> representation.
        /// </summary>
        [XmlIgnore()]
        public PhaseSelection PhaseConfiguration
        {
            get 
            {
                return m_networkModel.PhaseConfiguration; 
            }
            set 
            {
                m_networkModel.PhaseConfiguration = value; 
            }
        }

        /// <summary>
        /// The physical representation of the network.
        /// </summary>
        [XmlElement("Model")]
        public NetworkModel Model
        {
            get 
            { 
                return m_networkModel; 
            }
            set 
            { 
                m_networkModel = value; 
            }
        }

        /// <summary>
        /// The present state matrix for the system.
        /// </summary>
        [XmlIgnore()]
        public SystemMatrix Matrix
        {
            get
            {
                return m_systemMatrix;
            }
        }

        /// <summary>
        /// A boolean flag which indicates whether the discrete representations of the measurements have changed since the previous execution
        /// </summary>
        [XmlIgnore()]
        public bool HasChangedSincePreviousFrame
        {
            get
            {
                return m_hasChangedSincePreviousFrame;
            }
        }
        
        /// <summary>
        /// The <see cref="SynchrophasorAnalytics.Modeling.ObservedBus"/> which contains the assumed perfect voltage measurement to do 24 hour CT and PT calibration. Possibility of decpreciation.
        /// </summary>
        [XmlIgnore()]
        public ObservedBus PerfectPtBusForPrimaryPhasorCalibration
        {
            get
            {
                foreach (ObservedBus observedBus in m_networkModel.ObservedBusses)
                {
                    foreach (Node node in observedBus.Nodes)
                    {
                        if (node.Voltage.PhaseA.Measurement.InstrumentTransformerCalibrationSetting == CalibrationSetting.Perfect &&
                            node.Voltage.PhaseB.Measurement.InstrumentTransformerCalibrationSetting == CalibrationSetting.Perfect &&
                            node.Voltage.PhaseC.Measurement.InstrumentTransformerCalibrationSetting == CalibrationSetting.Perfect)
                        {
                            return observedBus;
                        }
                    }
                }
                return null;
            }
        }

        #region [ State Vector ]

        /// <summary>
        /// A <see cref="DenseMatrix"/> of the state vector of the <see cref="SynchrophasorAnalytics.Networks.Network"/> in line-to-neutral volts.
        /// </summary>
        [XmlIgnore()]
        public DenseMatrix StateVector
        {
            get
            {
                bool usePerUnit = false;
                if (m_networkModel.PhaseConfiguration == PhaseSelection.PositiveSequence)
                {
                    return GetPositiveSequenceStateVectorFromModel(usePerUnit);
                }
                else if (m_networkModel.PhaseConfiguration == PhaseSelection.ThreePhase)
                {
                    return GetThreePhaseStateVectorFromModel(usePerUnit);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                bool isPerUnit = false;
                if (m_networkModel.PhaseConfiguration == PhaseSelection.PositiveSequence)
                {
                    SendPositiveSequenceStateVectorToModel(value, isPerUnit);
                }
                else if (m_networkModel.PhaseConfiguration == PhaseSelection.ThreePhase)
                {
                    SendThreePhaseStateVectorToModel(value, isPerUnit);
                }
            }
        }

        /// <summary>
        /// A <see cref="DenseMatrix"/> of the state vector of the <see cref="SynchrophasorAnalytics.Networks.Network"/> in per unit volts.
        /// </summary>
        [XmlIgnore()]
        public DenseMatrix PerUnitStateVector
        {
            get
            {
                bool usePerUnit = true;
                if (m_networkModel.PhaseConfiguration == PhaseSelection.PositiveSequence)
                {
                    return GetPositiveSequenceStateVectorFromModel(usePerUnit);
                }
                else if (m_networkModel.PhaseConfiguration == PhaseSelection.ThreePhase)
                {
                    return GetThreePhaseStateVectorFromModel(usePerUnit);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                bool isPerUnit = true;
                if (m_networkModel.PhaseConfiguration == PhaseSelection.PositiveSequence)
                {
                    SendPositiveSequenceStateVectorToModel(value, isPerUnit);
                }
                else if (m_networkModel.PhaseConfiguration == PhaseSelection.ThreePhase)
                {
                    SendThreePhaseStateVectorToModel(value, isPerUnit);
                }
            }
        }

        /// <summary>
        /// A state vector of the <see cref="SynchrophasorAnalytics.Networks.Network"/> in line-to-neutral voltage magnitude.
        /// </summary>
        [XmlIgnore()]
        public double[] StateVectorMagnitude
        {
            get
            {
                DenseMatrix stateVector = StateVector;
                double[] stateVectorMagnitude = new double[stateVector.RowCount];
                for (int i = 0; i < stateVector.RowCount; i++)
                {
                    stateVectorMagnitude[i] = Math.Sqrt(stateVector[i, 0].Real * stateVector[i, 0].Real + stateVector[i, 0].Imaginary * stateVector[i, 0].Imaginary);
                }
                return stateVectorMagnitude;
            }
        }

        /// <summary>
        /// A state vector of the <see cref="SynchrophasorAnalytics.Networks.Network"/> in per unit voltage magnitude.
        /// </summary>
        [XmlIgnore()]
        public double[] PerUnitStateVectorMagnitude
        {
            get
            {
                DenseMatrix perUnitStateVector = PerUnitStateVector;
                double[] stateVectorMagnitude = new double[perUnitStateVector.RowCount];
                for (int i = 0; i < perUnitStateVector.RowCount; i++)
                {
                    stateVectorMagnitude[i] = Math.Sqrt(perUnitStateVector[i, 0].Real * perUnitStateVector[i, 0].Real + perUnitStateVector[i, 0].Imaginary * perUnitStateVector[i, 0].Imaginary);
                }
                return stateVectorMagnitude;
            }
        }

        /// <summary>
        /// A state vector of the <see cref="SynchrophasorAnalytics.Networks.Network"/> voltage angles in degrees.
        /// </summary>
        [XmlIgnore()]
        public double[] StateVectorAngleInDegrees
        {
            get
            {
                DenseMatrix stateVector = StateVector;
                double[] stateVectorAngleInDegrees = new double[stateVector.RowCount];
                for (int i = 0; i < stateVector.RowCount; i++)
                {
                    PhasorBase phasorPlaceHolder = new PhasorBase();
                    phasorPlaceHolder.ComplexPhasor = stateVector[i, 0];
                    stateVectorAngleInDegrees[i] = phasorPlaceHolder.AngleInDegrees;
                }
                return stateVectorAngleInDegrees;
            }
        }

        /// <summary>
        /// A state vector of the <see cref="SynchrophasorAnalytics.Networks.Network"/> voltage angles in radians.
        /// </summary>
        [XmlIgnore()]
        public double[] StateVectorAngleInRadians
        {
            get
            {
                DenseMatrix stateVector = StateVector;
                double[] stateVectorAngleInRadians = new double[stateVector.RowCount];
                List<PhasorBase> phasorContainer = new List<PhasorBase>();
                for (int i = 0; i < stateVector.RowCount; i++)
                {
                    PhasorBase phasorBase = new PhasorBase();
                    phasorBase.ComplexPhasor = stateVector[i, 0];
                    stateVectorAngleInRadians[i] = phasorBase.AngleInRadians;
                }
                return stateVectorAngleInRadians;
            }
        }

        #endregion

        #region [ Measurement Vector ]

        /// <summary>
        /// A <see cref="DenseMatrix"/> of the measurement vector of the <see cref="SynchrophasorAnalytics.Networks.Network"/> in nominal values.
        /// </summary>
        [XmlIgnore()]
        public DenseMatrix MeasurementVector
        {
            get
            {
                bool usePerUnit = false;
                if (m_networkModel.PhaseConfiguration == PhaseSelection.PositiveSequence)
                {
                    return GetPositiveSequenceMeasurementVectorFromModel(usePerUnit);
                }
                else if (m_networkModel.PhaseConfiguration == PhaseSelection.ThreePhase)
                {
                    return GetThreePhaseMeasurementVectorFromModel(usePerUnit);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// A <see cref="DenseMatrix"/> of the measurement vector of the <see cref="SynchrophasorAnalytics.Networks.Network"/> in per unit values.
        /// </summary>
        [XmlIgnore()]
        public DenseMatrix PerUnitMeasurementVector
        {
            get
            {
                bool usePerUnit = true;
                if (m_networkModel.PhaseConfiguration == PhaseSelection.PositiveSequence)
                {
                    return GetPositiveSequenceMeasurementVectorFromModel(usePerUnit);
                }
                else if (m_networkModel.PhaseConfiguration == PhaseSelection.ThreePhase)
                {
                    return GetThreePhaseMeasurementVectorFromModel(usePerUnit);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// A measuremrent vector of the <see cref="SynchrophasorAnalytics.Networks.Network"/> in phasor magnitude values.
        /// </summary>
        [XmlIgnore()]
        public double[] MeasurementVectorMagnitude
        {
            get
            {
                DenseMatrix measurementVector = MeasurementVector;
                double[] measurementVectorMagnitude = new double[measurementVector.RowCount];
                for (int i = 0; i < measurementVector.RowCount; i++)
                {
                    measurementVectorMagnitude[i] = Math.Sqrt(measurementVector[i, 0].Real * measurementVector[i, 0].Real + measurementVector[i, 0].Imaginary * measurementVector[i, 0].Imaginary);
                }
                return measurementVectorMagnitude;
            }
        }

        /// <summary>
        /// A measuremrent vector of the <see cref="SynchrophasorAnalytics.Networks.Network"/> in per unit values.
        /// </summary>
        [XmlIgnore()]
        public double[] PerUnitMeasurementVectorMagnitude
        {
            get
            {
                DenseMatrix perUnitMeasurementVector = PerUnitMeasurementVector;
                double[] measurementVectorMagnitude = new double[perUnitMeasurementVector.RowCount];
                for (int i = 0; i < perUnitMeasurementVector.RowCount; i++)
                {
                    measurementVectorMagnitude[i] = Math.Sqrt(perUnitMeasurementVector[i, 0].Real * perUnitMeasurementVector[i, 0].Real + perUnitMeasurementVector[i, 0].Imaginary * perUnitMeasurementVector[i, 0].Imaginary);
                }
                return measurementVectorMagnitude;
            }
        }

        /// <summary>
        /// A measuremrent vector of the <see cref="SynchrophasorAnalytics.Networks.Network"/> angles in degrees.
        /// </summary>
        [XmlIgnore()]
        public double[] MeasurementVectorAngleInDegrees
        {
            get
            {
                DenseMatrix measurementVector = MeasurementVector;
                double[] measurementVectorAngleInDegrees = new double[measurementVector.RowCount];
                for (int i = 0; i < measurementVector.RowCount; i++)
                {
                    PhasorBase phasorPlaceHolder = new PhasorBase();
                    phasorPlaceHolder.ComplexPhasor = measurementVector[i, 0];
                    measurementVectorAngleInDegrees[i] = phasorPlaceHolder.AngleInDegrees;
                }
                return measurementVectorAngleInDegrees;
            }
        }

        /// <summary>
        /// A measuremrent vector of the <see cref="SynchrophasorAnalytics.Networks.Network"/> angles in radians.
        /// </summary>
        [XmlIgnore()]
        public double[] MeasurementVectorAngleInRadians
        {
            get
            {
                DenseMatrix measurementVector = MeasurementVector;
                double[] measurementVectorAngleInRadians = new double[measurementVector.RowCount];
                for (int i = 0; i < measurementVector.RowCount; i++)
                {
                    PhasorBase phasorPlaceHolder = new PhasorBase();
                    phasorPlaceHolder.ComplexPhasor = measurementVector[i, 0];
                    measurementVectorAngleInRadians[i] = phasorPlaceHolder.AngleInRadians;
                }
                return measurementVectorAngleInRadians;
            }
        }

        /// <summary>
        /// A <see cref="DenseMatrix"/> of the current measurement vector of the <see cref="SynchrophasorAnalytics.Networks.Network"/> in per unit values used for primary phasor calibration. Possibility of depreciation.
        /// </summary>
        [XmlIgnore()]
        public DenseMatrix CurrentMeasurementVectorForPrimaryPhasorCalibration
        {
            get
            {
                List<CurrentFlowPhasorGroup> currentPhasorsForPrimaryPhasorCalibration = new List<CurrentFlowPhasorGroup>();

                foreach (CurrentFlowPhasorGroup currentPhasorGroup in m_networkModel.ActiveCurrentFlows)
                {
                    if (currentPhasorGroup.PhaseA.Measurement.InstrumentTransformerCalibrationSetting == CalibrationSetting.Active ||
                        currentPhasorGroup.PhaseB.Measurement.InstrumentTransformerCalibrationSetting == CalibrationSetting.Active ||
                        currentPhasorGroup.PhaseC.Measurement.InstrumentTransformerCalibrationSetting == CalibrationSetting.Active)
                    {
                        currentPhasorsForPrimaryPhasorCalibration.Add(currentPhasorGroup);
                    }
                }

                DenseMatrix currentMeasurementVectorForPrimaryPhasorCalibration = DenseMatrix.OfArray(new Complex[3 * currentPhasorsForPrimaryPhasorCalibration.Count(), 1]);

                for (int i = 0; i < currentMeasurementVectorForPrimaryPhasorCalibration.RowCount / 3; i++)
                {
                    currentMeasurementVectorForPrimaryPhasorCalibration[3 * i, 0] = currentPhasorsForPrimaryPhasorCalibration[i].PhaseA.Measurement.PerUnitComplexPhasor;
                    currentMeasurementVectorForPrimaryPhasorCalibration[3 * i + 1, 0] = currentPhasorsForPrimaryPhasorCalibration[i].PhaseB.Measurement.PerUnitComplexPhasor;
                    currentMeasurementVectorForPrimaryPhasorCalibration[3 * i + 2, 0] = currentPhasorsForPrimaryPhasorCalibration[i].PhaseC.Measurement.PerUnitComplexPhasor;
                }

                return currentMeasurementVectorForPrimaryPhasorCalibration;
            }
        }

        /// <summary>
        /// A <see cref="DenseMatrix"/> of the voltage measurement vector of the <see cref="SynchrophasorAnalytics.Networks.Network"/> in per unit values used for primary phasor calibration. Possibility of depreciation.
        /// </summary>
        [XmlIgnore()]
        public DenseMatrix VoltageMeasurementVectorForPrimaryPhasorCalibration
        {
            get
            {
                List<VoltagePhasorGroup> voltagePhasorForPrimaryPhasorCalibration = new List<VoltagePhasorGroup>();

                foreach (ObservedBus observedBus in m_networkModel.ObservedBusses)
                {
                    foreach (Node node in observedBus.Nodes)
                    {
                        if ((node.Voltage.PhaseA.Measurement.InstrumentTransformerCalibrationSetting == CalibrationSetting.Active ||
                             node.Voltage.PhaseA.Measurement.InstrumentTransformerCalibrationSetting == CalibrationSetting.Perfect) &&
                            (node.Voltage.PhaseB.Measurement.InstrumentTransformerCalibrationSetting == CalibrationSetting.Active ||
                             node.Voltage.PhaseB.Measurement.InstrumentTransformerCalibrationSetting == CalibrationSetting.Perfect) &&
                            (node.Voltage.PhaseC.Measurement.InstrumentTransformerCalibrationSetting == CalibrationSetting.Active ||
                             node.Voltage.PhaseC.Measurement.InstrumentTransformerCalibrationSetting == CalibrationSetting.Perfect))
                        {
                            voltagePhasorForPrimaryPhasorCalibration.Add(node.Voltage);
                        }
                    }
                }

                DenseMatrix voltageMeasurementVectorForPrimaryPhasorCalibration = DenseMatrix.OfArray(new Complex[3 * voltagePhasorForPrimaryPhasorCalibration.Count(), 1]);

                for (int i = 0; i < voltageMeasurementVectorForPrimaryPhasorCalibration.RowCount / 3; i++)
                {
                    voltageMeasurementVectorForPrimaryPhasorCalibration[3 * i, 0] = voltagePhasorForPrimaryPhasorCalibration[i].PhaseA.Measurement.PerUnitComplexPhasor;
                    voltageMeasurementVectorForPrimaryPhasorCalibration[3 * i + 1, 0] = voltagePhasorForPrimaryPhasorCalibration[i].PhaseB.Measurement.PerUnitComplexPhasor;
                    voltageMeasurementVectorForPrimaryPhasorCalibration[3 * i + 2, 0] = voltagePhasorForPrimaryPhasorCalibration[i].PhaseC.Measurement.PerUnitComplexPhasor;
                }

                return voltageMeasurementVectorForPrimaryPhasorCalibration;
            }
        }

        #endregion

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// A blank constructor with default values.
        /// </summary>
        public Network()
            :this(new NetworkModel())
        {
        }

        /// <summary>
        /// The designated constructor for the <see cref="Network"/> class.
        /// </summary>
        /// <param name="networkModel">The virtualized <see cref="NetworkModel"/>.</param>
        public Network(NetworkModel networkModel)
        {
            m_networkModel = networkModel;
            m_networkModel.ParentNetwork = this;
            m_hasChangedSincePreviousFrame = true;
        }

        #endregion

        #region [ Private Methods ]

        private DenseMatrix GetPositiveSequenceStateVectorFromModel(bool usePerUnit)
        {
            List<VoltagePhasorGroup> nodeVoltages = m_networkModel.Voltages;

            DenseMatrix stateVector = DenseMatrix.OfArray(new Complex[nodeVoltages.Count, 1]);

            if (usePerUnit)
            {
                for (int i = 0; i < nodeVoltages.Count; i++)
                {
                    stateVector[i, 0] = nodeVoltages[i].PositiveSequence.Estimate.PerUnitComplexPhasor;
                }
            }
            else
            {
                for (int i = 0; i < nodeVoltages.Count; i++)
                {
                    stateVector[i, 0] = nodeVoltages[i].PositiveSequence.Estimate.ComplexPhasor;
                }
            }

            return stateVector;
        }

        private DenseMatrix GetThreePhaseStateVectorFromModel(bool usePerUnit)
        {
            List<VoltagePhasorGroup> nodeVoltages = m_networkModel.Voltages;

            DenseMatrix stateVector = DenseMatrix.OfArray(new Complex[3 * nodeVoltages.Count, 1]);

            if (usePerUnit)
            {
                for (int i = 0; i < nodeVoltages.Count; i++)
                {
                    stateVector[3 * i, 0] = nodeVoltages[i].PhaseA.Estimate.PerUnitComplexPhasor;
                    stateVector[3 * i + 1, 0] = nodeVoltages[i].PhaseB.Estimate.PerUnitComplexPhasor;
                    stateVector[3 * i + 2, 0] = nodeVoltages[i].PhaseC.Estimate.PerUnitComplexPhasor;
                }
            }
            else
            {
                for (int i = 0; i < nodeVoltages.Count; i++)
                {
                    stateVector[3 * i, 0] = nodeVoltages[i].PhaseA.Estimate.ComplexPhasor;
                    stateVector[3 * i + 1, 0] = nodeVoltages[i].PhaseB.Estimate.ComplexPhasor;
                    stateVector[3 * i + 2, 0] = nodeVoltages[i].PhaseC.Estimate.ComplexPhasor;
                }
            }

            return stateVector;
        }

        #region [ Get Positive Sequence Measurement Vector Methods ]

        private DenseMatrix GetPositiveSequenceMeasurementVectorFromModel(bool usePerUnit)
        {
            DenseMatrix voltageMeasurementVector = GetPositiveSequenceVoltageMeasurementVectorFromModel(usePerUnit);
            DenseMatrix currentFlowMeasurementVector = GetPositiveSequenceCurrentFlowMeasurementVectorFromModel(usePerUnit);
            DenseMatrix currentInjectionMeasurementVector = GetPositiveSequenceCurrentInjectionMeasurementVectorFromModel(usePerUnit);

            DenseMatrix measurementVector = voltageMeasurementVector;

            if (currentFlowMeasurementVector != null)
            {
                measurementVector = MatrixCalculationExtensions.VerticallyConcatenate(measurementVector, currentFlowMeasurementVector);
            }

            if (currentInjectionMeasurementVector != null)
            {
                measurementVector = MatrixCalculationExtensions.VerticallyConcatenate(measurementVector, currentInjectionMeasurementVector);
            }

            return measurementVector;

        }

        private DenseMatrix GetPositiveSequenceVoltageMeasurementVectorFromModel(bool usePerUnit)
        {
            // Resolve the Network into a list of ObservedBusses
            List<ObservedBus> observedBusses = m_networkModel.ObservedBusses;

            // Create a list of directly measured nodes from the set of ObservedBusses
            List<Node> measuredNodes = new List<Node>();

            foreach (ObservedBus observedBus in observedBusses)
            {
                foreach (Node node in observedBus.Nodes)
                {
                    if (node.Observability == ObservationState.DirectlyObserved)
                    {
                        measuredNodes.Add(node);
                    }
                }
            }

            if (measuredNodes.Count > 0)
            {
                DenseMatrix voltageMeasurementVector = DenseMatrix.OfArray(new Complex[measuredNodes.Count, 1]);

                if (usePerUnit)
                {
                    for (int i = 0; i < measuredNodes.Count; i++)
                    {
                        voltageMeasurementVector[i, 0] = measuredNodes[i].Voltage.PositiveSequence.Measurement.PerUnitComplexPhasor;
                    }

                }
                else
                {
                    for (int i = 0; i < measuredNodes.Count; i++)
                    {
                        voltageMeasurementVector[i, 0] = measuredNodes[i].Voltage.PositiveSequence.Measurement.ComplexPhasor;
                    }

                }

                return voltageMeasurementVector;
            }
            else
            {
                throw new Exception("No voltages to include in the measurement vector.");
            }
        }

        private DenseMatrix GetPositiveSequenceCurrentFlowMeasurementVectorFromModel(bool usePerUnit)
        {
            List<CurrentFlowPhasorGroup> measuredCurrentFlows = m_networkModel.IncludedCurrentFlows;

            if (measuredCurrentFlows.Count > 0)
            {
                DenseMatrix currentFlowMeasurementVector = DenseMatrix.OfArray(new Complex[measuredCurrentFlows.Count, 1]);

                if (usePerUnit)
                {
                    for (int i = 0; i < measuredCurrentFlows.Count; i++)
                    {
                        currentFlowMeasurementVector[i, 0] = measuredCurrentFlows[i].PositiveSequence.Measurement.PerUnitComplexPhasor;
                    }
                }
                else
                {
                    for (int i = 0; i < measuredCurrentFlows.Count; i++)
                    {
                        currentFlowMeasurementVector[i, 0] = measuredCurrentFlows[i].PositiveSequence.Measurement.ComplexPhasor;
                    }
                }

                return currentFlowMeasurementVector;
            }
            else
            {
                return null;
            }
        }

        private DenseMatrix GetPositiveSequenceCurrentInjectionMeasurementVectorFromModel(bool usePerUnit)
        {
            // Get the currrent measurements from shunt injections
            List<CurrentInjectionPhasorGroup> measuredCurrentInjections = m_networkModel.ActiveCurrentInjections;

            if (measuredCurrentInjections.Count > 0)
            {
                DenseMatrix currentInjectionMeasurementVector = DenseMatrix.OfArray(new Complex[measuredCurrentInjections.Count, 1]);

                if (usePerUnit)
                {
                    for (int i = 0; i < measuredCurrentInjections.Count; i++)
                    {
                        currentInjectionMeasurementVector[i, 0] = measuredCurrentInjections[i].PositiveSequence.Measurement.PerUnitComplexPhasor;
                    }
                }
                else
                {
                    for (int i = 0; i < measuredCurrentInjections.Count; i++)
                    {
                        currentInjectionMeasurementVector[i, 0] = measuredCurrentInjections[i].PositiveSequence.Measurement.ComplexPhasor;
                    }
                }

                return currentInjectionMeasurementVector;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region [ Get Three Phase Measurement Vector Methods ]

        private DenseMatrix GetThreePhaseMeasurementVectorFromModel(bool usePerUnit)
        {
            DenseMatrix voltageMeasurementVector = GetThreePhaseVoltageMeasurementVectorFromModel(usePerUnit);
            DenseMatrix currentFlowMeasurementVector = GetThreePhaseCurrentFlowMeasurementVectorFromModel(usePerUnit);
            DenseMatrix currentInjectionMeasurementVector = GetThreePhaseCurrentInjectionMeasurementVectorFromModel(usePerUnit);

            DenseMatrix measurementVector = voltageMeasurementVector;

            if (currentFlowMeasurementVector != null)
            {
                measurementVector = MatrixCalculationExtensions.VerticallyConcatenate(measurementVector, currentFlowMeasurementVector);
            }

            if (currentInjectionMeasurementVector != null)
            {
                measurementVector = MatrixCalculationExtensions.VerticallyConcatenate(measurementVector, currentInjectionMeasurementVector);
            }

            return measurementVector;
        }

        private DenseMatrix GetThreePhaseVoltageMeasurementVectorFromModel(bool usePerUnit)
        {
            // Resolve the Network into a list of ObservedBusses
            List<ObservedBus> observedBusses = m_networkModel.ObservedBusses;

            // Create a list of directly measured nodes from the set of ObservedBusses
            List<Node> measuredNodes = new List<Node>();
            foreach (ObservedBus observedBus in observedBusses)
            {
                foreach (Node node in observedBus.Nodes)
                {
                    if (node.Observability == ObservationState.DirectlyObserved)
                    {
                        measuredNodes.Add(node);
                    }
                }
            }

            if (measuredNodes.Count > 0)
            {
                DenseMatrix voltageMeasurementVector = DenseMatrix.OfArray(new Complex[3 * measuredNodes.Count, 1]);

                if (usePerUnit)
                {
                    for (int i = 0; i < measuredNodes.Count; i++)
                    {
                        voltageMeasurementVector[3 * i, 0] = measuredNodes[i].Voltage.PhaseA.Measurement.PerUnitComplexPhasor;
                        voltageMeasurementVector[3 * i + 1, 0] = measuredNodes[i].Voltage.PhaseB.Measurement.PerUnitComplexPhasor;
                        voltageMeasurementVector[3 * i + 2, 0] = measuredNodes[i].Voltage.PhaseC.Measurement.PerUnitComplexPhasor;
                    }
                }
                else
                {
                    for (int i = 0; i < measuredNodes.Count; i++)
                    {
                        voltageMeasurementVector[3 * i, 0] = measuredNodes[i].Voltage.PhaseA.Measurement.ComplexPhasor;
                        voltageMeasurementVector[3 * i + 1, 0] = measuredNodes[i].Voltage.PhaseB.Measurement.ComplexPhasor;
                        voltageMeasurementVector[3 * i + 2, 0] = measuredNodes[i].Voltage.PhaseC.Measurement.ComplexPhasor;
                    }
                }

                return voltageMeasurementVector;
            }
            else
            {
                throw new Exception("No voltages to include in the measurement vector.");
            }
        }

        private DenseMatrix GetThreePhaseCurrentFlowMeasurementVectorFromModel(bool usePerUnit)
        {
            List<CurrentFlowPhasorGroup> measuredCurrentFlows = m_networkModel.IncludedCurrentFlows;

            if (measuredCurrentFlows.Count > 0)
            {
                DenseMatrix currentFlowMeasurementVector = DenseMatrix.OfArray(new Complex[3 * measuredCurrentFlows.Count, 1]);

                if (usePerUnit)
                {
                    for (int i = 0; i < measuredCurrentFlows.Count; i++)
                    {
                        currentFlowMeasurementVector[3 * i, 0] = measuredCurrentFlows[i].PhaseA.Measurement.PerUnitComplexPhasor;
                        currentFlowMeasurementVector[3 * i + 1, 0] = measuredCurrentFlows[i].PhaseB.Measurement.PerUnitComplexPhasor;
                        currentFlowMeasurementVector[3 * i + 2, 0] = measuredCurrentFlows[i].PhaseC.Measurement.PerUnitComplexPhasor;
                    }
                }
                else
                {
                    for (int i = 0; i < measuredCurrentFlows.Count; i++)
                    {
                        currentFlowMeasurementVector[3 * i, 0] = measuredCurrentFlows[i].PhaseA.Measurement.ComplexPhasor;
                        currentFlowMeasurementVector[3 * i + 1, 0] = measuredCurrentFlows[i].PhaseB.Measurement.ComplexPhasor;
                        currentFlowMeasurementVector[3 * i + 2, 0] = measuredCurrentFlows[i].PhaseC.Measurement.ComplexPhasor;
                    }
                }

                return currentFlowMeasurementVector;
            }
            else
            {
                return null;
            }
        }

        private DenseMatrix GetThreePhaseCurrentInjectionMeasurementVectorFromModel(bool usePerUnit)
        {
            // Get the currrent measurements from shunt injections
            List<CurrentInjectionPhasorGroup> measuredCurrentInjections = m_networkModel.ActiveCurrentInjections;

            if (measuredCurrentInjections.Count > 0)
            {
                DenseMatrix currentInjectionMeasurementVector = DenseMatrix.OfArray(new Complex[3 * measuredCurrentInjections.Count, 1]);

                if (usePerUnit)
                {
                    for (int i = 0; i < measuredCurrentInjections.Count; i++)
                    {
                        currentInjectionMeasurementVector[3 * i, 0] = measuredCurrentInjections[i].PhaseA.Measurement.PerUnitComplexPhasor;
                        currentInjectionMeasurementVector[3 * i + 1, 0] = measuredCurrentInjections[i].PhaseB.Measurement.PerUnitComplexPhasor;
                        currentInjectionMeasurementVector[3 * i + 2, 0] = measuredCurrentInjections[i].PhaseC.Measurement.PerUnitComplexPhasor;
                    }
                }
                else
                {
                    for (int i = 0; i < measuredCurrentInjections.Count; i++)
                    {
                        currentInjectionMeasurementVector[3 * i, 0] = measuredCurrentInjections[i].PhaseA.Measurement.ComplexPhasor;
                        currentInjectionMeasurementVector[3 * i + 1, 0] = measuredCurrentInjections[i].PhaseB.Measurement.ComplexPhasor;
                        currentInjectionMeasurementVector[3 * i + 2, 0] = measuredCurrentInjections[i].PhaseC.Measurement.ComplexPhasor;
                    }
                }

                return currentInjectionMeasurementVector;
            }
            else
            {
                return null;
            }
        }

        #endregion

        private void SendPositiveSequenceStateVectorToModel(DenseMatrix stateVector, bool isPerUnit)
        {
            List<ObservedBus> observedBusses = m_networkModel.ObservedBusses;

            for (int i = 0; i < observedBusses.Count; i++)
            {
                VoltagePhasorGroup voltagePhasorGroup = new VoltagePhasorGroup();
                voltagePhasorGroup.PositiveSequence.Estimate.BaseKV = observedBusses[i].BaseKV;
                voltagePhasorGroup.PositiveSequence.Estimate.PerUnitComplexPhasor = stateVector[i, 0];
                observedBusses[i].Value = voltagePhasorGroup;
            }
        }

        private void SendThreePhaseStateVectorToModel(DenseMatrix stateVector, bool isPerUnit)
        {
            List<ObservedBus> observedBusses = m_networkModel.ObservedBusses;

            for (int i = 0; i < observedBusses.Count; i++)
            {
                VoltagePhasorGroup voltagePhasorGroup = new VoltagePhasorGroup();
                voltagePhasorGroup.PhaseA.Estimate.BaseKV = observedBusses[i].BaseKV;
                voltagePhasorGroup.PhaseB.Estimate.BaseKV = observedBusses[i].BaseKV;
                voltagePhasorGroup.PhaseC.Estimate.BaseKV = observedBusses[i].BaseKV;
                voltagePhasorGroup.PhaseA.Estimate.PerUnitComplexPhasor = stateVector[3 * i, 0];
                voltagePhasorGroup.PhaseB.Estimate.PerUnitComplexPhasor = stateVector[3 * i + 1, 0];
                voltagePhasorGroup.PhaseC.Estimate.PerUnitComplexPhasor = stateVector[3 * i + 2, 0];
                voltagePhasorGroup.ComputeSequenceComponents();
                observedBusses[i].Value = voltagePhasorGroup;
            }
        }

        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// Initializes the <see cref="SynchrophasorAnalytics.Networks.Network"/> by re-establishing references that are not preserved hierarchically in the configuration file.
        /// </summary>
        public void Initialize()
        {
            // Initialize children
            m_networkModel.Initialize();
            InitializePastDiscreteStates();
        }

        /// <summary>
        /// Computes the current state of the network
        /// </summary>
        public void ComputeSystemState()
        {
            if (m_systemMatrix == null || m_hasChangedSincePreviousFrame)
            {
                m_systemMatrix = new SystemMatrix(this);
            }
            
            PerUnitStateVector = m_systemMatrix.PsuedoInverseOfMatrix * PerUnitMeasurementVector;
        }

        /// <summary>
        /// A method which checks to see if the matrix representation of the networks needs to be reperformed and sets the flag for <see cref="SynchrophasorAnalytics.Networks.Network.HasChangedSincePreviousFrame"/>.
        /// </summary>
        public void RunNetworkReconstructionCheck()
        {
            if (ComparePresentAndPastDiscreteStates())
            {
                m_hasChangedSincePreviousFrame = true;
                UpdatePastDiscreteStates();
            }
            else
            {
                m_hasChangedSincePreviousFrame = false;
            }
        }

        #endregion

        #region [ Private Methods ]

        private void UpdatePastDiscreteStates()
        {

            for (int i = 0; i < m_networkModel.Voltages.Count(); i++)
            {
                if (m_networkModel.PhaseConfiguration == PhaseSelection.ThreePhase)
                {
                    m_pastDiscreteVoltagePhasorState[i] = m_networkModel.Voltages[i].IncludeInEstimator;
                }
                else if (m_networkModel.PhaseConfiguration == PhaseSelection.PositiveSequence)
                {
                    m_pastDiscreteVoltagePhasorState[i] = m_networkModel.Voltages[i].IncludeInPositiveSequenceEstimator;
                }
            }

            for (int i = 0; i < m_networkModel.CurrentFlows.Count(); i++)
            {
                if (m_networkModel.PhaseConfiguration == PhaseSelection.ThreePhase)
                {
                    m_pastDiscreteCurrentPhasorState[i] = m_networkModel.CurrentFlows[i].IncludeInEstimator;
                }
                else if (m_networkModel.PhaseConfiguration == PhaseSelection.PositiveSequence)
                {
                    m_pastDiscreteCurrentPhasorState[i] = m_networkModel.CurrentFlows[i].IncludeInPositiveSequenceEstimator;
                }
            }

            for (int i = 0; i < m_networkModel.CurrentInjections.Count(); i++)
            {
                if (m_networkModel.PhaseConfiguration == PhaseSelection.ThreePhase)
                {
                    m_pastDiscreteShuntCurrentPhasorState[i] = m_networkModel.CurrentInjections[i].IncludeInEstimator;
                }
                else if (m_networkModel.PhaseConfiguration == PhaseSelection.PositiveSequence)
                {
                    m_pastDiscreteShuntCurrentPhasorState[i] = m_networkModel.CurrentInjections[i].IncludeInPositiveSequenceEstimator;
                }
            }

            for (int i = 0; i < m_networkModel.BreakerStatuses.Count(); i++)
            {
                m_pastBreakerStatusState[i] = m_networkModel.BreakerStatuses[i].BinaryValue;
            }

            for (int i = 0; i < m_networkModel.StatusWords.Count(); i++)
            {
                m_pastStatusWordState[i] = m_networkModel.StatusWords[i].BinaryValue;
            }

        }

        private void InitializePastDiscreteStates()
        {

            m_pastDiscreteVoltagePhasorState = new bool[m_networkModel.Voltages.Count()];
            m_pastDiscreteCurrentPhasorState = new bool[m_networkModel.CurrentFlows.Count()];
            m_pastDiscreteShuntCurrentPhasorState = new bool[m_networkModel.CurrentInjections.Count()];
            m_pastBreakerStatusState = new int[m_networkModel.BreakerStatuses.Count()];
            m_pastStatusWordState = new double[m_networkModel.StatusWords.Count()];

            for (int i = 0; i < m_pastDiscreteVoltagePhasorState.Length; i++)
            {
                m_pastDiscreteVoltagePhasorState[i] = true;
            }

            if (m_pastDiscreteCurrentPhasorState.Length > 0)
            {
                for (int i = 0; i < m_pastDiscreteCurrentPhasorState.Length; i++)
                {
                    m_pastDiscreteCurrentPhasorState[i] = true;
                }
            }

            if (m_pastDiscreteShuntCurrentPhasorState.Length > 0)
            {
                for (int i = 0; i < m_pastDiscreteShuntCurrentPhasorState.Length; i++)
                {
                    m_pastDiscreteShuntCurrentPhasorState[i] = true;
                }
            }

            if (m_pastBreakerStatusState.Length > 0)
            {
                for (int i = 0; i < m_pastBreakerStatusState.Length; i++)
                {
                    m_pastBreakerStatusState[i] = 0;
                }
            }

            if (m_pastStatusWordState.Length > 0)
            {
                for (int i = 0; i < m_pastStatusWordState.Length; i++)
                {
                    m_pastStatusWordState[i] = 0;
                }
            }
        }

        private bool ComparePresentAndPastDiscreteStates()
        {
            for (int i = 0; i < m_networkModel.Voltages.Count(); i++)
            {
                if (m_networkModel.PhaseConfiguration == PhaseSelection.ThreePhase)
                {
                    if (m_pastDiscreteVoltagePhasorState[i] != m_networkModel.Voltages[i].IncludeInEstimator)
                    {
                        return true;
                    }
                }
                else if (m_networkModel.PhaseConfiguration == PhaseSelection.PositiveSequence)
                {
                    if (m_pastDiscreteVoltagePhasorState[i] != m_networkModel.Voltages[i].IncludeInPositiveSequenceEstimator)
                    {
                        return true;
                    }
                }
            }

            for (int i = 0; i < m_networkModel.CurrentFlows.Count(); i++)
            {
                if (m_networkModel.PhaseConfiguration == PhaseSelection.ThreePhase)
                {
                    if (m_pastDiscreteCurrentPhasorState[i] != m_networkModel.CurrentFlows[i].IncludeInEstimator)
                    {
                        return true;
                    }
                }
                else if (m_networkModel.PhaseConfiguration == PhaseSelection.PositiveSequence)
                {
                    if (m_pastDiscreteCurrentPhasorState[i] != m_networkModel.CurrentFlows[i].IncludeInPositiveSequenceEstimator)
                    {
                        return true;
                    }
                }
            }

            for (int i = 0; i < m_networkModel.CurrentInjections.Count(); i++)
            {
                if (m_networkModel.PhaseConfiguration == PhaseSelection.ThreePhase)
                {
                    if (m_pastDiscreteCurrentPhasorState[i] != m_networkModel.CurrentInjections[i].IncludeInEstimator)
                    {
                        return true;
                    }
                }
                else if (m_networkModel.PhaseConfiguration == PhaseSelection.PositiveSequence)
                {
                    if (m_pastDiscreteCurrentPhasorState[i] != m_networkModel.CurrentInjections[i].IncludeInPositiveSequenceEstimator)
                    {
                        return true;
                    }
                }
            }

            for (int i = 0; i < m_networkModel.BreakerStatuses.Count(); i++)
            {
                if (m_pastBreakerStatusState[i] != m_networkModel.BreakerStatuses[i].BinaryValue)
                {
                    return true;
                }
            }

            for (int i = 0; i < m_networkModel.StatusWords.Count(); i++)
            {
                if (m_pastStatusWordState[i] != m_networkModel.StatusWords[i].BinaryValue)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region [ Xml Serialization/Deserialization methods ]

        /// <summary>
        /// Creates a new <see cref="SynchrophasorAnalytics.Networks.Network"/> by deserializing the configuration file from the specified location.
        /// </summary>
        /// <param name="pathName">The location of the configuration file including the file name.</param>
        /// <returns>A new (but uninitialized) <see cref="SynchrophasorAnalytics.Networks.Network"/> based on the configuration file. Must call <see cref="SynchrophasorAnalytics.Modeling.NetworkModel.Initialize"/> method 
        /// to reconstitute parent, child, and sibling references in the <see cref="SynchrophasorAnalytics.Networks.Network"/>.</returns>
        public static Network DeserializeFromXml(string pathName)
        {
            try
            {
                // Create an empy NetworkMeasurements object reference.
                Network network = null;

                // Create an XmlSerializer with the type of NetworkMeasurements.
                XmlSerializer deserializer = new XmlSerializer(typeof(Network));

                // Read the data in from the file.
                StreamReader reader = new StreamReader(pathName);

                // Cast the deserialized data as a NetworkMeasurements object.
                network = (Network)deserializer.Deserialize(reader);

                // Close the connection.
                reader.Close();
                
                return network;
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to Deserialize the Network from the Configuration File: " + exception.ToString());
            }
        }

        /// <summary>
        /// Serialized the <see cref="SynchrophasorAnalytics.Networks.Network"/> to the specified file.
        /// </summary>
        /// <param name="pathName">The directory name included the file name of the desired location for Xml Serialization.</param>
        public void SerializeToXml(string pathName)
        {
            try
            {
                // Create an XmlSerializer with the type of Network
                XmlSerializer serializer = new XmlSerializer(typeof(Network));
                
                // Open a connection to the file and path.
                TextWriter writer = new StreamWriter(pathName);

                // Serialize this instance of NetworkMeasurements
                serializer.Serialize(writer, this);

                // Close the connection
                writer.Close();
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to Serialize the Network to the Configuration File: " + exception.ToString());
            }
        }

        /// <summary>
        /// Sends command based on boolen argument to either serialize the operating point with the model or not.
        /// </summary>
        /// <param name="serializationOption">A flag which represents whether or not to serialize the operating point with the model data.</param>
        public void SerializeData(bool serializationOption)
        {
            foreach (StatusWord statusWord in m_networkModel.StatusWords)
            {
                statusWord.ShouldSerializeData = serializationOption;
            }

            foreach (VoltagePhasorGroup voltagePhasorGroup in m_networkModel.Voltages)
            {
                voltagePhasorGroup.ShouldSerializeData = serializationOption;

                voltagePhasorGroup.PositiveSequence.Measurement.ShouldSerializeData = serializationOption;
                voltagePhasorGroup.PositiveSequence.Estimate.ShouldSerializeData = serializationOption;

                voltagePhasorGroup.PhaseA.Measurement.ShouldSerializeData = serializationOption;
                voltagePhasorGroup.PhaseA.Estimate.ShouldSerializeData = serializationOption;

                voltagePhasorGroup.PhaseB.Measurement.ShouldSerializeData = serializationOption;
                voltagePhasorGroup.PhaseB.Estimate.ShouldSerializeData = serializationOption;

                voltagePhasorGroup.PhaseC.Measurement.ShouldSerializeData = serializationOption;
                voltagePhasorGroup.PhaseC.Estimate.ShouldSerializeData = serializationOption;
            }

            foreach (CurrentFlowPhasorGroup currentPhasorGroup in m_networkModel.CurrentFlows)
            {
                currentPhasorGroup.ShouldSerializeData = serializationOption;

                currentPhasorGroup.PositiveSequence.Measurement.ShouldSerializeData = serializationOption;
                currentPhasorGroup.PositiveSequence.Estimate.ShouldSerializeData = serializationOption;

                currentPhasorGroup.PhaseA.Measurement.ShouldSerializeData = serializationOption;
                currentPhasorGroup.PhaseA.Estimate.ShouldSerializeData = serializationOption;

                currentPhasorGroup.PhaseB.Measurement.ShouldSerializeData = serializationOption;
                currentPhasorGroup.PhaseB.Estimate.ShouldSerializeData = serializationOption;

                currentPhasorGroup.PhaseC.Measurement.ShouldSerializeData = serializationOption;
                currentPhasorGroup.PhaseC.Estimate.ShouldSerializeData = serializationOption;
            }

            foreach (CurrentInjectionPhasorGroup shuntCurrentPhasorGroup in m_networkModel.CurrentInjections)
            {
                shuntCurrentPhasorGroup.ShouldSerializeData = serializationOption;

                shuntCurrentPhasorGroup.PositiveSequence.Measurement.ShouldSerializeData = serializationOption;
                shuntCurrentPhasorGroup.PositiveSequence.Estimate.ShouldSerializeData = serializationOption;

                shuntCurrentPhasorGroup.PhaseA.Measurement.ShouldSerializeData = serializationOption;
                shuntCurrentPhasorGroup.PhaseA.Estimate.ShouldSerializeData = serializationOption;

                shuntCurrentPhasorGroup.PhaseB.Measurement.ShouldSerializeData = serializationOption;
                shuntCurrentPhasorGroup.PhaseB.Estimate.ShouldSerializeData = serializationOption;

                shuntCurrentPhasorGroup.PhaseC.Measurement.ShouldSerializeData = serializationOption;
                shuntCurrentPhasorGroup.PhaseC.Estimate.ShouldSerializeData = serializationOption;
            }
        }

        /// <summary>
        /// Saves the current state of the <see cref="SynchrophasorAnalytics.Networks.Network"/> object to a *.txt file to be used as a rudimentary momento design pattern. This is an alternative to Xml serialization. Honestly, I don't know why anyone would use plain text for a network model but you never know :P
        /// </summary>
        /// <param name="pathName">The path where the *.txt file should be saved.</param>
        public void ToTextFile(string pathName)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();

                foreach (Company company in m_networkModel.Companies)
                {
                    stringBuilder.AppendLine(company.ToString());
                    foreach (Division division in company.Divisions)
                    {
                        stringBuilder.AppendLine(division.ToString());
                        foreach (Substation substation in division.Substations)
                        {
                            stringBuilder.AppendLine(substation.ToString());
                            foreach (Node node in substation.Nodes)
                            {
                                stringBuilder.AppendLine(node.ToString());
                                stringBuilder.AppendLine(node.Voltage.ToString());
                            }
                            foreach (CircuitBreaker circuitBreaker in substation.CircuitBreakers)
                            {
                                stringBuilder.AppendLine(circuitBreaker.ToString());
                            }
                            foreach (Switch circuitSwitch in substation.Switches)
                            {
                                stringBuilder.AppendLine(circuitSwitch.ToString());
                            }
                            foreach (ShuntCompensator shunt in substation.Shunts)
                            {
                                stringBuilder.AppendLine(shunt.ToString());
                            }
                        }

                        foreach (TransmissionLine transmissionLine in division.TransmissionLines)
                        {
                            stringBuilder.AppendLine(transmissionLine.ToString());
                            stringBuilder.AppendLine(transmissionLine.FromSubstationCurrent.ToString());
                            stringBuilder.AppendLine(transmissionLine.ToSubstationCurrent.ToString());
                            foreach (LineSegment lineSegment in transmissionLine.LineSegments)
                            {
                                stringBuilder.AppendLine(lineSegment.ToString());
                            }
                        }
                    }
                }
                File.WriteAllText(pathName, stringBuilder.ToString());
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to cache the network as a text file: " + exception.ToString());
            }
        }

        /// <summary>
        /// Will throw a <b>NotImplementedException</b>
        /// </summary>
        /// <param name="pathName">The path to retrieve the *.txt file from</param>
        /// <returns>A reinstantiated <see cref="SynchrophasorAnalytics.Networks.Network"/> object.</returns>
        public static Network FromTextFile(string pathName)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
