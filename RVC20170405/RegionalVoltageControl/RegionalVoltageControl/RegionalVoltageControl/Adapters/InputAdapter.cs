using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RegionalVoltageControl.NetworkModel;
using RegionalVoltageControl.Measurements;

namespace RegionalVoltageControl.Adapters
{
    public class InputAdapter
    {
        //const string VOLT_FILENAME = "Voltages.csv";
        const string VOLT_FILENAME = "insecureVoltages.csv";
        const string CAP_FILENAME = "CapBanks.csv";
        const int numberOfPhasors = 118;
        const int numberofShuntBreakers = 6;


        #region [ Private Methods ]
        private void ReadVoltageMeasurments(Network systemNetwork, string inputFilePath, int rowNumber)
        {
            string VoltageMeasurmentsPathName = inputFilePath + VOLT_FILENAME;
            CsvLineAdapter csvReadMagnitude = new CsvLineAdapter();
            CsvLineAdapter csvReadMagnitudeKey = new CsvLineAdapter();
            csvReadMagnitudeKey.ReadCSVLine(VoltageMeasurmentsPathName, 0);
            csvReadMagnitude.ReadCSVLine(VoltageMeasurmentsPathName, rowNumber);


            for (int i = 0; i < systemNetwork.Voltages.Count; i++)
            {   
                
                systemNetwork.Voltages[i].Measurement.MagnitudeKey = csvReadMagnitudeKey.LineArray[i];
                systemNetwork.Voltages[i].Measurement.Magnitude = Convert.ToDouble(csvReadMagnitude.LineArray[i]);
            }
        }

        private void ReadShuntBreakers(Network systemNetwork, string inputFilePath, int rowNumber)
        {
            string ShuntBreakersPathname = inputFilePath + "CapBanks.csv";
            CsvLineAdapter csvReadBreakerStatus = new CsvLineAdapter();
            CsvLineAdapter csvReadBreakerKey = new CsvLineAdapter();
            csvReadBreakerKey.ReadCSVLine(ShuntBreakersPathname, 0);
            csvReadBreakerStatus.ReadCSVLine(ShuntBreakersPathname, rowNumber);

            for (int i = 0; i < systemNetwork.ShuntBreakers.Count; i++ )
            {
                systemNetwork.ShuntBreakers[i].CapBankSwitchKey = csvReadBreakerKey.LineArray[i];
                systemNetwork.ShuntBreakers[i].CapBankSwitchStatus = csvReadBreakerStatus.LineArray[i];
            }

        }

        private void CreateDictionary(Network systemNetwork)
        {
            systemNetwork.RawkeyValuePairs = new Dictionary<string, dynamic>();


            // Add Voltage Phasor
            foreach (Phasor voltage in systemNetwork.Voltages)
            {

                systemNetwork.RawkeyValuePairs.Add(voltage.Measurement.MagnitudeKey, voltage.Measurement.Magnitude);


            }

            // Add ShuntBreaker Status
            foreach (CapBank shuntBreaker in systemNetwork.ShuntBreakers)
            {

                systemNetwork.RawkeyValuePairs.Add(shuntBreaker.CapBankSwitchKey, shuntBreaker.CapBankSwitchStatus);

            }
        }

        #endregion

        #region [ Public Methods ]
        public Network ReadFrame(string inputdatafolder, string fileName, int rowNumber)
        {
            Network frame = new Network();
            frame.LinkNetworkComponents(numberOfPhasors,numberofShuntBreakers);
            ReadVoltageMeasurments(frame, fileName, rowNumber);
            ReadShuntBreakers(frame, fileName, rowNumber);
            CreateDictionary(frame);

            return frame;
        }

        #endregion
    }
}
