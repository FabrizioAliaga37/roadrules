namespace RoadRules.Shared;

public record LimitRecord(string AxleGroup, int AxleCount, int MaxKg, DateOnly EffectiveFrom);

public record WeightResult(string JurisdictionCode, string AxleGroup, int MaxKg, DateOnly EffectiveFrom);
