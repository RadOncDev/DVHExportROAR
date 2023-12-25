using System;
using System.Collections.Generic;
using DVHAnalysis;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ChuckDvhBatch
{
    public class MetricResultSet
    {
        public PlanningItem PlanningItem { get; set; }
        public Structure Structure { get; set; }

        public DVH DVH { get; set; }
        public double Volume { get; set; }
        public double Coverage { get; set; }
        public double MinDose { get; set; }
        public double MaxDose { get; set; }
        public double MeanDose { get; set; }
        public double MedianDose { get; set; }
        public double StdDevDose { get; set; }
        public double D0p05ccGy { get; set; }
        public double DC0p05ccGy { get; set; }
        public double V20Gycc { get; set; }    // V20Gy[cc]

        public double[,] VolumeDvh { get; set; }
        public double[,] DoseDvh { get; set; }

        public double[,] VolumeBioDvh025 { get; set; }  // Dx%(a/b=2.5)[EQD2Gy]
        public double[,] VolumeBioDvh050 { get; set; }  // Dx%(a/b=5.0)[EQD2Gy]
        public double[,] VolumeBioDvh100 { get; set; }  // Dx%(a/b=10.0)[EQD2Gy]

        public static MetricResultSet CalculateMetrics(
            PlanningItem plan, Structure structure, Dictionary<string, double> scalingFactors)
        {
            DVH dvh = CalculateDvh(plan, structure, scalingFactors);

            if (Math.Abs(dvh.Coverage) < 0.00001)
            {
                throw new Exception("Structure has no dose coverage.");
            }

            if (dvh.CurveData.Length == 0)
            {
                throw new Exception("Structure has no DVH data.");
            }

            double D0p05ccGy;
            double DC0p05ccGy;
            double V20Gycc;

            try
            {
                D0p05ccGy = CalculateDoseToVolume(0.05, dvh);
            }
            catch (Exception)
            {
                D0p05ccGy = double.NaN;
            }

            try
            {
                DC0p05ccGy = CalculateDoseComplementToVolume(0.05, dvh);
            }
            catch (Exception)
            {
                DC0p05ccGy = double.NaN;
            }

            try
            {
                V20Gycc = CalculateVolumeWithDose(20.0, dvh);
            }
            catch (Exception)
            {
                V20Gycc = double.NaN;
            }

            var volumeDvhCurve = new RelativeVolumeDvh(dvh).CalculateDvh();
            var doseDvhCurve = GetDoseDvhCurve(dvh);

            // Note: DVHAnalysis library doesn't set the Coverage on the bio-DVH (perhaps it should),
            // so as a workaround, use the coverage calculated by the standard DVH

            var bioDvh025 = CalculateBioDvh(plan, structure, 2.5, scalingFactors);
            bioDvh025.Coverage = dvh.Coverage;
            var volumeBioDvhCurve025 = new RelativeVolumeDvh(bioDvh025).CalculateDvh();

            var bioDvh050 = CalculateBioDvh(plan, structure, 5.0, scalingFactors);
            bioDvh050.Coverage = dvh.Coverage;
            var volumeBioDvhCurve050 = new RelativeVolumeDvh(bioDvh050).CalculateDvh();

            var bioDvh100 = CalculateBioDvh(plan, structure, 10.0, scalingFactors);
            bioDvh100.Coverage = dvh.Coverage;
            var volumeBioDvhCurve100 = new RelativeVolumeDvh(bioDvh100).CalculateDvh();

            return new MetricResultSet
            {
                PlanningItem = plan,
                Structure = structure,
                DVH = dvh,
                Coverage = dvh.Coverage,
                Volume = dvh.TotalVolume,
                MinDose = dvh.MinDose,
                MaxDose = dvh.MaxDose,
                MeanDose = dvh.MeanDose,
                MedianDose = dvh.MedianDose,
                StdDevDose = dvh.StdDevDose,
                D0p05ccGy = D0p05ccGy,
                DC0p05ccGy = DC0p05ccGy,
                V20Gycc = V20Gycc,
                VolumeDvh = volumeDvhCurve,
                DoseDvh = doseDvhCurve,
                VolumeBioDvh025 = volumeBioDvhCurve025,
                VolumeBioDvh050 = volumeBioDvhCurve050,
                VolumeBioDvh100 = volumeBioDvhCurve100
            };
        }

        private static DVH CalculateDvh(PlanningItem plan, Structure structure,
            Dictionary<string, double> scalingFactors)
        {
            EclipseData eclipseData = new EclipseData
            {
                Patient = plan.GetCourse().Patient,
                Course = plan.GetCourse(),
                Plan = plan,
                Structure = structure
            };

            var standardDvhModel = new StandardDVHModel
            {
                DoseType = DoseValuePresentation.Absolute,
                VolumeType = VolumePresentation.AbsoluteCm3,
                BinSize = 0.001,
            };

            var dvhModel = new DeliveredDoseDVHModel(standardDvhModel, scalingFactors);

            return dvhModel.Calculate(eclipseData);
        }

        private static DVH CalculateBioDvh(PlanningItem plan, Structure structure, double alphaBeta,
            Dictionary<string, double> scalingFactors)
        {
            EclipseData eclipseData = new EclipseData
            {
                Patient = plan.GetCourse().Patient,
                Course = plan.GetCourse(),
                Plan = plan,
                Structure = structure
            };

            var bioDvhModel = new LQBioDoseDVHModel
            {
                DoseType = DoseValuePresentation.Absolute,
                VolumeType = VolumePresentation.AbsoluteCm3,
                BinSize = 0.001,
                AlphaBeta = alphaBeta
            };

            var dvhModel = new DeliveredDoseDVHModel(bioDvhModel, scalingFactors);

            return dvhModel.Calculate(eclipseData);
        }

        private static double CalculateDoseToVolume(double volume, DVH dvh)
        {
            return new DoseToVolumeMetric {Volume = volume}.Calculate(dvh).Value;
        }

        private static double CalculateDoseComplementToVolume(double volume, DVH dvh)
        {
            return new DoseComplementToVolumeMetric {Volume = volume}.Calculate(dvh).Value;
        }

        private static double CalculateVolumeWithDose(double dose, DVH dvh)
        {
            return new VolumeWithDoseMetric {Dose = dose}.Calculate(dvh).Value;
        }

        private static double[,] GetDoseDvhCurve(DVH dvh)
        {
            double[] doses = GetRegularDoses(0.0, dvh.MaxDose, 0.1);

            // Dose, volume pairs
            double[,] results = new double[doses.Length, 2];

            for (int i = 0; i < doses.Length; i++)
            {
                double dose = doses[i];
                double volume = CalculateVolumeWithDose(dose, dvh);
                results[i, 0] = dose;
                results[i, 1] = volume;
            }

            return results;
        }

        private static double[] GetRegularDoses(double min, double max, double binSize)
        {
            List<double> doses = new List<double>();

            for (double x = min; x < max; x += binSize)
            {
                doses.Add(x);
            }

            // Always include the max
            doses.Add(max);

            return doses.ToArray();
        }
    }
}
