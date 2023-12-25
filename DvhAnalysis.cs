using System;
using System.Linq;
using VMS.TPS.Common.Model.API;

namespace ChuckDvhBatch
{
    internal class DvhAnalysis
    {
        private readonly IEsapiApp _esapiApp;
        private readonly AnalysisData _data;

       public DvhAnalysis(IEsapiApp esapiApp, AnalysisData data)
        {
            _esapiApp = esapiApp;
            _data = data;
        }

        public void Analyze()
        {
            foreach (var patientId in _data.GetUniquePatientIds())
            {
                try
                {
                    Console.Error.WriteLine($"-- Start Patient: {patientId}");

                    Analyze(patientId);
                }
                catch (Exception e) when (IsScriptAborted(e))
                {
                    throw;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"ERROR: Patient {patientId}: {e.Message}");
                }
            }
        }

        private void Analyze(string patientId)
        {
            try
            {
                _esapiApp.OpenPatient(patientId);

                var onePlanSumPerCourseEnforcer = new OnePlanSumPerCourseEnforcer();
                foreach (var planningItem in _esapiApp.GetPatientPlanningItems())
                {
                    if (planningItem is PlanSetup p)
                    {
                        if (_data.FindInputDataFor(p) == null)
                        {
                            Console.Error.WriteLine($"---- Course: [{planningItem.GetCourse().Id}], PlanSetup: [{planningItem.Id}] is not listed in input.");

                            continue;
                        }
                    }

                    if (planningItem is PlanSum ps)
                    {
                        //if (!_data._input.Data.Select(t => t.CourseId).Contains(ps.Course.Id))
                        //{
                        //    Console.Error.WriteLine($"---- PlanSum: [{planningItem.Id}]'s Course [{planningItem.GetCourse().Id}] is not listed in input.");

                        //    continue;
                        //}

                        if (_data.FindInputDataFor(ps) == null)
                        {
                            Console.Error.WriteLine($"---- Course: [{planningItem.GetCourse().Id}], PlanSum: [{planningItem.Id}] is not listed in input.");

                            continue;
                        }
                    }

                    string plan_type = planningItem is PlanSetup ? "Plan" : "PlanSum";
                    Console.Error.WriteLine($"\n---- Start Process Course [{planningItem.GetCourse().Id}] {plan_type} [{planningItem.Id}]");

                    try
                    {
                        //if (onePlanSumPerCourseEnforcer.CanAnalyze(planningItem))
                        //{
                            var plan = new AnalysisPlan(planningItem, _data);
                            Analyze(plan);
                            //onePlanSumPerCourseEnforcer.MarkAnalyzed(planningItem); -- not needed anymore.
                        //}
                        //else
                        //{
                        //    Console.Error.WriteLine(
                        //        $"WARNING: Patient [{patientId}], [{planningItem.GetCourse().Id}], [{planningItem.Id}]: Skipped because another plan sum in the same course was already analyzed.");
                        //}
                    }
                    catch (Exception e) when (IsScriptAborted(e))
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine($"ERROR: Patient [{patientId}], [{planningItem.GetCourse().Id}], [{planningItem.Id}]: {e.Message}");
                    }
                }
            }
            finally
            {
                _esapiApp.ClosePatient();
            }
        }

        private void Analyze(AnalysisPlan plan)
        {
            plan.Validate();

            foreach (var structure in plan.ValidStructures)
            {
                try
                {
                    Console.Error.Write($"{structure.Id}, ");
                    Analyze(structure, plan);
                }
                catch (Exception e) when (IsScriptAborted(e))
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"ERROR: Patient [{plan.PatientId}], Course [{plan.PlanningItem.GetCourse().Id}], {plan.Type}, [{plan.Id}], structure {structure.Id}: {ex.Message}");
                }
            }

            Console.Error.WriteLine();
        }

        private void Analyze(Structure structure, AnalysisPlan plan)
        {
            var results = MetricResultSet.CalculateMetrics(plan.PlanningItem, structure, plan.ScalingFactors);
            DvhAnalysisWriter.WriteToConsole(results);
        }

        // ESAPI throws this exception when the user presses Ctrl + C
        private bool IsScriptAborted(Exception e) =>
            e.Message.StartsWith("Script execution was aborted.");
    }
}