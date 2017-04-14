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
using RegionalVoltageControl.ControlActions;


namespace RegionalVoltageControl.ControlActions
{
    [Serializable()]
    public class CtrlActions
    {
        #region [ Private Members ]
        private string m_securityStatus;
        private string m_ctrlTreeName;
        private int m_index;
        private int m_actionsCounter;
        private string m_ctrlCombinationsString;
        private int[] m_ctrlCombinations;

        #endregion 

        #region [ Properties ]
        [XmlAttribute("SecurityStatus")]
        public string SecurityStatus
        {
            get
            {
                return m_securityStatus;
            }
            set
            {
                m_securityStatus = value;
            }
        }

        [XmlAttribute("CtrlTreeName")]
        public string CtrlTreeName
        {
            get
            {
                return m_ctrlTreeName;
            }
            set
            {
                m_ctrlTreeName = value;
            }
        }

        [XmlAttribute("CtrlIndex")]
        public int Index
        {
            get
            {
                return m_index;
            }
            set
            {
                m_index = value;
            }
        }

        [XmlAttribute("ActionsCounter")]
        public int ActionsCounter
        {
            get
            {
                return m_actionsCounter;
            }
            set
            {
                m_actionsCounter = value;
            }
        }
        [XmlAttribute("CtrlCombinationsString")]
        public string CtrlCombinationsString
        {
            get
            {
                return m_ctrlCombinationsString;
            }
            set
            {
                m_ctrlCombinationsString = value;
            }
        }
        [XmlIgnore()]
        public int[] CtrlCombinations
        {
            get
            {
                return m_ctrlCombinations;
            }
            set
            {
                m_ctrlCombinations = value;
            }
        }

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
