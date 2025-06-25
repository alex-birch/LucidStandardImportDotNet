using LucidStandardImport.model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

public static class SerializationHelper
{
    public static string SerializeToJsonString(this LucidDocument lucidDocument)
    {
        var opts = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = { new StringEnumConverter(new CamelCaseNamingStrategy()) },
            Formatting = Formatting.Indented,
        };
        return JsonConvert.SerializeObject(lucidDocument, opts).Replace("\r\n", "\n"); // CRLF -> LF line endings.
    }
}
