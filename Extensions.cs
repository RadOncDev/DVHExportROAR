using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;

namespace ChuckDvhBatch
{
    public static class Extensions
    {
        public static IEnumerable<Structure> GetStructures(this PlanningItem planningItem)
        {
            if (planningItem is PlanSetup)
            {
                return ((PlanSetup)planningItem)
                    .StructureSet?.Structures ?? Enumerable.Empty<Structure>();
            }
            else
            {
                return ((PlanSum)planningItem)
                    .StructureSet?.Structures ?? Enumerable.Empty<Structure>();
            }
        }

        public static Course GetCourse(this PlanningItem planningItem)
        {
            if (planningItem is PlanSetup)
            {
                return ((PlanSetup)planningItem).Course;
            }
            else
            {
                return ((PlanSum)planningItem).Course;
            }
        }

        public static Patient GetPatient(this PlanningItem planningItem)
        {
            return planningItem.GetCourse().Patient;
        }
    }
}
