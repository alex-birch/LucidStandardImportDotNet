using System.Runtime.CompilerServices;
using LucidStandardImport.Api;
using LucidStandardImport.Auth;
using LucidStandardImport.model;
using Microsoft.Extensions.Configuration;

var lucidDocument = new LucidDocument { Title = "Test Document" };
var page = new Page();
var layer = new Layer { Title = "Test Layer" };
var image = new ImageShape
{
    Image = new ImageFill("./Resources/test_image.png", ImageScale.Original),
    BoundingBox = new BoundingBox(0, 0, 100, 100),
    Opacity = 60,
    Stroke = new Stroke { Width = 0 },
};
layer.AddShape(image);
page.AddShape(image);
page.AddLayer(layer);
lucidDocument.AddPage(page);

// Get config from appsettings.json, env variables user secrets, or other
var oAuthConfig = GetConfiguration();

var importer = new LucidStandardImporter()
{
    // use to inspect the zip file that is to be uploaded to Lucid
    DebugOutputFileLocation = "./",
};
// var oauthProvider = new LocalAuthProvider(oAuthConfig, "./authTokens");
var oauthProvider = new ConsoleAuthProvider(oAuthConfig);
var session = await oauthProvider.CreateLucidSessionAsync();

// almost always returns 1 result but may return more than 1 if the
// file size of the import is larger than the limit, 2MB
// var urls = await importer.ImportDocument(session, lucidDocument, "test");
var urls = await importer.UploadLucidFile("data_be070406-3b35-403d-9cc1-47098cae6e5a.lucid.zip", session, "test");

importer.LaunchUrlInBrowser(urls);

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
