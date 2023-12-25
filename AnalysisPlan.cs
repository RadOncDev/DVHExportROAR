using System;
using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;

namespace ChuckDvhBatch
{
    internal class AnalysisPlan
    {
        private readonly PlanScalingFactors _scalingFactors;

        private readonly AnalysisData _data;

        public AnalysisPlan(PlanningItem planningItem, AnalysisData data)
        {
            PlanningItem = planningItem;

            _data = data;
            _scalingFactors = new PlanScalingFactors(planningItem, data);
        }

        public PlanningItem PlanningItem { get; }

        public string PatientId => PlanningItem.GetPatient().Id;

        public string Id => PlanningItem.Id;

        public string Type => PlanningItem is PlanSetup ? "plan" : "plan sum";

        public Dictionary<string, double> ScalingFactors =>
            _scalingFactors.GetDictionary();

        public IEnumerable<Structure> ValidStructures =>
            PlanningItem.GetStructures().Where(IsValidStructure);

        public void Validate()
        {
            if (PlanningItem.Dose == null)
                throw new Exception("No dose");

            if (!PlanningItem.GetStructures().Any())
                throw new Exception("No structures");

            ThrowIfNotAllPlanSumPlansInSameCourse();

            //ThrowIfNotAllPlanSetupsIncludedInPlanSum();

            ThrowIfPlanSumMismatchWithScalingFactors();
        }

        private void ThrowIfNotAllPlanSetupsIncludedInPlanSum()
        {
            if (PlanningItem is PlanSum ps)
            {
                List<string> planSetupIDs_in_this_Course_from_input = _data._input.Data.Where(t => t.CourseId == ps.Course.Id && t.PlanSetupId != ps.Id).Select(t => t.PlanSetupId).OrderBy(t => t).ToList();

                List<string> planSetupIDs_in_PlanSum = ps.PlanSumComponents.Select(t => t.PlanSetupId).OrderBy(t => t).ToList();

                if (!planSetupIDs_in_this_Course_from_input.SequenceEqual(planSetupIDs_in_PlanSum))
                {
                    throw new Exception($"PlanSum incomplete {string.Join(" | ", planSetupIDs_in_this_Course_from_input)} VS {string.Join(" | ", planSetupIDs_in_PlanSum)}");
                }
            }
        }

        private void ThrowIfPlanSumMismatchWithScalingFactors()
        {
            foreach (var plan in _scalingFactors.GetPlanSumMismatches()) // Only PlanSum will return with count() > 0 here.
                throw new Exception(
                    $"Mismatch between the scaling factor and weight for plan {plan.PlanSetupId} " +
                    $"({_scalingFactors[plan.PlanSetupId]} /= {plan.PlanWeight}).");
        }

        private void ThrowIfNotAllPlanSumPlansInSameCourse()
        {
            if (PlanningItem is PlanSum planSum)
            {
                if (!planSum.PlanSetups.All(plan => plan.Course.Id == planSum.Course.Id))
                    throw new Exception("Not all plans belong to the same course as the plan sum.");
            }
        }

        private static bool IsValidStructure(Structure structure) =>
            !structure.IsEmpty && structure.DicomType.ToUpper() != "SUPPORT" && structure.DicomType.ToUpper() != "MARKER";
    }
}
