using System.Collections.Generic;
using VMS.TPS.Common.Model.API;

namespace ChuckDvhBatch
{
    internal class OnePlanSumPerCourseEnforcer
    {
        // Track courses where a plan sum was analyzed
        private readonly IList<string> _analyzedCourses = new List<string>();

        internal bool CanAnalyze(PlanningItem planningItem)
        {
            if (planningItem is PlanSum planSum)
            {
                if (_analyzedCourses.Contains(planSum.Course.Id))
                    return false;
            }

            return true;
        }

        internal void MarkAnalyzed(PlanningItem planningItem)
        {
            if (planningItem is PlanSum planSum)
                _analyzedCourses.Add(planSum.Course.Id);
        }
    }
}