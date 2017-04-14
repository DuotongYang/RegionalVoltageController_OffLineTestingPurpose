using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using RegionalVoltageControl.Measurements;
using RegionalVoltageControl.Modeling;

namespace RegionalVoltageControl.NetworkModel
{
    [Serializable()]
    public class Network
    {

        #region [ Private Members ]

        /// <summary>
        /// KeyValuePairs
        /// </summary>
        private Dictionary<string, object> m_rawKeyValuePairs;


        /// <summary>
        /// Network Measurements
        /// </summary>
        private List<Phasor> m_voltages;


        /// <summary>
        /// Network Measurements
        /// </summary>
        private List<CapBank> m_shuntBreakers;

        #endregion

        #region [ Properties ]

        #region [ Network Measurements ]
        [XmlArray("Voltages")]
        public List<Phasor> Voltages
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
        //[XmlArray("ShuntBreakers")]
        public List<CapBank> ShuntBreakers
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

        #region [ KeyValuePairs ]
        [XmlIgnore()]
        public Dictionary<string, object> RawkeyValuePairs
        {
            get
            {
                return m_rawKeyValuePairs;
            }
            set
            {
                m_rawKeyValuePairs = value;
            }
        }

        #endregion

        #endregion

        #region [ Initialize the Objects Outside the Scope of Constructor ]

        private void Initialize()
        {
            m_voltages = new List<Phasor>();
            m_shuntBreakers = new List<CapBank>();
            m_rawKeyValuePairs = new Dictionary<string, object>();
        }


        #endregion

        #region [ Constructors ]
        public Network()
        {
            Initialize();
        }
        #endregion

        #region [ Private Methods ]
        private void InsertVoltageMeasurements()
        {
            foreach (Phasor voltage in m_voltages)
            {
                object value = 0;
                if (m_rawKeyValuePairs.TryGetValue(voltage.Measurement.MagnitudeKey, out value))
                {
                    voltage.Measurement.Magnitude = (double)value;
                    value = 0;
                }
            }

        }

        private void InsertCapBankSwtchStatus()
        {
            foreach (CapBank shuntBreaker in m_shuntBreakers)
            {
                object value = 0;
                if (m_rawKeyValuePairs.TryGetValue(shuntBreaker.CapBankSwitchKey, out value))
                {
                    shuntBreaker.CapBankSwitchStatus = (string)value;
                    value = 0;
                }
            }
        }



        #endregion

        #region [ Public Method ]

        public void LinkNetworkComponents(int numberOfPhasors, int numberofShuntBreakers)
        {
            for(int i = 0;  i < numberOfPhasors; i ++ )
            {
                Phasor voltage = new Phasor();
                Voltages.Add(voltage);
            }

            for (int i = 0; i < numberofShuntBreakers; i++)
            {
                CapBank shuntBreaker = new CapBank();
                ShuntBreakers.Add(shuntBreaker);

            }

        }

        public void OnNewMeasurements()
        {
            InsertVoltageMeasurements();
            InsertCapBankSwtchStatus();

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
