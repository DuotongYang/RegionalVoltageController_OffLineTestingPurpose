using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SynchrophasorAnalytics.Measurements;
using SynchrophasorAnalytics.Modeling;
using SynchrophasorAnalytics.Networks;
using SynchrophasorAnalytics.Graphs;
using SynchrophasorAnalytics.Testing;
using SynchrophasorAnalytics.Matrices;
using SynchrophasorAnalytics.Calibration;
using SynchrophasorAnalytics.DataConditioning;
using System.Xml.Serialization;
using System.IO;
using System.Configuration;
using System.Numerics;

namespace SynchrophasorAnalyticsConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //PhasorMeasurement busAVoltage = new PhasorMeasurement()
            //    {
            //        Type = PhasorType.VoltagePhasor,
            //        BaseKV = new VoltageLevel(1, 230),
            //        Magnitude = 133518.0156,
            //        AngleInDegrees = -2.4644
            //    };
            //PhasorMeasurement busBVoltage = new PhasorMeasurement()
            //{
            //    Type = PhasorType.VoltagePhasor,
            //    BaseKV = new VoltageLevel(1, 230),
            //    Magnitude = 133758.7656,
            //    AngleInDegrees = 2.4317
            //};
            //PhasorMeasurement busCVoltage = new PhasorMeasurement()
            //{
            //    Type = PhasorType.VoltagePhasor,
            //    BaseKV = new VoltageLevel(1, 230),
            //    Magnitude = 133666.7188,
            //    AngleInDegrees = -2.1697
            //};
            //PhasorMeasurement busDVoltage = new PhasorMeasurement()
            //{
            //    Type = PhasorType.VoltagePhasor,
            //    BaseKV = new VoltageLevel(1, 230),
            //    Magnitude = 134102.8125,
            //    AngleInDegrees = 0.0096257
            //};
            //PhasorMeasurement busEVoltage = new PhasorMeasurement()
            //{
            //    Type = PhasorType.VoltagePhasor,
            //    BaseKV = new VoltageLevel(1, 230),
            //    Magnitude = 133088.9688,
            //    AngleInDegrees = -7.2477
            //};
            //PhasorMeasurement busFVoltage = new PhasorMeasurement()
            //{
            //    Type = PhasorType.VoltagePhasor,
            //    BaseKV = new VoltageLevel(1, 230),
            //    Magnitude = 133141.7344,
            //    AngleInDegrees = -6.3372
            //};
            //PhasorMeasurement busGVoltage = new PhasorMeasurement()
            //{
            //    Type = PhasorType.VoltagePhasor,
            //    BaseKV = new VoltageLevel(1, 230),
            //    Magnitude = 133346.1094,
            //    AngleInDegrees = -5.8259
            //};
            //PhasorMeasurement busHVoltage = new PhasorMeasurement()
            //{
            //    Type = PhasorType.VoltagePhasor,
            //    BaseKV = new VoltageLevel(1, 230),
            //    Magnitude = 133492.2969,
            //    AngleInDegrees = -4.6002
            //};
            /////
            /////
            ///// Current
            //PhasorMeasurement busBtoBusAFlow = new PhasorMeasurement()
            //    {
            //        Type = PhasorType.CurrentPhasor,
            //        BaseKV = new VoltageLevel(1, 230)
            //    };
            //PhasorMeasurement busBtoBusCFlow = new PhasorMeasurement()
            //{
            //    Type = PhasorType.CurrentPhasor,
            //    BaseKV = new VoltageLevel(1, 230)
            //};
            //PhasorMeasurement busDtoBusCFlow = new PhasorMeasurement()
            //{
            //    Type = PhasorType.CurrentPhasor,
            //    BaseKV = new VoltageLevel(1, 230)
            //};
            //PhasorMeasurement busDtoBusFFlow = new PhasorMeasurement()
            //{
            //    Type = PhasorType.CurrentPhasor,
            //    BaseKV = new VoltageLevel(1, 230)
            //};
            //PhasorMeasurement busAtoBusEFlow = new PhasorMeasurement()
            //{
            //    Type = PhasorType.CurrentPhasor,
            //    BaseKV = new VoltageLevel(1, 230)
            //};
            //PhasorMeasurement busFtoBusEFlow = new PhasorMeasurement()
            //{
            //    Type = PhasorType.CurrentPhasor,
            //    BaseKV = new VoltageLevel(1, 230)
            //};
            //PhasorMeasurement busAtoBusGFlow = new PhasorMeasurement()
            //{
            //    Type = PhasorType.CurrentPhasor,
            //    BaseKV = new VoltageLevel(1, 230)
            //};
            //PhasorMeasurement busHtoBusGFlow = new PhasorMeasurement()
            //{
            //    Type = PhasorType.CurrentPhasor,
            //    BaseKV = new VoltageLevel(1, 230)
            //};
            //PhasorMeasurement busDtoBusHFlow = new PhasorMeasurement()
            //{
            //    Type = PhasorType.CurrentPhasor,
            //    BaseKV = new VoltageLevel(1, 230)
            //};

            //busBtoBusAFlow.PerUnitComplexPhasor = (busBVoltage.PerUnitComplexPhasor - busAVoltage.PerUnitComplexPhasor) / (new Complex(0.0, 0.01));
            //busBtoBusCFlow.PerUnitComplexPhasor = (busBVoltage.PerUnitComplexPhasor - busCVoltage.PerUnitComplexPhasor) / (new Complex(0.0, 0.06));
            //busDtoBusCFlow.PerUnitComplexPhasor = (busDVoltage.PerUnitComplexPhasor - busCVoltage.PerUnitComplexPhasor) / (new Complex(0.0, 0.06));
            //busDtoBusFFlow.PerUnitComplexPhasor = (busDVoltage.PerUnitComplexPhasor - busFVoltage.PerUnitComplexPhasor) / (new Complex(0.0, 0.01));
            //busAtoBusEFlow.PerUnitComplexPhasor = (busAVoltage.PerUnitComplexPhasor - busEVoltage.PerUnitComplexPhasor) / (new Complex(0.0, 0.005));
            //busFtoBusEFlow.PerUnitComplexPhasor = (busFVoltage.PerUnitComplexPhasor - busEVoltage.PerUnitComplexPhasor) / (new Complex(0.0, 0.005));
            //busAtoBusGFlow.PerUnitComplexPhasor = (busAVoltage.PerUnitComplexPhasor - busGVoltage.PerUnitComplexPhasor) / (new Complex(0.0, 0.005));
            //busHtoBusGFlow.PerUnitComplexPhasor = (busHVoltage.PerUnitComplexPhasor - busGVoltage.PerUnitComplexPhasor) / (new Complex(0.0, 0.01));
            //busDtoBusHFlow.PerUnitComplexPhasor = (busDVoltage.PerUnitComplexPhasor - busHVoltage.PerUnitComplexPhasor) / (new Complex(0.0, 0.01));
            

            //Console.WriteLine("BusB.BusA: " + busBtoBusAFlow.Magnitude.ToString() + " " + (busBtoBusAFlow.AngleInDegrees).ToString());
            //Console.WriteLine("BusB.BusC: " + busBtoBusCFlow.Magnitude.ToString() + " " + (busBtoBusCFlow.AngleInDegrees).ToString());
            //Console.WriteLine("BusD.BusC: " + busDtoBusCFlow.Magnitude.ToString() + " " + (busDtoBusCFlow.AngleInDegrees).ToString());
            //Console.WriteLine("BusD.BusF: " + busDtoBusFFlow.Magnitude.ToString() + " " + (busDtoBusFFlow.AngleInDegrees).ToString());
            //Console.WriteLine("BusA.BusE: " + busAtoBusEFlow.Magnitude.ToString() + " " + (busAtoBusEFlow.AngleInDegrees).ToString());
            //Console.WriteLine("BusF.BusE: " + busFtoBusEFlow.Magnitude.ToString() + " " + (busFtoBusEFlow.AngleInDegrees).ToString());
            //Console.WriteLine("BusA.BusG: " + busAtoBusGFlow.Magnitude.ToString() + " " + (busAtoBusGFlow.AngleInDegrees).ToString());
            //Console.WriteLine("BusH.BusG: " + busHtoBusGFlow.Magnitude.ToString() + " " + (busHtoBusGFlow.AngleInDegrees).ToString());
            //Console.WriteLine("BusD.BusH: " + busDtoBusHFlow.Magnitude.ToString() + " " + (busDtoBusHFlow.AngleInDegrees).ToString());


            //Network network = Network.DeserializeFromXml(@"\\psf\Home\Documents\mc2\Linear State Estimation\EPG\SynchrophasorAnalytics.OfflineModule\x86\TransmissionLineGraphTestFull4.xml");
            //Network network = Network.DeserializeFromXml(@"\\psf\Home\Documents\mc2\Projects\EPG - WECC SDVCA\Data\July 20, 2014 Test Cases\shunt_test_model.xml");

            //RawMeasurements rawMeasurements = RawMeasurements.DeserializeFromXml(@"\\psf\Home\Documents\mc2\Projects\EPG - WECC SDVCA\Data\July 20, 2014 Test Cases\ShuntSeriesTestCase151.xml");
            //network.Initialize();

            //network.Model.InputKeyValuePairs.Clear();

            //for (int i = 0; i < rawMeasurements.Items.Count(); i++)
            //{
            //    network.Model.InputKeyValuePairs.Add(rawMeasurements.Items[i].Key, Convert.ToDouble(rawMeasurements.Items[i].Value));
            //}

            //network.Model.OnNewMeasurements();

            //Console.WriteLine(network.Model.ComponentList());

            //Dictionary<string, double> receivedMeasurements = network.Model.GetReceivedMeasurements();

            //foreach (KeyValuePair<string, double> keyValuePair in receivedMeasurements)
            //{
            //    Console.WriteLine(keyValuePair.Key + " " + keyValuePair.Value.ToString());
            //}

            //Console.WriteLine(rawMeasurements.Items.Count().ToString());
            //Console.WriteLine(receivedMeasurements.Count.ToString());
            //Console.WriteLine();

            //network.RunNetworkReconstructionCheck();

            //network.Model.DetermineActiveCurrentFlows();
            //network.Model.DetermineActiveCurrentInjections();

            //Console.WriteLine(network.Model.MeasurementInclusionStatusList());

            //network.Model.ResolveToObservedBusses();

            //Console.WriteLine(network.Model.ObservedBusses.Count);

            //foreach (Substation substation in network.Model.Substations)
            //{
            //    Console.WriteLine(substation.Graph.AdjacencyList.ToString());
            //}

            //network.Model.ResolveToSingleFlowBranches();

            //foreach (TransmissionLine transmissionLine in network.Model.TransmissionLines)
            //{
            //    Console.WriteLine();
            //    Console.WriteLine(transmissionLine.Name);
            //    Console.WriteLine("Has at least one flow path: " + transmissionLine.Graph.HasAtLeastOneFlowPath.ToString());
            //    Console.WriteLine(transmissionLine.Graph.DirectlyConnectedAdjacencyList.ToString());
            //    Console.WriteLine(transmissionLine.Graph.SeriesImpedanceConnectedAdjacencyList.ToString());
            //    Console.WriteLine(transmissionLine.Graph.RootNode.ToSubtreeString());
            //    List<SeriesBranchBase> seriesBranches = transmissionLine.Graph.SingleFlowPathBranches;
            //    foreach (SeriesBranchBase seriesBranch in seriesBranches)
            //    {
            //        seriesBranch.ToVerboseString();
            //    }
            //}

            //network.ComputeSystemState();
            //network.Model.ComputeEstimatedCurrentFlows();
            //network.Model.ComputeEstimatedCurrentInjections();

            //TransmissionLine transmissionLine = network.Model.Companies[0].Divisions[0].TransmissionLines[0];

            //foreach (Switch bypassSwitch in transmissionLine.Switches)
            //{
            //    bypassSwitch.IsInDefaultMode = false;
            //    bypassSwitch.ActualState = SwitchingDeviceActualState.Closed;
            //    if (bypassSwitch.InternalID == 1) { bypassSwitch.ActualState = SwitchingDeviceActualState.Closed; }
            //    Console.WriteLine(String.Format("ID:{0} Normally:{1} Actually:{2}", bypassSwitch.InternalID, bypassSwitch.NormalState, bypassSwitch.ActualState));
            //}

            //TransmissionLineGraph graph = new TransmissionLineGraph(transmissionLine);
            //graph.InitializeAdjacencyLists();
            //Console.WriteLine(graph.DireclyConnectedAdjacencyList.ToString());
            //graph.ResolveConnectedAdjacencies();
            //Console.WriteLine(graph.DireclyConnectedAdjacencyList.ToString());
            //Console.WriteLine(graph.SeriesImpedanceConnectedAdjacencyList.ToString());
            //if (graph.HasAtLeastOneFlowPath)
            //{
            //    Console.WriteLine(graph.SeriesImpedanceConnectedAdjacencyList.ToString());
            //    graph.InitializeTree();
            //    Console.WriteLine(graph.RootNode.ToSubtreeString());
            //    Console.WriteLine("Number of series branches: " + graph.SingleFlowPathBranches.Count);
            //    Console.WriteLine(graph.ResolveToSingleSeriesBranch().RawImpedanceParameters.ToString());
            //}
            //else
            //{
            //    Console.WriteLine("Graph does not have at least one flow path -> tree would be invalid!");
            //}
            //SequencedMeasurementSnapshotPathSet sequencedMeasurementSnapshotPathSet = new SequencedMeasurementSnapshotPathSet();
            //sequencedMeasurementSnapshotPathSet.MeasurementSnapshotPaths.Add(new MeasurementSnapshotPath("value"));

            //sequencedMeasurementSnapshotPathSet.SerializeToXml("\\\\psf\\Home\\Documents\\mc2\\Projects\\EPG - WECC SDVCA\\Data\\TestCases.xml");
            Network network = Network.DeserializeFromXml("C:\\Program Files\\openPDC\\TwoSubstationNetworkModelExample_hastransmissionline_outputmeasurekey.xml");
            network.Initialize();
            List<string> inputMeasurementKeys = new List<string>();
            foreach (VoltagePhasorGroup voltagePhasorGroup in network.Model.Voltages)
            {
                Console.WriteLine(voltagePhasorGroup.ToVerboseString());
                if (network.Model.AcceptsMeasurements)
                {
                    inputMeasurementKeys.Add(voltagePhasorGroup.PositiveSequence.Measurement.MagnitudeKey);
                    inputMeasurementKeys.Add(voltagePhasorGroup.PositiveSequence.Measurement.AngleKey);

                    if (network.PhaseConfiguration == PhaseSelection.ThreePhase)
                    {
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseA.Measurement.MagnitudeKey);
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseA.Measurement.AngleKey);
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseB.Measurement.MagnitudeKey);
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseB.Measurement.AngleKey);
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseC.Measurement.MagnitudeKey);
                        inputMeasurementKeys.Add(voltagePhasorGroup.PhaseC.Measurement.AngleKey);
                    }
                }
            }
            Console.WriteLine(inputMeasurementKeys.Count());

            List<string> test = network.Model.GetVoltagePhasorGroupInputKeys();
            List<string> test2 = network.Model.GetInputMeasurementKeys();
            
            Console.WriteLine(test.Count());
            Console.WriteLine(test2.Count()); 
            

            Console.ReadLine();
        }
    }
}
