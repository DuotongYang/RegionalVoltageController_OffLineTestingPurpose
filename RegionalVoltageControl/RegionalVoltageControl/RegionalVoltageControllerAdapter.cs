
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using SynchrophasorAnalytics.Modeling;


namespace RegionalVoltageControl
{
	[Serializable()]
	public class RegionalVoltageControllerAdapter
	{
		#region [ Private Members ]
		private NetworkModel m_networkModel;
		#endregion


		#region [ Properties ]
		[XmlElement("Model")]
		public NetworkModel Model
		{
			get
			{
				return m_networkModel;
			}
			set
			{
				m_networkModel = value;
			}

		}


		#endregion
	}

	class Program
	{
		public static void Main()
		{

		}
		


	}


}
