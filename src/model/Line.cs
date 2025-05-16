using System.Text.Json.Serialization;

namespace LucidStandardImport.model;

public class Line : IIdentifiableLucidObject
{
    public Line(string endPoint1ExternalId, string endPoint2ExternalId)
    {
        Endpoint1 = new Endpoint(endPoint1ExternalId);
        Endpoint2 = new Endpoint(endPoint2ExternalId);
    }

    public Line(
        List<Position> path,
        string endPoint1ExternalId = null,
        string endPoint2ExternalId = null
    )
    {
        if (path.Count < 2)
            throw new ArgumentException("Path must contain at least two points.");

        Endpoint1 = !string.IsNullOrEmpty(endPoint1ExternalId)
            ? new Endpoint(endPoint1ExternalId)
            : new Endpoint(path.First().X, path.First().Y);
        Endpoint2 = !string.IsNullOrEmpty(endPoint2ExternalId)
            ? new Endpoint(endPoint2ExternalId)
            : new Endpoint(path.Last().X, path.Last().Y);
            
        path.RemoveAt(0);
        path.RemoveAt(path.Count - 1);
        Joints = path; 
    }

    public string Id { get; set; }

    [JsonIgnore]
    public string ExternalId { get; set; }
    public LineType LineType { get; set; } = LineType.straight;
    public Endpoint Endpoint1 { get; }
    public Endpoint Endpoint2 { get; }
    public Stroke Stroke { get; set; }
    public List<Position> Joints { get; } = [];
}
