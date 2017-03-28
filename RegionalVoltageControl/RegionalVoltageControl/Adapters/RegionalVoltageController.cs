
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using RegionalVoltageControl.NetworkModel;
using SynchrophasorAnalytics.Measurements;
using SynchrophasorAnalytics.Modeling;

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

        private void Initialize()
        {

            m_systemModel = new Network();

        }

        #endregion

    }

    class Program
    {
        public static void Main()
        {

            RegionalVoltageControllerAdapter RVC = new RegionalVoltageControllerAdapter();

        }



    }


}
