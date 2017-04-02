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
        //private List<Switch> m_shuntBreakers;

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
        //public List<Switch> ShuntBreakers
        //{
        //    get
        //    {
        //        return m_shuntBreakers;
        //    }
        //    set
        //    {
        //        m_shuntBreakers = value;
        //    }
        //}
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

        #region [ Initialize he Objects Outside the Scope of Constructor ]

        private void Initialize()
        {
            //m_shuntBreakers = new List<Switch>();
            m_voltages = new List<Phasor>();
        }


        #endregion

        #region [ Constructors ]
        public Network()
        {
            Initialize();
        }
        #endregion

        #region [ Private Methods ]
        public void InsertVoltageMeasurements()
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
        #endregion

        #region [ Public Method ]

        public void LinkNetworkComponents(int numberOfPhasors)
        {
            for(int i = 0;  i < numberOfPhasors; i ++ )
            {
                Phasor voltage = new Phasor();
                Voltages.Add(voltage);
            }

            //for(int i = 0; i < numberofShuntBreakers; i++ )
            //{
            //    Switch shuntBreaker = new Switch();
            //    ShuntBreakers.Add(shuntBreaker); 
                   
            //}

        }

        public void OnNewMeasurements()
        {
            //InsertVoltageMeasurements();
            //InsertShuntBreakers();
            
        }

    
        //public void ToggleSwitchingDeviceStatus(string switchingDeviceName, SwitchingDeviceActualState desiredActualState)
        //{
        //    Dictionary<string, SwitchingDeviceBase> switchingDevices = m_shuntBreakers.ToDictionary(X => X.Name, x => x);
        //    if (switchingDevices.TryGetValue(switchingDeviceName, out switchingDevice))
        //    {
        //        switchiongDevice.
        //    }

        //}

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
