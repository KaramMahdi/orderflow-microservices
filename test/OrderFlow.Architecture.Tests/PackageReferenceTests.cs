using OrderFlow.Architecture.Tests.Infrastructure;

namespace OrderFlow.Architecture.Tests;

/// <summary>
/// Inner layers stay free of infrastructure packages. Checks every package the restore resolved,
/// including transitive ones, and the ASP.NET Core shared framework.
/// </summary>
public sealed class PackageReferenceTests
{
    private static readonly SolutionProjects Solution = SolutionProjects.Instance;

    [Theory]
    [InlineData(ProjectNames.Domain)]
    [InlineData(ProjectNames.Application)]
    [InlineData(ProjectNames.ApplicationContracts)]
    [InlineData(ProjectNames.HttpApiClient)]
    public void NeverReferencesRedisOrRabbitMqPackages(string project)
    {
        Assert.Empty(MatchingPackages(project, ForbiddenDependencies.RedisAndRabbitMqPackages));
    }

    [Theory]
    [InlineData(ProjectNames.DomainSharedLayer)]
    [InlineData(ProjectNames.Domain)]
    [InlineData(ProjectNames.Application)]
    [InlineData(ProjectNames.ApplicationContracts)]
    public void NeverReferencesEntityFrameworkCoreRedisRabbitMqOrAspNetCorePackages(string project)
    {
        Assert.Empty(MatchingPackages(project, ForbiddenDependencies.InfrastructurePackages));
    }

    [Theory]
    [InlineData(ProjectNames.DomainSharedLayer)]
    [InlineData(ProjectNames.Domain)]
    [InlineData(ProjectNames.Application)]
    [InlineData(ProjectNames.ApplicationContracts)]
    public void NeverReferencesTheAspNetCoreFramework(string project)
    {
        Assert.DoesNotContain(ForbiddenDependencies.AspNetCoreFramework, Solution.FrameworkReferenceClosure(project));
        Assert.Equal(SdkNames.Default, Solution[project].Sdk);
    }

    private static string[] MatchingPackages(string project, IEnumerable<string> forbiddenPrefixes) =>
        Solution[project].ResolvedPackages
            .Where(package => forbiddenPrefixes.Any(prefix => IsSameOrBelow(package, prefix)))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static bool IsSameOrBelow(string packageId, string prefix) =>
        packageId.Equals(prefix, StringComparison.OrdinalIgnoreCase)
        || packageId.StartsWith(prefix + ".", StringComparison.OrdinalIgnoreCase);
}
