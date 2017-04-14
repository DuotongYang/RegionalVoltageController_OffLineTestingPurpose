using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using RegionalVoltageControl.NetworkModel;
using RegionalVoltageControl.Measurements;
using Accord;
using Accord.IO;
using Accord.MachineLearning.DecisionTrees;
using Accord.MachineLearning.DecisionTrees.Learning;
using RegionalVoltageControl.Adapters;


namespace RegionalVoltageControl
{
    [Serializable()]
    public class RegionalVoltageControllerAdapter
    {
        #region [ Private Members ]
        /// <summary>
        /// Network Modeling
        /// </summary>
        private Network m_systemModel;
        private Network m_inputFrame;
        private string m_logMeassage;
        private string m_configurationPathName;
        private string m_treeFileFolder;
        private List<CtrlActions> m_DecisionList;

        #endregion

        #region [ Properties ]
        public string TreeFileFolder
        {
            get
            {
                return m_treeFileFolder;
            }
            set
            {
                m_treeFileFolder = value;
            }
        }

        [XmlIgnore()]
        public Network SystemModel
        {
            get
            {
                return m_systemModel;
            }
            set
            {
                m_systemModel = value;
            }
        }

        [XmlIgnore()]
        public Network InputFrame
        {
            get
            {
                return m_inputFrame;
            }
            set
            {
                m_inputFrame = value;
            }
        }

        [XmlAttribute("ConfigurationPathName")]
        public string ConfigurationPathName
        {
            get
            {
                return m_configurationPathName;
            }
            set
            {
                m_configurationPathName = value;
            }
        }

        [XmlArray("CtrlDecisionList")]
        public List<CtrlActions> DecisionList
        {
            get { return m_DecisionList; }
            set { m_DecisionList = value; }
        }

        
        #endregion

        #region [ Constructor ]
        public RegionalVoltageControllerAdapter()
        {
            InitializeInputFrame();
        }

        #endregion

        #region [ Private Methods ]

        private void InitializeInputFrame()
        {
            m_inputFrame = new Network();
        }

        #endregion

        #region [ Public Methods ]


        public void Initialize()
        {
            m_inputFrame = Network.DeserializeFromXml(m_configurationPathName);
            m_logMeassage = null;
            m_DecisionList = new List<CtrlActions>();
        }

        public void PublishFrame(Network Frame)
        {

            #region [ Read the Input KVP ]
            foreach (KeyValuePair<string, object> kvp in Frame.RawkeyValuePairs)
            {
                m_inputFrame.RawkeyValuePairs.Add(kvp.Key,kvp.Value);
            }
            #endregion

            #region [ Measurements Mapping ]

            m_inputFrame.OnNewMeasurements();

            #endregion

            #region [ Create Control Decision ]
            string treeFileFolder = (@"C:\Users\Duotong\Documents\GitHub\RegionalVoltageController\RVC20170405\RegionalVoltageControl\RegionalVoltageControl\RegionalVoltageControl\DecisionTreeModels");
            m_DecisionList = getLeastActionsSecureCtrl(m_inputFrame, treeFileFolder);
            foreach(CtrlActions CtrlDecision in m_DecisionList)
            {
                //CtrlDecision.SerializeToXml(@"C: \Users\niezj\Desktop\ctrlact.xml");
                Console.WriteLine("\t| {0},{1},{2} move", CtrlDecision.CtrlTreeName
                                                        , CtrlDecision.CtrlCombinationsString
                                                        , CtrlDecision.ActionsCounter);
            }

            //Console.ReadLine();

            #endregion

            #region [ clear KVP for next frame]
            m_inputFrame.RawkeyValuePairs.Clear();
            #endregion
        }

        // Search for all Feasible Control Combinations
        public List<CtrlActions> getLeastActionsSecureCtrl(Network frame, string TreeModelsPath)
        {
            List<CtrlActions> CtrlActionsList = new List<CtrlActions>();

            // Read the voltage measurement values of current frame
            double[] VoltMeasMagFrame = new double[frame.Voltages.Count];
            for (int i = 0; i < frame.Voltages.Count; i++)
            {
                VoltMeasMagFrame[i] = frame.Voltages[i].Measurement.Magnitude;
            }

            // Read the CapBank combination of current frame
            int[] CapBankCombinationFrame = new int[frame.ShuntBreakers.Count];
            for (int i = 0; i < frame.ShuntBreakers.Count; i++)
            {
                switch (frame.ShuntBreakers[i].CapBankSwitchStatus)
                {
                    case "Open":
                        CapBankCombinationFrame[i] = 0;
                        break;
                    case "Close":
                        CapBankCombinationFrame[i] = 1;
                        break;
                }
            }
            
            // Load trained decision-tree models (n = 63)
            for (int i = 0; i < Math.Pow(2, frame.ShuntBreakers.Count) - 1; i++)
            {
                CtrlActions ctrlact = new CtrlActions();
                ctrlact.Index = i + 1;

                // Get the control combinations according to the tree index ( Index = 7 => Combination 000111)
                ctrlact.CtrlCombinationsString = Convert.ToString(ctrlact.Index, 2)
                                                        .PadLeft(frame.ShuntBreakers.Count, '0'); // Convert to binary in a string                
                ctrlact.CtrlCombinations = ctrlact.CtrlCombinationsString.Select(c => int.Parse(c.ToString()))
                                                                         .ToArray();

                // Count the actions required for this combination
                int[] actionsCount = new int[frame.ShuntBreakers.Count];
                for (int degit = 0; degit <= frame.ShuntBreakers.Count - 1; degit++)
                {
                    actionsCount[degit] = Math.Abs(CapBankCombinationFrame[degit] - ctrlact.CtrlCombinations[degit]);
                }
                ctrlact.ActionsCounter = actionsCount.Sum();

                // Deserialize decision-tree model from the saved file
                string treename = String.Format("DecisionTreeModel_ControlForLowVoltage_{0}_train", ctrlact.Index);
                string treeFilePath = Path.Combine(TreeModelsPath, treename);
                DecisionTree tree = Serializer.Load<DecisionTree>(treeFilePath);
                ctrlact.CtrlTreeName = treename;

                if (CapBankCombinationFrame.SequenceEqual(ctrlact.CtrlCombinations)) 
                {
                    ctrlact.SecurityStatus = "Insecure: Current CapBank Combinations"; // Unchanged CapBank combination is insecure
                }
                else
                {
                    // Compute the actual tree outputs based on loaded decision-tree model                             
                    switch (tree.Decide(VoltMeasMagFrame))
                    {
                        case 1:
                            ctrlact.SecurityStatus = "Secure";
                            CtrlActionsList.Add(ctrlact);
                            break;
                        case 0:
                            ctrlact.SecurityStatus = "Insecure";
                            break;
                    }
                }                                             
            }

            // Check if any secure actions exist
            if (CtrlActionsList.Any()) 
            {
                // Remove control combinations of not-the-least actions
                int minactnum = frame.ShuntBreakers.Count;
                foreach (CtrlActions ctrlact in CtrlActionsList)
                {
                    if (minactnum > ctrlact.ActionsCounter)
                    {
                        minactnum = ctrlact.ActionsCounter;
                    }
                }
                CtrlActionsList.RemoveAll(a => a.ActionsCounter != minactnum);
            }
            else
            {
                Console.WriteLine("\t| No secure CapBank Control is available.");
            }
            
            return CtrlActionsList;
        }

        #endregion

        #region [ Xml Serialization/Deserialization methods ]
        public void SerializeToXml(string pathName)
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(RegionalVoltageControllerAdapter));

                    TextWriter writer = new StreamWriter(pathName);

                    serializer.Serialize(writer, this);

                    writer.Close();
                }
                catch (Exception exception)
                {
                    throw new Exception("Failed to Serialize");
                }
            }

            public static RegionalVoltageControllerAdapter DeserializeFromXml(string pathName)
            {
                try
                {
                    RegionalVoltageControllerAdapter regionalVoltageControllerAdapter = null;

                    XmlSerializer deserializer = new XmlSerializer(typeof(RegionalVoltageControllerAdapter));

                    StreamReader reader = new StreamReader(pathName);

                    regionalVoltageControllerAdapter = (RegionalVoltageControllerAdapter)deserializer.Deserialize(reader);

                    reader.Close();

                    return regionalVoltageControllerAdapter;
                }
                catch (Exception exception)
                {
                    throw new Exception("Failed to Deserialize");
                }
            }

            #endregion

    }

    [Serializable()]
    public class CtrlActions
    {
        #region [ Properties ]
        [XmlAttribute("SecurityStatus")]
        public string SecurityStatus { get; set; }
        [XmlAttribute("CtrlTreeName")]
        public string CtrlTreeName { get; set; }
        [XmlAttribute("CtrlIndex")]
        public int Index { get; set; }
        [XmlAttribute("ActionsCounter")]
        public int ActionsCounter { get; set; }
        [XmlAttribute("CtrlCombinationsString")]
        public string CtrlCombinationsString { get; set; }
        [XmlIgnore()]
        public int[] CtrlCombinations { get; set; }

        #endregion

        #region [ Xml Serialization/Deserialization methods ]
        public void SerializeToXml(string pathName)
        {
            using (var stream = new FileStream(pathName, FileMode.Create))
            {
                var serializer = new XmlSerializer(typeof(CtrlActions));
                serializer.Serialize(stream, this);
            }
        }
        #endregion
    }
    
    }
