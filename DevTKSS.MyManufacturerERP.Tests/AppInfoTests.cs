namespace DevTKSS.MyManufacturerERP.Tests;

public class AppInfoTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void AppInfoCreation()
    {
        var appInfo = new AppConfig { Environment = "Test" };

        appInfo.ShouldNotBeNull();
        appInfo.Environment.ShouldBe("Test");
    }
}
