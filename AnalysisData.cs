using System;
using System.Linq;
using VMS.TPS.Common.Model.API;

namespace ChuckDvhBatch
{
    public class AnalysisData
    {
        public readonly Input _input;

        public AnalysisData(Input input)
        {
            _input = input;
        }

        public string[] GetUniquePatientIds() =>
            _input.Data.Select(x => x.PatientId).Distinct().ToArray();

        public int GetFractionsDeliveredFor(PlanSetup plan)
        {
            var inputData = FindInputDataFor(plan);

            if (inputData == null)
                throw new ArgumentException($"\t -- [{plan.Course.Id}] [{plan.Id}] not listed in input, cannot get FractionDelivered.");

            return inputData.FractionsDelivered;
        }

        public InputData FindInputDataFor(PlanSetup plan) =>
            _input.Data.FirstOrDefault(x => x.PatientId == plan.GetPatient().Id
                                            && x.CourseId == plan.GetCourse().Id
                                            && x.PlanSetupId == plan.Id);

        public InputData FindInputDataFor(PlanSum ps) =>
            _input.Data.FirstOrDefault(x => x.PatientId == ps.GetPatient().Id
                                            && x.CourseId == ps.GetCourse().Id
                                            && x.PlanSetupId == ps.Id);
    }
}
