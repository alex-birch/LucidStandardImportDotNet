using System.Runtime.CompilerServices;
using LucidStandardImport;
using LucidStandardImport.Api;
using LucidStandardImport.Auth;
using LucidStandardImport.model;
using Microsoft.Extensions.Configuration;

var lucidDocument = new LucidDocument("Test Document");
var page = lucidDocument.AddPage();
var layer = page.AddLayer("Test Layer");
var image = new ImageShape
{
    ImageFill = new ImageFill("./Resources/test_image.png", ImageScale.Original),
    BoundingBox = new BoundingBox(0, 0, 100, 100),
    Opacity = 60,
    Stroke = new Stroke { Width = 0 },
};
layer.AddShape(image);
page.AddShape(image);
var circle = new CircleShape
{
    BoundingBox = new BoundingBox(50, 50, 50, 50),
    Opacity = 50,
    Style = new Style
    {
        Fill = new Fill { Type = "color", Color = "#bedbed" }
    }
};

page.AddShape(
    new TableShape(
        new BoundingBox(100, 0, 100, 100),
        new TableCell[2, 2] { { new("Test 0,0"), new("Test 1,0") }, { null, new("Test 1,1") } }
    )
);

layer.AddShape(circle);
page.AddLayer(layer);

// page.AddShape(circle);
// lucidDocument.AddPage(page);

// Get config from appsettings.json, env variables user secrets, or other
var oAuthConfig = GetConfiguration();

var importer = new LucidStandardImporter()
{
    // use to inspect the zip file that is to be uploaded to Lucid
    DebugOutputFileLocation = "./",
    // DebugInputZipFileLocation = "./data_be070406-3b35-403d-9cc1-47098cae6e5a.lucid.zip"
};

// var oauthProvider = new LocalAuthProvider(oAuthConfig, "./authTokens");
var oauthProvider = new ConsoleAuthProvider(oAuthConfig);
var session = await oauthProvider.CreateLucidSessionAsync();

// almost always returns 1 result but may return more than 1 if the
// file size of the import is larger than the limit, 2MB
var urls = await importer.ImportDocumentAsync(session, lucidDocument, "test");

// var urls = await importer.UploadLucidFile("data_be070406-3b35-403d-9cc1-47098cae6e5a.lucid.zip", session, "test");

importer.LaunchUrlInBrowser(urls.FirstOrDefault());

/// <summary>
/// Gets the configuration from appsettings, env variables, and user secrets
/// </summary>
static LucidOAuthConfig GetConfiguration()
{
    var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory()) // Needed for JSON config
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) // Load from appsettings.json
        .AddEnvironmentVariables() // Load from environment variables
        .AddUserSecrets<Program>(optional: true) // Load from User Secrets in development
        .Build();

    var oAuthConfig = config.GetSection("LucidOAuthConfig").Get<LucidOAuthConfig>();
    if (oAuthConfig == null)
    {
        throw new ArgumentException(
            "OAuth configuration is missing or invalid. Define in appsettings.json, env variables, or user secrets."
        );
    }
    return oAuthConfig;
}
