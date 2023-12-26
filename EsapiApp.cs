using System;
using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;

namespace ChuckDvhBatch
{
    public class EsapiApp : IEsapiApp
    {
        
        private Application _app;
        private Patient _patient;

        public void LogIn(string username, string password)
        {
            _app = Application.CreateApplication();

            if (_app == null)
                throw new Exception("Cannot create ESAPI application");

            Console.Error.WriteLine("Created ESAPI application");
        }

        public void LogOut()
        {
            _app.Dispose();
            Console.Error.WriteLine("Disposed of ESAPI application");
        }

        public void OpenPatient(string patientId)
        {
            _patient = _app.OpenPatientById(patientId);

            if (_patient == null)
                throw new Exception($"Cannot open patient {patientId}");

            Console.Error.WriteLine($"Opened patient {patientId}");
        }

        public void ClosePatient()
        {
            _app.ClosePatient();
            Console.Error.WriteLine("Closed patient");
        }

        public IEnumerable<PlanningItem> GetPatientPlanningItems() =>
            GetPlanSetups().Concat(GetPlanSums());

        private IEnumerable<PlanningItem> GetPlanSetups() =>
            _patient.Courses?.SelectMany(x => x.PlanSetups) ?? new PlanSetup[0];

        private IEnumerable<PlanningItem> GetPlanSums() =>
            _patient.Courses?.SelectMany(x => x.PlanSums) ?? new PlanSum[0];
    }
}