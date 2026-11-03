namespace RoadRules.Services;

public record LimitRecord(string AxleGroup, int AxleCount, int MaxKg);

public record WeightResult(string JurisdictionCode, string AxleGroup, int MaxKg);
