Simple C# Library implementation of the Lucid Standard Import.

Refer to https://developer.lucid.co/docs/overview-si

Example code:

```c#
using LucidStandardImport.Api;
using LucidStandardImport.Auth;
using LucidStandardImport.model;
using Microsoft.Extensions.Configuration;

var lucidDocument = new LucidDocument { Title = "Test Document" };
var page = new Page();
var layer = new Layer { Title = "Test Layer" };
var image = new ImageShape
{
    Image = new ImageFill(
        "test_image.png",
        ImageScale.Original
    ),
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
    // use this to inspect the zip file that is to be uploaded to Lucid
    DebugOutputFileLocation = "<some-local-path>",
};
var oauthProvider = new LocalAuthProvider(oAuthConfig, "./authTokens");
var session = await oauthProvider.CreateLucidSessionAsync();
// almost always returns 1 result but may return more than 1 if the 
// file size of the import is larger than the 2MB limit
var urls = await importer.ImportDocument(session, lucidDocument, "test");

importer.LaunchUrlsInBrowser(urls);
```

Config can be defined inline, in user secrets, or env variables.

```
dotnet user-secrets init
dotnet user-secrets set "LucidOAuthConfig:ClientId" "your-client-id"
dotnet user-secrets set "LucidOAuthConfig:ClientSecret" "your-client-secret"
dotnet user-secrets set "LucidOAuthConfig:RedirectUri" "https://your-app/callback"
```

Example configuration:
```c#
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
```


# Using OAuth

To authenticate with the library you need a Lucid Session.

You'll need a developer project and, if you haven't published your project, to register your username with the developer application.

Refer to the Lucid Docs for that under OAuth. Once set up you can follow the patterns below:

Easiest way to get started is to use the `LocalAuthProvider` class - this will launch a server at the localhost:port specified in your config and will handle the auth flow to create a code for an OAuth session.

You can also use the `ConsolePromptingOauthProvider` like so:
```c#

var oauthProvider = new LucidOAuthProvider(
    oAuthConfig,
    new LocalFileTokenStorage("./authTokens"),
    new ConsolePromptingOAuthFlow()
);

var session = await oauthProvider.CreateLucidSessionAsync();
```

# 