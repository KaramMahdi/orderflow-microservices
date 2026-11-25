using OrderFlow.Architecture.Tests.Infrastructure;

namespace OrderFlow.Architecture.Tests;

/// <summary>
/// The project reference rules from AGENTS.md ("Project reference rules"), checked on the .csproj files.
/// </summary>
public sealed class ProjectReferenceTests
{
    private static readonly SolutionProjects Solution = SolutionProjects.Instance;

    [Theory]
    [InlineData(ProjectNames.HttpApi)]
    [InlineData(ProjectNames.HttpApiClient)]
    public void HttpApiProjectsReferenceOnlyApplicationContracts(string project)
    {
        Assert.Equal([ProjectNames.ApplicationContracts], Solution[project].ProjectReferences.Order());
    }

    [Fact]
    public void DomainReferencesOnlyTheTwoDomainSharedProjects()
    {
        string[] allowed = [ProjectNames.DomainShared, ProjectNames.DomainSharedLayer];

        Assert.Equal(allowed.Order(), Solution[ProjectNames.Domain].ProjectReferences.Order());
    }

    [Fact]
    public void EntityFrameworkCoreReferencesDomain()
    {
        Assert.Contains(ProjectNames.Domain, Solution[ProjectNames.EntityFrameworkCore].ProjectReferences);
    }

    [Fact]
    public void EntityFrameworkCoreNeverReferencesApplication()
    {
        Assert.DoesNotContain(ProjectNames.Application, Solution.ReferenceClosure(ProjectNames.EntityFrameworkCore));
    }

    [Fact]
    public void NoProjectReferencesTheHost()
    {
        var projectsReferencingHost = Solution.All
            .Where(project => project.ProjectReferences.Contains(ProjectNames.HttpApiHost))
            .Select(project => project.Name);

        Assert.Empty(projectsReferencingHost);
    }
}
