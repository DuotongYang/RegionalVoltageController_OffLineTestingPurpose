
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
        private string m_logMeassage;

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


        #endregion

        #region [ Constructor ]
        public RegionalVoltageControllerAdapter()
        {
            Initialize();
        }

        #endregion

        #region [ Private Methods ]

        private void InitializeInputFrame()
        {
            m_inputFrame = new 
        }

        #endregion

        #region [ Public Methods ]

        private void Initialize()
        {

            m_systemModel = new Network();
            m_logMeassage = null;

        }

        #endregion

    }
}
