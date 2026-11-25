using System.Reflection;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnitV3;
using Domain.Shared.Layer.Context;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchitectureModel = ArchUnitNET.Domain.Architecture;

namespace OrderFlow.Architecture.Tests;

/// <summary>
/// Type-level rules checked with ArchUnitNET on the compiled assemblies.
/// The assemblies are still almost empty, so the rules accept an empty set of types
/// (WithoutRequiringPositiveResults). They start to bite as soon as types are added.
/// </summary>
public sealed class LayerDependencyTests
{
    private static readonly string[] InnerLayerProjects =
    [
        ProjectNames.DomainSharedLayer,
        ProjectNames.Domain,
        ProjectNames.Application,
        ProjectNames.ApplicationContracts,
    ];

    private static readonly string[] LayersAboveDomain =
    [
        ProjectNames.Application,
        ProjectNames.ApplicationContracts,
        ProjectNames.EntityFrameworkCore,
        ProjectNames.HttpApi,
        ProjectNames.HttpApiClient,
        ProjectNames.HttpApiHost,
    ];

    private static readonly ArchitectureModel Architecture = new ArchLoader()
        .LoadAssemblies(LoadAll(InnerLayerProjects.Union(LayersAboveDomain)))
        .Build();

    [Fact]
    public void DomainDoesNotDependOnLayersAboveIt()
    {
        var layersAboveDomain = LoadAll(LayersAboveDomain);

        IArchRule rule = Types().That().ResideInAssembly(Load(ProjectNames.Domain))
            .Should().NotDependOnAny(Types().That().ResideInAssembly(layersAboveDomain[0], layersAboveDomain[1..]))
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }

    /// <summary>
    /// ADR-0013: Application Services read the request context and pass plain values to the Domain.
    /// </summary>
    [Fact]
    public void DomainDoesNotUseTheRequestContext()
    {
        IArchRule rule = Types().That().ResideInAssembly(Load(ProjectNames.Domain))
            .Should().NotDependOnAny(typeof(IRequestContext))
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }

    [Fact]
    public void EntityFrameworkCoreDoesNotDependOnApplication()
    {
        IArchRule rule = Types().That().ResideInAssembly(Load(ProjectNames.EntityFrameworkCore))
            .Should().NotDependOnAny(Types().That().ResideInAssembly(Load(ProjectNames.Application)))
            .WithoutRequiringPositiveResults();

        rule.Check(Architecture);
    }

    [Theory]
    [InlineData(ProjectNames.DomainSharedLayer)]
    [InlineData(ProjectNames.Domain)]
    [InlineData(ProjectNames.Application)]
    [InlineData(ProjectNames.ApplicationContracts)]
    public void InnerLayersDoNotUseInfrastructureNamespaces(string project)
    {
        foreach (var forbiddenNamespace in ForbiddenDependencies.InfrastructureNamespaces)
        {
            IArchRule rule = Types().That().ResideInAssembly(Load(project))
                .Should().NotDependOnAnyTypesThat().ResideInNamespaceOrBelow(forbiddenNamespace)
                .WithoutRequiringPositiveResults();

            rule.Check(Architecture);
        }
    }

    private static Assembly Load(string project) => Assembly.Load(new AssemblyName(project));

    private static Assembly[] LoadAll(IEnumerable<string> projects) => projects.Select(Load).ToArray();
}
