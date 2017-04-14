using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using RegionalVoltageControl.Measurements;
using RegionalVoltageControl.Modeling;

namespace RegionalVoltageControl.Measurements
{
    [Serializable()]
    public class PhasorMeasurement: PhasorBase
    {
        #region [ Private Constants ]
        private const string DEFAULT_MEASUREMENT_KEY = "Undefined";


        #endregion

        #region [ Private Members ]

        private double m_measurementVariance;
        private bool m_measurementShouldBeCalibrated;
        private double m_rcf;
        private double m_pacf;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// A flag which represents whether or not to include the <see cref="SynchrophasorAnalytics.Measurements.PhasorMeasurement"/> in the state estimator.
        /// </summary>
        [XmlIgnore()]
        public bool IncludeInEstimator
        {
            get
            {
                if (MeasurementWasReported == false)
                {
                    return false;
                }
                else if (Type == PhasorType.VoltagePhasor && Magnitude < (0.2 * (BaseKV.Value * 1000)))
                {
                    return false;
                }
                else if (Type == PhasorType.CurrentPhasor && Magnitude < 10)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// The measurement variance for the <see cref="SynchrophasorAnalytics.Measurements.PhasorMeasurement"/>.
        /// </summary>
        [XmlAttribute("Variance")]
        public double MeasurementVariance
        {
            get
            {
                return m_measurementVariance;
            }
            set
            {
                m_measurementVariance = value;
            }
        }

        /// <summary>
        /// The measurement covariance
        /// </summary>
        [XmlIgnore]
        public double MeasurementCovariance
        {
            get
            {
                return m_measurementVariance * m_measurementVariance;
            }
        }

        /// <summary>
        /// The ratio correction factor (RCF) for the <see cref="SynchrophasorAnalytics.Measurements.PhasorMeasurement"/>.
        /// </summary>
        [XmlAttribute("RCF")]
        public double RCF
        {
            get
            {
                return m_rcf;
            }
            set
            {
                m_rcf = value;
            }
        }

        /// <summary>
        /// The phase angle correction factor (PACF) for the <see cref="SynchrophasorAnalytics.Measurements.PhasorMeasurement"/>.
        /// </summary>
        [XmlAttribute("PACF")]
        public double PACF
        {
            get
            {
                return m_pacf;
            }
            set
            {
                m_pacf = value;
            }
        }

        /// <summary>
        /// A flag representing whether or not to calibrate the measurement using the RCF and PACF during insertion.
        /// </summary>
        [XmlAttribute("Calibrated")]
        public bool MeasurementShouldBeCalibrated
        {
            get
            {
                return m_measurementShouldBeCalibrated;
            }
            set
            {
                m_measurementShouldBeCalibrated = value;
            }
        }

        ///// <summary>
        ///// The <see cref="SynchrophasorAnalytics.Calibration.CalibrationSetting"/> type for the <see cref="SynchrophasorAnalytics.Measurements.PhasorMeasurement"/>. Determines how the measurement is included in the calibration algorithm.
        ///// </summary>
        //[XmlAttribute("CalibrationType")]
        //public CalibrationSetting InstrumentTransformerCalibrationSetting
        //{
        //    get
        //    {
        //        return m_calibrationSetting;
        //    }
        //    set
        //    {
        //        m_calibrationSetting = value;
        //    }
        //}

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// A blank constructor with default values.
        /// </summary>
        public PhasorMeasurement()
            : base(DEFAULT_MEASUREMENT_KEY, DEFAULT_MEASUREMENT_KEY, PhasorType.VoltagePhasor, new VoltageLevel())
        {

        }

        /// <summary>
        /// A constructor for the <see cref="SynchrophasorAnalytics.Measurements.PhasorMeasurement"/> which only specifies the measurement keys, phasor type, and base KV.
        /// </summary>
        /// <param name="magnitudeKey">The openPDC input measurement key for the <see cref="SynchrophasorAnalytics.Measurements.PhasorBase.Magnitude"/> of the <see cref="SynchrophasorAnalytics.Measurements.PhasorMeasurement"/>.</param>
        /// <param name="angleKey">The openPDC input measurement key for the <see cref="SynchrophasorAnalytics.Measurements.PhasorBase.AngleInDegrees"/> of the <see cref="SynchrophasorAnalytics.Measurements.PhasorMeasurement"/>.</param>
        /// <param name="type">Specifies whether the phasor measurement is a current phasor or a
        /// voltage phasor  or complex power with the <see cref="SynchrophasorAnalytics.Measurements.PhasorType"/> enumeration, either <see cref="SynchrophasorAnalytics.Measurements.PhasorType.VoltagePhasor"/>, 
        /// <see cref="PhasorType.CurrentPhasor"/>, or <see cref="SynchrophasorAnalytics.Measurements.PhasorType.ComplexPower"/>.</param>
        /// <param name="baseKV">The <see cref="SynchrophasorAnalytics.Modeling.VoltageLevel"/> of the phasor measurement.</param>
        public PhasorMeasurement(string magnitudeKey, string angleKey, PhasorType type, VoltageLevel baseKV)
            : base(magnitudeKey, angleKey, type, baseKV)
        {
        }

        ///// <summary>
        ///// A constructor for the <see cref="SynchrophasorAnalytics.Measurements.PhasorMeasurement"/> which only specifies the measurement keys, phasor type, base KV and measurement variance.
        ///// </summary>
        ///// <param name="magnitudeKey">The openPDC input measurement key for the <see cref="SynchrophasorAnalytics.Measurements.PhasorBase.Magnitude"/> of the <see cref="SynchrophasorAnalytics.Measurements.PhasorMeasurement"/>.</param>
        ///// <param name="angleKey">The openPDC input measurement key for the <see cref="SynchrophasorAnalytics.Measurements.PhasorBase.AngleInDegrees"/> of the <see cref="SynchrophasorAnalytics.Measurements.PhasorMeasurement"/>.</param>
        ///// <param name="type">Specifies whether the phasor measurement is a current phasor or a voltage phasor  or complex power with the <see cref="SynchrophasorAnalytics.Measurements.PhasorType"/> enumeration, either <see cref="SynchrophasorAnalytics.Measurements.PhasorType.VoltagePhasor"/>, <see cref="SynchrophasorAnalytics.Measurements.PhasorType.CurrentPhasor"/>, or <see cref="SynchrophasorAnalytics.Measurements.PhasorType.ComplexPower"/>.</param>
        ///// <param name="baseKV">The <see cref="SynchrophasorAnalytics.Modeling.VoltageLevel"/> of the phasor measurement.</param>
        ///// <param name="variance">The measurement variance in per unit.</param>
        //public PhasorMeasurement(string magnitudeKey, string angleKey, PhasorType type, VoltageLevel baseKV, double variance)
        //    : this(magnitudeKey, angleKey, type, baseKV, variance)
        //{
        //}

        ///// <summary>
        ///// A constructor for the <see cref="SynchrophasorAnalytics.Measurements.PhasorMeasurement"/> which specifies the measurement keys, phasor type, base KV, measurement variance as well as RCF and PACF.
        ///// </summary>
        ///// <param name="magnitudeKey">The openPDC input measurement key for the <see cref="SynchrophasorAnalytics.Measurements.PhasorBase.Magnitude"/> of the <see cref="SynchrophasorAnalytics.Measurements.PhasorMeasurement"/>.</param>
        ///// <param name="angleKey">The openPDC input measurement key for the <see cref="SynchrophasorAnalytics.Measurements.PhasorBase.AngleInDegrees"/> of the <see cref="SynchrophasorAnalytics.Measurements.PhasorMeasurement"/>.</param>
        ///// <param name="type">Specifies whether the phasor measurement is a current phasor or a
        ///// voltage phasor  or complex power with the <see cref="SynchrophasorAnalytics.Measurements.PhasorType"/> enumeration, either <see cref="SynchrophasorAnalytics.Measurements.PhasorType.VoltagePhasor"/>, 
        ///// <see cref="SynchrophasorAnalytics.Measurements.PhasorType.CurrentPhasor"/>, or <see cref="SynchrophasorAnalytics.Measurements.PhasorType.ComplexPower"/>.</param>
        ///// <param name="baseKV">The <see cref="SynchrophasorAnalytics.Modeling.VoltageLevel"/> of the phasor measurement.</param>
        ///// <param name="variance">The measurement variance in per unit.</param>
        ///// <param name="rcf">The <b>Ratio Correction Factor (RCF)</b> for the measurement.</param>
        ///// <param name="pacf">The <b>Phase Angle Correction Factor (PACF)</b> for the measurementin degrees</param>
        //public PhasorMeasurement(string magnitudeKey, string angleKey, PhasorType type, VoltageLevel baseKV, double variance, double rcf, double pacf)
        //    : this(magnitudeKey, angleKey, type, baseKV, variance, rcf, pacf, CalibrationSetting.Inactive)
        //{
        //}

        ///// <summary>
        ///// the designated constructor for the <see cref="synchrophasoranalytics.measurements.phasormeasurement"/> class.
        ///// </summary>
        ///// <param name="magnitudekey">the openpdc input measurement key for the <see cref="synchrophasoranalytics.measurements.phasorbase.magnitude"/> of the <see cref="synchrophasoranalytics.measurements.phasormeasurement"/>.</param>
        ///// <param name="anglekey">the openpdc input measurement key for the <see cref="synchrophasoranalytics.measurements.phasorbase.angleindegrees"/> of the <see cref="synchrophasoranalytics.measurements.phasormeasurement"/>.</param>
        ///// <param name="type">specifies whether the phasor measurement is a current phasor or a
        ///// voltage phasor  or complex power with the <see cref="synchrophasoranalytics.measurements.phasortype"/> enumeration, either <see cref="synchrophasoranalytics.measurements.phasortype.voltagephasor"/>, 
        ///// <see cref="synchrophasoranalytics.measurements.phasortype.currentphasor"/>, or <see cref="synchrophasoranalytics.measurements.phasortype.complexpower"/>.</param>
        ///// <param name="basekv">the <see cref="synchrophasoranalytics.modeling.voltagelevel"/> of the phasor measurement.</param>
        ///// <param name="variance">the measurement variance in per unit.</param>
        ///// <param name="rcf">the <b>ratio correction factor (rcf)</b> for the measurement.</param>
        ///// <param name="pacf">the <b>phase angle correction factor (pacf)</b> for the measurement in degrees.</param>
        ///// <param name="calibrationsetting">the <see cref="synchrophasoranalytics.calibration.calibrationsetting"/> type for the <see cref="synchrophasoranalytics.measurements.phasormeasurement"/>. determines how the measurement is included in the calibration algorithm.</param>
        //public phasormeasurement(string magnitudekey, string anglekey, phasortype type, voltagelevel basekv, double variance, double rcf, double pacf)
        //    : base(magnitudekey, anglekey, type, basekv)
        //{
        //    m_measurementvariance = variance;
        //    m_rcf = rcf;
        //    m_pacf = pacf;

        //}
        #endregion

        #region [ Public Methods ]

        /// <summary>
        /// A string representation of the instance of the class.
        /// </summary>
        /// <returns>A string representation of the instance of the class.</returns>
        public override string ToString()
        {
            return base.ToString() + "," + m_measurementVariance.ToString() + "," + m_rcf.ToString() + "," + m_pacf.ToString() + "," + m_measurementShouldBeCalibrated.ToString(); /*+ "," + m_calibrationSetting.ToString();*/
        }

        /// <summary>
        /// A verbose descriptive string representation of the <see cref="PhasorMeasurement"/> class.
        /// </summary>
        /// <returns>A verbose descriptive string representation of the <see cref="PhasorMeasurement"/> class.</returns>
        public new string ToVerboseString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat("----- Phasor Measurement -------------------------------------------------------");
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat("            Type: " + Type.ToString() + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("         Base KV: " + BaseKV.ToString() + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("     MagnitudKey: " + MagnitudeKey + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("        AngleKey: " + AngleKey + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("       Magnitude: " + Magnitude.ToString() + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("           Angle: " + AngleInDegrees.ToString() + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("        Reported: " + MeasurementWasReported.ToString() + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("        Variance: " + m_measurementVariance.ToString() + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("             RCF: " + m_rcf.ToString() + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat("            PACF: " + m_pacf.ToString() + "{0}", Environment.NewLine);
            stringBuilder.AppendFormat(" ShouldCalibrate: " + m_measurementShouldBeCalibrated.ToString() + "{0}", Environment.NewLine);
            //stringBuilder.AppendFormat("CalibrationSttng: " + m_calibrationSetting.ToString() + "{0}", Environment.NewLine);
            stringBuilder.AppendLine();
            return stringBuilder.ToString();
        }

        #endregion  

    }
}
