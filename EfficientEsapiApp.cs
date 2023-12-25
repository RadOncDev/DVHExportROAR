using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using VMS.TPS.Common.Model.API;

namespace ChuckDvhBatch
{
    // Create and dispose the application every few patients
    // to clear out unmanaged memory, which for some reason
    // continues to accumulate even after a patient is closed

    // Also, a single plan sum can take up a lot of memory,
    // so close and re-open the patient when getting a plan sum
    // to clear out any memory use by previous plans

    public class EfficientEsapiApp : IEsapiApp
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const int CountLimit = 20;

        private readonly IEsapiApp _app;

        private string _username;
        private string _password;

        private int _count;

        private string _patientId;

        public EfficientEsapiApp(IEsapiApp app)
        {
            _app = app;
        }

        public void LogIn(string username, string password)
        {
            _app.LogIn(username, password);

            _username = username;
            _password = password;

            _count = 0;
        }

        public void LogOut() => _app.LogOut();

        public void OpenPatient(string patientId)
        {
            if (_count >= CountLimit)
            {
                Logger.Debug($"Reached count limit of {CountLimit}");

                _app.LogOut();
                _app.LogIn(_username, _password);

                Logger.Debug("Recreated ESAPI application");

                _count = 0;
            }

            _count++;

            _app.OpenPatient(patientId);
            _patientId = patientId;
        }

        public void ClosePatient()
        {
            _app.ClosePatient();
            _patientId = null;
        }

        public IEnumerable<PlanningItem> GetPatientPlanningItems()
        {
            var planningItemSummaries = CreatePlanningItemSummaries();

            foreach (var planningItemSummary in planningItemSummaries)
            {
                if (planningItemSummary.Type == EsapiPlanningItemType.PlanSum)
                {
                    Logger.Debug($"About to re-open patient because planning item [{planningItemSummary.Id}] is a plan sum");
                    //Console.Error.WriteLine($"\n-- * About to re-open patient because planning item [{planningItemSummary.Id}] is a plan sum");

                    ReopenPatient();  // TTT should check if the PlanSum is in the list first maybe? at least later when it is certain that DVH will be calculated for this PlanSum?
                }

                yield return GetPlanningItem(planningItemSummary);
            }
        }

        private EsapiPlanningItemSummary[] CreatePlanningItemSummaries()
        {
            var planningItems = _app.GetPatientPlanningItems();

            Console.Error.WriteLine($"{planningItems.Count()} planningItems are retrieved from patient.");
            return planningItems.Select(CreatePlanningItemSummary).ToArray();
        }

        private EsapiPlanningItemSummary CreatePlanningItemSummary(PlanningItem planningItem)
        {
            if (planningItem is PlanSetup planSetup)
                return CreatePlanningItemSummary(planSetup);

            if (planningItem is PlanSum planSum)
                return CreatePlanningItemSummary(planSum);

            throw new InvalidOperationException("Unknown planning item type");
        }

        private EsapiPlanningItemSummary CreatePlanningItemSummary(PlanSetup planSetup) =>
            new EsapiPlanningItemSummary
            {
                Type = EsapiPlanningItemType.PlanSetup,
                CourseId = planSetup.Course.Id,
                Id = planSetup.Id
            };

        private EsapiPlanningItemSummary CreatePlanningItemSummary(PlanSum planSum) =>
            new EsapiPlanningItemSummary
            {
                Type = EsapiPlanningItemType.PlanSum,
                CourseId = planSum.Course.Id,
                Id = planSum.Id
            };

        private void ReopenPatient()
        {
            _app.ClosePatient();
            _app.OpenPatient(_patientId);
        }

        private PlanningItem GetPlanningItem(EsapiPlanningItemSummary summary)
        {
            var planningItems = _app.GetPatientPlanningItems();  // TTT why here again???
            return planningItems.First(x => MatchesSummary(x, summary));
        }

        private bool MatchesSummary(PlanningItem planningItem, EsapiPlanningItemSummary summary)
        {
            return GetType(planningItem) == summary.Type &&
                   planningItem.GetCourse().Id == summary.CourseId &&
                   planningItem.Id == summary.Id;
        }

        private EsapiPlanningItemType GetType(PlanningItem planningItem)
        {
            if (planningItem is PlanSetup)
                return EsapiPlanningItemType.PlanSetup;

            if (planningItem is PlanSum)
                return EsapiPlanningItemType.PlanSum;

            throw new InvalidOperationException("Unknown planning item type");
        }
    }
}
