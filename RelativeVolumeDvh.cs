using System.Collections.Generic;
using System.Linq;
using DVHAnalysis;

namespace ChuckDvhBatch
{
    public class RelativeVolumeDvh
    {
        // Percent coverage (and above) that will be treated as 100%
        private const double CoverageTolerance = 98.0;

        private static readonly IReadOnlyList<double> DefaultRelativeVolumes = new[]
        {
            100, 99.5, 99, 98, 97, 96, 95, 94, 93, 92, 91, 90, 85, 80, 75, 70,
            65, 60, 55, 50, 45, 40, 35, 30, 25, 20, 15, 10, 5, 4, 3, 2, 1, 0.5, 0
        };

        private readonly DVH _absoluteDvh;

        public RelativeVolumeDvh(DVH absoluteDvh)
        {
            _absoluteDvh = absoluteDvh;
        }

        private double PercentCoverage => _absoluteDvh.Coverage * 100.0;
        private bool IsFullCoverage => PercentCoverage > CoverageTolerance;

        public double[,] CalculateDvh()
        {
            var relativeVolumes = GetRelativeVolumes();
            var dvh = CalculateRelativeVolumeDvh(relativeVolumes);
            dvh = AddFirstPoint(dvh);
            return ConvertTo2DArray(dvh);
        }

        // For incomplete coverage, replace 100%, 99.5%, etc.
        // with percent coverage until coverage is greater
        private double[] GetRelativeVolumes() =>
            IsFullCoverage
                ? DefaultRelativeVolumes.ToArray()
                : DefaultRelativeVolumes.Select(v => Min(v, PercentCoverage)).ToArray();

        private DVPoint[] CalculateRelativeVolumeDvh(IEnumerable<double> relativeVolumes) =>
            relativeVolumes.Select(v => new DVPoint(CalculateDoseToRelativeVolume(v), v)).ToArray();

        private double CalculateDoseToRelativeVolume(double relativeVolume) =>
            CalculateDoseToVolume(ConvertToAbsolute(relativeVolume));

        // Use the first point of the DVH instead of the total volume
        // because they sometimes differ enough to cause calculation problems
        private double ConvertToAbsolute(double relativeVolume) =>
            (relativeVolume / 100.0) * _absoluteDvh.CurveData[0].Volume;

        private double CalculateDoseToVolume(double volume) =>
            new DoseToVolumeMetric {Volume = volume}.Calculate(_absoluteDvh).Value;

        private DVPoint[] AddFirstPoint(IEnumerable<DVPoint> dvh) =>
            Prepend(GetFirstPoint(), dvh).ToArray();

        private IEnumerable<T> Prepend<T>(T item, IEnumerable<T> items) =>
            new[] {item}.Concat(items);

        private DVPoint GetFirstPoint() =>
            IsFullCoverage
                ? new DVPoint(0.0, 100.0)
                : new DVPoint(0.0, PercentCoverage);

        private double[,] ConvertTo2DArray(IEnumerable<DVPoint> dvh)
        {
            var dvArray = dvh.ToArray();
            var array2D = new double[dvArray.Length, 2];

            for (var i = 0; i < dvArray.Length; i++)
            {
                array2D[i, 0] = dvArray[i].Dose;
                array2D[i, 1] = dvArray[i].Volume;
            }

            return array2D;
        }

        private double Min(double x, double y) => x < y ? x : y;
    }
}
