namespace DevTKSS.MyManufacturerERP.xUnitTests;

public class AppInfoTests
{
    [Fact]
    public void AppInfoCreation()
    {
        var appInfo = new AppConfig { Environment = "Test" };
        appInfo.ShouldNotBeNull();
        appInfo.Environment.ShouldBe("Test");
        Assert.NotNull(appInfo);
        Assert.Equal("Test", appInfo.Environment);
    }
}
