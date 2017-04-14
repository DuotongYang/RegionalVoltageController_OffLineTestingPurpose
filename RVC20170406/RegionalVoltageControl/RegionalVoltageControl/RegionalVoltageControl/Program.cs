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
            string inputdatafolder = (@"C:\Users\Duotong\Documents\GitHub\RegionalVoltageController\RVC20170406\RegionalVoltageControl\RegionalVoltageControl\RegionalVoltageControl\Data\\");
            string logsfolder = (@"C:\Users\Duotong\Documents\GitHub\RegionalVoltageController\RVC20170406\RegionalVoltageControl\RegionalVoltageControl\RegionalVoltageControl\Log\\");
            string inputFilePath = (@"C:\Users\Duotong\Documents\GitHub\RegionalVoltageController\RVC20170406\RegionalVoltageControl\Test1\\");
            string configurationPathName = inputFilePath + "configuration.xml";
            string treeFileFolder = (@"C:\Users\Duotong\Documents\GitHub\RegionalVoltageController\RVC20170405\RegionalVoltageControl\RegionalVoltageControl\RegionalVoltageControl\DecisionTreeModels");
            string pythonScriptFolder = (@"C:\Users\Duotong\Documents\GitHub\RegionalVoltageController\RVC20170406\RegionalVoltageControl\Test1\PythonScript\\");
            string cmdForControl = "ControlSystem.py";

            // Configure in nie's background settings
            //string mainfolder = (@"C:\Users\niezj\Desktop\16b_Spring\Dominion\RegionalVoltageControl20170402\RegionalVoltageControl\");
            //inputdatafolder = System.IO.Path.Combine(mainfolder, @"RegionalVoltageControl\RegionalVoltageControl\Data\");
            //logsfolder = System.IO.Path.Combine(mainfolder, @"RegionalVoltageControl\RegionalVoltageControl\Log\");
            //inputFilePath = System.IO.Path.Combine(mainfolder, @"Test1\");
            //configurationPathName = System.IO.Path.Combine(inputFilePath, "configuration.xml");

            RegionalVoltageControllerAdapter RVCAdapter = new RegionalVoltageControllerAdapter();
            InputAdapter inputAdapter = new InputAdapter();
            Network Frame = new Network();

            #endregion

            #region [ initialize Regional Voltage Controller ]
            RVCAdapter.ConfigurationPathName = configurationPathName;
            RVCAdapter.TreeFileFolder = treeFileFolder;
            RVCAdapter.PythonCMDForControl = pythonScriptFolder + cmdForControl;
            RVCAdapter.Initialize();
            
            #endregion

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine("Case {0}", i+1);
                int rowNumber = i + 1;

                string inputLogName = inputdatafolder + String.Format("{0:yyyy-MM-dd  hh-mm-ss}_{1}", DateTime.UtcNow, rowNumber) + ".xml";
                string outputDecision = logsfolder + String.Format("{0:yyyy-MM-dd  hh-mm-ss}_{1}", DateTime.UtcNow, rowNumber) + ".xml";
                Frame = inputAdapter.ReadFrame(inputdatafolder,inputFilePath, rowNumber);
                RVCAdapter.PublishFrame(Frame);
                RVCAdapter.InputFrame.SerializeToXml(inputLogName);
                RVCAdapter.SerializeToXml(outputDecision);                                  

            }
            Console.ReadLine();

        }

    }
}
