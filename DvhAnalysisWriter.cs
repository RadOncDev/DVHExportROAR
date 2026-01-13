using System;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ChuckDvhBatch
{
    public class DvhAnalysisWriter
    {
        private const char OutputFieldSeparator = '\t';

        private static bool ifcGy = SystemDoseUnit_TestResult.SystemDoseUnit == DoseValue.DoseUnit.cGy;

        public static void WriteToConsole(MetricResultSet results)
        {
            if(ifcGy == false && SystemDoseUnit_TestResult.SystemDoseUnit != DoseValue.DoseUnit.Gy)
            {
                Console.Error.WriteLine($"******************** Fatal Error: System Dose Unit is neither Gy or cGy *********************");
            }

            var planningItem = results.PlanningItem;
            var structure = results.Structure;

            const char ofs = OutputFieldSeparator;
            string planUIDs = planningItem is PlanSetup
                ? ((PlanSetup)planningItem).UID
                : GetPlanSetupUids((PlanSum)planningItem);

            if(ifcGy == true)
            {
                results.D0p05ccGy /= 100;
                results.DC0p05ccGy /= 100;

                results.MinDose /= 100;
                results.MaxDose /= 100;
                results.MeanDose /= 100;
                results.MedianDose /= 100;
                results.StdDevDose /= 100;
            }

            var D0p05ccGy = double.IsNaN(results.D0p05ccGy) ? "" : results.D0p05ccGy.ToString("F4");
            var DC0p05ccGy = double.IsNaN(results.DC0p05ccGy) ? "" : results.DC0p05ccGy.ToString("F4");
            var V20Gycc = double.IsNaN(results.V20Gycc) ? "" : results.V20Gycc.ToString("F4");

            Console.WriteLine(
                $"{planningItem.GetCourse().Patient.Id}{ofs}" +
                $"{planningItem.GetCourse().Id}{ofs}" +
                $"{planningItem.Id}{ofs}" +
                $"{(planningItem is PlanSum ? "1" : "0")}{ofs}" +
                $"{planUIDs}{ofs}" +
                $"{structure.Id}{ofs}" +
                $"{results.Volume:F4}{ofs}" +
                $"{results.MinDose:F4}{ofs}" +
                $"{results.MaxDose:F4}{ofs}" +
                $"{results.MeanDose:F4}{ofs}" +
                $"{results.MedianDose:F4}{ofs}" +
                $"{results.StdDevDose:F4}{ofs}" +
                $"{D0p05ccGy}{ofs}" +
                $"{DC0p05ccGy}{ofs}" +
                $"{V20Gycc}{ofs}" +
                $"{results.Coverage:F4}{ofs}" +
                $"{FormatDvhCurveOutput(results.VolumeDvh, results.Coverage * 100.0)}{ofs}" +
                $"{FormatDvhCurveOutput(results.DoseDvh, results.Volume * results.Coverage)}{ofs}" +
                $"{FormatDvhCurveOutput(results.VolumeBioDvh025, results.Coverage * 100.0)}{ofs}" +
                $"{FormatDvhCurveOutput(results.VolumeBioDvh050, results.Coverage * 100.0)}{ofs}" +
                $"{FormatDvhCurveOutput(results.VolumeBioDvh100, results.Coverage * 100.0)}"
                );
            Console.Out.Flush();
        }

        private static string GetPlanSetupUids(PlanSum planSum)
        {
            if (planSum.PlanSetups == null)
            {
                return string.Empty;
            }

            return string.Join(";", planSum.PlanSetups.Select(p => p.UID));
        }

        private static string FormatDvhCurveOutput(double[,] dvhCurve, double volume)
        {
            StringBuilder sb = new StringBuilder();

            if (DvhCurveNeedsFirstPoint(dvhCurve, volume))
            {
                sb.Append($"0.0000,{volume:F4};");
            }

            for (int i = 0; i < dvhCurve.GetLength(0); i++)
            {
                if(ifcGy == true)
                {
                    sb.AppendFormat("{0:F4},{1:F4}", dvhCurve[i, 0] / 100, dvhCurve[i, 1]);
                }
                else
                {
                    sb.AppendFormat("{0:F4},{1:F4}", dvhCurve[i, 0], dvhCurve[i, 1]);
                }

                // Except for the last dose/volume, add separator
                if (i != dvhCurve.GetLength(0) - 1)
                {
                    sb.Append(";");
                }
            }

            return sb.ToString();
        }

        private static bool DvhCurveNeedsFirstPoint(double[,] dvhCurve, double volume)
        {
            return !(DoublesAreAboutEqual(dvhCurve[0, 0], 0.0) && DoublesAreAboutEqual(dvhCurve[0, 1], volume));
        }

        private static bool DoublesAreAboutEqual(double x, double y)
        {
            // Large tolerance because this is used in printing up to four decimal places
            return Math.Abs(x - y) < 0.0001;
        }
    }
}
