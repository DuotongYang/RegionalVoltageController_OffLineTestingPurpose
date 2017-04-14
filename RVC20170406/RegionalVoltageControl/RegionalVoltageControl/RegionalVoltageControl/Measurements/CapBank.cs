using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace RegionalVoltageControl.Measurements
{
    [Serializable()]
    public class CapBank
    {
        #region [ Private Members ]

        const string DEFAULT_CAPBANKKEY = "Undefined";
        const string DEFAULT_CAPBANK = "Undefined";
        private string m_capBankSwitchStatus;
        private string m_capBankSwitchKey;

        #endregion

        #region [ Property ]
        [XmlAttribute("CapBankSwitchStatus")]
        public string CapBankSwitchStatus
        {
            get
            {
                return m_capBankSwitchStatus;
            }
            set
            {
                m_capBankSwitchStatus = value;
            }
        }

        [XmlAttribute("CapBankSwitchKey")]
        public string CapBankSwitchKey
        {
            get
            {
                return m_capBankSwitchKey;
            }
            set
            {
                m_capBankSwitchKey = value;
            }
        }

        #endregion

       public CapBank()
        {
            CapBankSwitchKey = DEFAULT_CAPBANKKEY;
            CapBankSwitchStatus = DEFAULT_CAPBANK;
        }

    }
}
