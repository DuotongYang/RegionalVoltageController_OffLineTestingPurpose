
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

        #endregion

        #region [ Properties ]
        [XmlArray("SystemModel")]
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

        [XmlIgnore()]
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



            #region [Create Control Decision  ]





            #endregion


            #region [ clear KVP for next frame]
            m_inputFrame.RawkeyValuePairs.Clear();
            #endregion
        }

        #endregion

    }
}
