using LucidStandardImport.model;
using Newtonsoft.Json.Linq;
using Xunit;

namespace LucidStandardImport.Tests;

public class EndpointSerializationTests
{
    [Fact]
    public void PositionEndpointStyleSerializesAsArrowString()
    {
        var document = new LucidDocument("Endpoint style test");
        var page = document.AddPage();
        var line = new Line([new Position(0, 0), new Position(100, 0)])
        {
            Endpoint1 = new Endpoint(0, 0),
            Endpoint2 = new Endpoint(100, 0, EndpointStyle.arrow),
        };

        page.AddLine(line);

        var json = document.SerializeToJsonString();
        var parsed = JObject.Parse(json);

        Assert.Equal("arrow", parsed["pages"]?[0]?["lines"]?[0]?["endpoint2"]?["style"]?.Value<string>());
    }
}
