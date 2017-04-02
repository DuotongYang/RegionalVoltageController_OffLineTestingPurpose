using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SynchrophasorAnalytics.Measurements;

namespace SynchrophasorAnalyticsUnitTests.MeasurementsTests
{
    [TestClass]
    public class StatusWordTest
    {
        #region [ Constants ]

        /// <summary>
        /// The maximum value that the C37.118 status word can have. It is equivalent to 1111111111111111 in binary.
        /// </summary>
        private static int MAXIMUM_VALUE = 65535;

        private static int DEFAULT_INTERNAL_ID = 0;
        private static int DEFAULT_NUMBER = 0;
        private static string DEFAULT_DESCRIPTION = "Undefined";
        private static string DEFAULT_MEASUREMENT_KEY = "Undefined";

        #endregion  

        #region [ Private Members ]

        private int m_internalID;
        private int m_number;
        private string m_description;
        private string m_inputMeasurementKey;
        private StatusWord m_statusWord;

        #endregion
 
        [TestInitialize]
        public void InitializeStatusWordTest()
        {
            m_internalID = DEFAULT_INTERNAL_ID;
            m_number = DEFAULT_NUMBER;
            m_description = DEFAULT_DESCRIPTION;
            m_inputMeasurementKey = DEFAULT_MEASUREMENT_KEY;
            m_statusWord = new StatusWord();
        }

        [TestMethod]
        public void StatusWord_ConstructorTest()
        {
            StatusWord statusWord = new StatusWord();

        }

        [TestMethod]
        public void StatusWord_InternalID_AccessorTest()
        {
            StatusWord statusWord = new StatusWord();
            statusWord.InternalID = 5;
            Assert.AreEqual(statusWord.InternalID, 5);
        }


    }
}

