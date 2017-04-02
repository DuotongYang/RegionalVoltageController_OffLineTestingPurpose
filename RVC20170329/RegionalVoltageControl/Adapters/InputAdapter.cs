using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RegionalVoltageControl.NetworkModel;
using ReadWriteCSV;
using VoltController.Adapters;

namespace RegionalVoltageControl.Adapters
{
    public class InputAdapter
    {
        private void ReadVoltageMeasurments(Network systemNetwork, string VoltageMeasurmentsPathName,int rowNumber)
        {   

            CsvAdapter csvRead = new CsvAdapter();
            csvRead.ReadCSV(VoltageMeasurmentsPathName);
            for(int i = 0; i < systemNetwork.Voltages.Count; i++)
            {   
                systemNetwork.Voltages[i].PositiveSequence.Measurement.MagnitudeKey = csvRead.Frame[1, i];
                systemNetwork.Voltages[i].PositiveSequence.Measurement.Magnitude = Convert.ToDouble(csvRead.Frame[rowNumber, i]);
            }
        }

        //private void ReadBreakerStatus(Network systemNetwork, string ShuntBreakerStatusPath, int rowNumber)
        //{
        //    CsvAdapter csvRead = new CsvAdapter();
        //    csvRead.ReadCSV(ShuntBreakerStatusPath);
        //    for (int i = 0; i < systemNetwork.ShuntBreakers.Count; i++)
        //    {
        //        if (csvRead.Frame[rowNumber, i].Equals("Open"))
        //        {
        //            systemNetwork.ShuntBreakers[i].ManuallySwitchTo()                }
                   
        //    }
        //}

       

    }
}
