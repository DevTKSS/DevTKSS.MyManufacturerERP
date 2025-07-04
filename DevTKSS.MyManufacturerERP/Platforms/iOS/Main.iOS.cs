using DevTKSS.MyManufacturerERP;
using UIKit;
using Uno.UI.Hosting;

var host = UnoPlatformHostBuilder.Create()
    .App(() => new App())
    .UseAppleUIKit()
    .Build();

host.Run();
