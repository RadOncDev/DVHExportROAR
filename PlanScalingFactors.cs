using System;
using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;

namespace ChuckDvhBatch
{
    internal class PlanScalingFactors
    {
        private const double ScalingFactorTolerance = 0.01;

        private readonly PlanningItem _planningItem;
        private readonly AnalysisData _data;


        // Map each planSetup Id to its scaling factor (i.e. NFraction_Delivered/_Planned)
        private readonly Dictionary<string, double> _scalingFactors;

        public PlanScalingFactors(PlanningItem planningItem, AnalysisData data)
        {
            _planningItem = planningItem;
            _data = data;

            _scalingFactors = Generate();

            var lines = _scalingFactors.Select(kvp => "[" + kvp.Key + "]:" + kvp.Value.ToString());
            Console.Error.WriteLine($"-------- scalingFactors: {string.Join("; ", lines)}");
        }

        public Dictionary<string, double> GetDictionary() => _scalingFactors;

        public double this[string planningItemId] => _scalingFactors[planningItemId];

        public PlanSumComponent[] GetPlanSumMismatches()
        {
            var mismatches = new List<PlanSumComponent>();

            if (_planningItem is PlanSum planSum)
            {
                foreach (var planSumComponent in planSum.PlanSumComponents)
                {
                    var planSetupId = planSumComponent.PlanSetupId;

                    if (_scalingFactors.ContainsKey(planSetupId))
                    {
                        var scale = _scalingFactors[planSetupId];
                        var weight = planSumComponent.PlanWeight;

                        if (Math.Abs(scale - weight) > ScalingFactorTolerance)
                            mismatches.Add(planSumComponent);
                    }
                    else
                        throw new Exception(
                            $"The scaling factor was not found in the input for plan {planSetupId}");
                }
            }

            return mismatches.ToArray();
        }


        // Only PlanSetup have scalingFactors.
        private Dictionary<string, double> Generate()
        {
            var scalingFactors = new Dictionary<string, double>();

            if (_planningItem is PlanSetup planSetup)
            {
                scalingFactors[_planningItem.Id] = GetScalingFactor(planSetup);
            }
            else if (_planningItem is PlanSum planSum)
            {
                foreach (var planSetup2 in planSum.PlanSetups)
                {
                    scalingFactors[planSetup2.Id] = GetScalingFactor(planSetup2);
                }
            }

            return scalingFactors;
        }

        private double GetScalingFactor(PlanSetup plan)
        {
            var fractionsDelivered = _data.GetFractionsDeliveredFor(plan);
            return (double)fractionsDelivered / GetNumberOfFractions(plan);
        }

        private int GetNumberOfFractions(PlanSetup plan)
        {
            if (plan.NumberOfFractions != null)
                return plan.NumberOfFractions.Value;

            throw new InvalidOperationException("No number of fractions");
        }
    }
}
