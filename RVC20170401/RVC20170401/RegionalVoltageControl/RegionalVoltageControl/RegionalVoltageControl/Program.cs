using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RegionalVoltageControl.Adapters;
using RegionalVoltageControl.NetworkModel;

namespace RegionalVoltageControl
{
    class Program
    {
        static void Main()
        {
            #region [ initialize all data path ]
            string inputdatafolder = (@"C:\Users\Duotong\Documents\GitHub\RegionalVoltageController\RVC20170401\RVC20170401\RegionalVoltageControl\RegionalVoltageControl\RegionalVoltageControl\Data\\");
            string logsfolder = (@"C:\Users\Duotong\Documents\GitHub\RegionalVoltageController\RVC20170401\RVC20170401\RegionalVoltageControl\RegionalVoltageControl\RegionalVoltageControl\Log\\");
            string inputFilePath = (@"C:\Users\Duotong\Documents\GitHub\RegionalVoltageController\RVC20170401\RVC20170401\RegionalVoltageControl\Test1\\");
            string configurationPathName = inputFilePath + "configuration.xml";


            RegionalVoltageControllerAdapter RVCAdapter = new RegionalVoltageControllerAdapter();
            InputAdapter inputAdapter = new InputAdapter();
            Network Frame = new Network();

            #endregion

            #region [ initialize Regional Voltage Controller ]
            RVCAdapter.ConfigurationPathName = configurationPathName;
            RVCAdapter.Initialize();

            #endregion

            for (int i = 0; i < 2; i++)
            {
                int rowNumber = i + 1;

                string inputLogName = inputdatafolder + String.Format("{0:yyyy-MM-dd  hh-mm-ss}_{1}", DateTime.UtcNow, rowNumber) + ".xml";
                Frame = inputAdapter.ReadFrame(inputdatafolder,inputFilePath, rowNumber);
                RVCAdapter.PublishFrame(Frame);
                RVCAdapter.InputFrame.SerializeToXml(inputLogName);
                

            }

        }

    }
}
