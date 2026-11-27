namespace OrderFlow.Architecture.Tests.Infrastructure;

/// <summary>
/// What one src project declares in its .csproj, plus every NuGet package its restore resolved.
/// </summary>
internal sealed record ProjectInfo(
    string Name,
    string Sdk,
    IReadOnlySet<string> ProjectReferences,
    IReadOnlySet<string> FrameworkReferences,
    IReadOnlySet<string> ResolvedPackages);
