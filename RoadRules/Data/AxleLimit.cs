namespace RoadRules.Data;

public class AxleLimit
{
    public int Id { get; set; }
    public int JurisdictionId { get; set; }
    public Jurisdiction? Jurisdiction { get; set; }
    public string AxleGroup { get; set; } = "";   // Single, Tandem, Tridem
    public int AxleCount { get; set; }            // 1, 2, 3
    public int MaxKg { get; set; }
    public DateOnly EffectiveFrom { get; set; }
}
