
// namespace LucidStandardImport;

// public class LucidStandardImportSimpleTests
// {
//     [Fact]
//     public async Task TestCreateSimpleImport()
//     {
//         var lucidDocument = new LucidDocument { Title = "Test Document" };
//         var page = new Page();
//         var layer = new Layer { Title = "Test Layer" };
//         var image = new ImageShape
//         {
//             Image = new ImageFill("test_image.png", ImageScale.Original),
//             BoundingBox = new BoundingBox(0, 0, 100, 100),
//             Opacity = 60,
//             Stroke = new Stroke { Width = 0 },
//         };
//         layer.AddShape(image);
//         page.AddShape(image);
//         page.AddLayer(layer);
//         lucidDocument.AddPage(page);

//         //https://developer.lucid.co/docs/overview-si

//         var oAuthConfig = new LucidOAuthConfig
//         {
//             ClientId = "ozYdHHJV7DBkN1PZgKOh2oDXFYAquTHaUZtLXwRQ",
//             ClientSecret =
//                 "nfBjceUs63oQRBTbym9F3GnvCXXRlHEEPXQTszlttj3NVyykdqavnp4lyK3qNQlPOxZY3Vb6FGPV5ikoHqpQ",
//         };

//         // var localOauth = new LocalOAuthProvider(oAuthConfig, "./authTokens");

//         // var importer = new LucidStandardImporter(localOauth)
//         // {
//         //     DebugOutputFileLocation = "/Users/alexbirch/Downloads/"
//         // };
//         // var urls = await importer.ImportDocument(lucidDocument, "test");

//         // System.Diagnostics.Debug.WriteLine(string.Join(", ", urls));
//     }
// }
