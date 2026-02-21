using System.Collections.Generic;
using RoboCleanCloud.Domain.Enums;
using RoboCleanCloud.Domain.Primitives;

namespace RoboCleanCloud.Domain.ValueObjects;

public class CleaningParameters : ValueObject
{
    public CleaningMode Mode { get; init; }
    public SuctionPower SuctionPower { get; init; }
    public WaterFlow WaterFlow { get; init; }
    public int? NumberOfPasses { get; init; }
    public bool CarpetBoost { get; init; }
    public bool EdgeCleaning { get; init; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Mode;
        yield return SuctionPower;
        yield return WaterFlow;
        yield return NumberOfPasses ?? 0;
        yield return CarpetBoost;
        yield return EdgeCleaning;
    }
}