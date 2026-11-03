namespace RoadRules.Data;

public class Jurisdiction
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public List<AxleLimit> Limits { get; set; } = new();
}
