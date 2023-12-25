using System.Collections.Generic;
using VMS.TPS.Common.Model.API;

namespace ChuckDvhBatch
{
    public interface IEsapiApp
    {
        void LogIn(string username, string password);
        void LogOut();

        void OpenPatient(string patientId);
        void ClosePatient();

        IEnumerable<PlanningItem> GetPatientPlanningItems();
    }
}