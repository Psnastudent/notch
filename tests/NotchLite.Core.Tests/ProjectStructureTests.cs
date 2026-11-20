using Xunit;

namespace NotchLite.Core.Tests;

public sealed class ProjectStructureTests
{
    [Fact]
    public void FoundationSolutionContainsAllRequiredProjects()
    {
        var repositoryRoot = FindRepositoryRoot();
        var requiredProjectPaths = new[]
        {
            "NotchLite.sln",
            Path.Combine("src", "NotchLite.App", "NotchLite.App.csproj"),
            Path.Combine("src", "NotchLite.Core", "NotchLite.Core.csproj"),
            Path.Combine("src", "NotchLite.Application", "NotchLite.Application.csproj"),
            Path.Combine("src", "NotchLite.Infrastructure", "NotchLite.Infrastructure.csproj"),
            Path.Combine("src", "NotchLite.Platform", "NotchLite.Platform.csproj"),
            Path.Combine("tests", "NotchLite.Application.Tests", "NotchLite.Application.Tests.csproj")
        };

        var missingPaths = requiredProjectPaths
            .Where(path => !File.Exists(Path.Combine(repositoryRoot, path)))
            .ToArray();

        Assert.True(missingPaths.Length == 0, $"Missing project files: {string.Join(", ", missingPaths)}");
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var gitPath = Path.Combine(directory.FullName, ".git");
            if (Directory.Exists(gitPath) || File.Exists(gitPath))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Unable to locate the repository root.");
    }
}
