using System;
using System.Collections.Generic;
using DVHAnalysis;
using VMS.TPS.Common.Model.API;

namespace ChuckDvhBatch
{
    public class DeliveredDoseDVHModel : DVHModel
    {
        private readonly DVHModel _dvhModel;
        private readonly Dictionary<string, double> _scalingFactors;

        private static readonly Random Random = new Random();

        public DeliveredDoseDVHModel(DVHModel dvhModel, Dictionary<string, double> scalingFactors)
        {
            _dvhModel = dvhModel;
            _scalingFactors = scalingFactors;
        }

        // Workaround to avoid using Cache
        public override double BinSize
        {
            get => Random.NextDouble();
            set { }
        }

        protected override DVH CalculateBase(EclipseData eclipseData)
        {
            var plan = eclipseData.Plan;

            var dvh = _dvhModel.Calculate(eclipseData);

            // Do not scale plan sum DVH because it should already be scaled
            // based on the PlanSum weights; for PlanSetups, scale the DVH
            if (plan is PlanSum)
                return dvh;

            // Scaling
            var scaleFactor = _scalingFactors[plan.Id];

            for (int i = 0; i < dvh.CurveData.Length; i++)
            {
                dvh.CurveData[i].Dose *= scaleFactor;
            }

            dvh.MinDose *= scaleFactor;
            dvh.MaxDose *= scaleFactor;
            dvh.MeanDose *= scaleFactor;
            dvh.MedianDose *= scaleFactor;
            dvh.StdDevDose *= scaleFactor;

            return dvh;
        }
    }
}