using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using SynchrophasorAnalytics.Measurements;
using SynchrophasorAnalytics.Modeling;

namespace RegionalVoltageControl.NetworkModel
{
    [Serializable()]
    public class Network
    {

        #region [ Private Members ]
        /// <summary>
        /// Network Measurements
        /// </summary>
        private List<VoltagePhasorGroup> m_voltages;


        /// <summary>
        /// Network Measurements
        /// </summary>
        private List<Switch> m_shuntBreakers;

        #endregion

        #region [ Properties ]

        #region [ Network Measurements ]
        [XmlArray("Voltages")]
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
        #endregion


        #region [ Network Components ]
        [XmlArray("ShuntBreakers")]
        public List<Switch> ShuntBreakers
        {
            get
            {
                return m_shuntBreakers;
            }
            set
            {
                m_shuntBreakers = value;
            }
        }


        #endregion
        #endregion

        #region [ Initialize he Objects Outside the Scope of Constructor ]

        private void Initialize()
        {
            m_shuntBreakers = new List<Switch>();
            m_voltages = new List<VoltagePhasorGroup>();
        }


        #endregion

        #region [ Constructors  ]
        public Network()
        {
            Initialize();
        }
        #endregion

        #region [ Xml Serialization/Deserialization methods ]
        public void SerializeToXml(string pathName)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Network));

                TextWriter writer = new StreamWriter(pathName);

                serializer.Serialize(writer, this);

                writer.Close();
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to Serialzie");
            }
        }

        public static Network DeserializeFromXml(string pathName)
        {
            try
            {
                Network voltVarController = null;

                XmlSerializer deserializer = new XmlSerializer(typeof(Network));

                StreamReader reader = new StreamReader(pathName);

                voltVarController = (Network)deserializer.Deserialize(reader);

                reader.Close();

                return voltVarController;
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to Deserialzie");
            }
        }

        #endregion

    }


}
