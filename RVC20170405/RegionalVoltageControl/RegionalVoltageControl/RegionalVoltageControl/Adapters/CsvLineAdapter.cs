using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ReadWriteCSV;
using RegionalVoltageControl.Adapters;

namespace RegionalVoltageControl.Adapters
{
    public class CsvLineAdapter

    {
        #region [ Members ]

        private string[] m_lineArray = new string[] { };

        #endregion

        #region [ Properties ]
        

        public string[] LineArray
        {
            get
            {
                return m_lineArray;
            }
            set
            {
                m_lineArray = value;
            }
        }

        #endregion

        #region [ Constructor ]
        public CsvLineAdapter()
        {
            string[] m_lineArray = new string[] { };
        }
        #endregion

        #region [ Public Methods ]

        public string[] ReadCSVLine(String path, int indexOfLine)
        {

            string line = File.ReadLines(path).ElementAtOrDefault(indexOfLine);

            LineArray = line.Split(',');
            return LineArray;

        }

        #endregion

    }

}
