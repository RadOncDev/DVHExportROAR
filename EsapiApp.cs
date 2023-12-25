using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using VMS.TPS.Common.Model.API;

namespace ChuckDvhBatch
{
    public class EsapiApp : IEsapiApp
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private Application _app;
        private Patient _patient;

        public void LogIn(string username, string password)
        {
            _app = Application.CreateApplication();

            if (_app == null)
                throw new Exception("Cannot create ESAPI application");

            Logger.Debug("Created ESAPI application");
        }

        public void LogOut()
        {
            _app.Dispose();
            Logger.Debug("Disposed of ESAPI application");
        }

        public void OpenPatient(string patientId)
        {
            _patient = _app.OpenPatientById(patientId);

            if (_patient == null)
                throw new Exception($"Cannot open patient {patientId}");

            Logger.Debug($"Opened patient {patientId}");
        }

        public void ClosePatient()
        {
            _app.ClosePatient();
            Logger.Debug("Closed patient");
        }

        public IEnumerable<PlanningItem> GetPatientPlanningItems() =>
            GetPlanSetups().Concat(GetPlanSums());

        private IEnumerable<PlanningItem> GetPlanSetups() =>
            _patient.Courses?.SelectMany(x => x.PlanSetups) ?? new PlanSetup[0];

        private IEnumerable<PlanningItem> GetPlanSums() =>
            _patient.Courses?.SelectMany(x => x.PlanSums) ?? new PlanSum[0];
    }
}