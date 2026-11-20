using NotchLite.Core.Configuration;
using Xunit;

namespace NotchLite.Core.Tests;

public sealed class AppSettingsTests
{
    [Fact]
    public void DefaultsStartAtSchemaVersionOne()
    {
        Assert.Equal(AppSettings.CurrentSchemaVersion, AppSettings.Default.SchemaVersion);
    }
}
