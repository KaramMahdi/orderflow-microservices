using System.Text.Json;
using System.Xml.Linq;

namespace OrderFlow.Architecture.Tests.Infrastructure;

/// <summary>
/// Reads the src/*.csproj files and their restore output (obj/project.assets.json).
/// Project references are checked on the project files because a compiled assembly only records
/// the references it actually uses, and most assemblies are still empty.
/// </summary>
internal sealed class SolutionProjects
{
    private const string SolutionFileName = "OrderFlow.sln";
    private const string SourceFolderName = "src";
    private const string ProjectFilePattern = "*.csproj";
    private const string ObjFolderName = "obj";
    private const string AssetsFileName = "project.assets.json";
    private const string IncludeAttribute = "Include";
    private const string SdkAttribute = "Sdk";
    private const string ProjectReferenceElement = "ProjectReference";
    private const string FrameworkReferenceElement = "FrameworkReference";
    private const string LibrariesProperty = "libraries";
    private const string TypeProperty = "type";
    private const string PackageType = "package";
    private const char LibraryVersionSeparator = '/';
    private const char WindowsPathSeparator = '\\';

    private static readonly Lazy<SolutionProjects> LazyInstance = new(Load);

    private readonly Dictionary<string, ProjectInfo> _projects;

    private SolutionProjects(Dictionary<string, ProjectInfo> projects) => _projects = projects;

    public static SolutionProjects Instance => LazyInstance.Value;

    public IReadOnlyCollection<ProjectInfo> All => _projects.Values;

    public ProjectInfo this[string name] => _projects[name];

    /// <summary>Every project reachable through project references, direct or indirect.</summary>
    public IReadOnlySet<string> ReferenceClosure(string name)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>(_projects[name].ProjectReferences);
        while (pending.TryPop(out var current))
        {
            if (visited.Add(current) && _projects.TryGetValue(current, out var project))
            {
                foreach (var reference in project.ProjectReferences)
                {
                    pending.Push(reference);
                }
            }
        }

        return visited;
    }

    /// <summary>Framework references of the project and of every project it references.</summary>
    public IReadOnlySet<string> FrameworkReferenceClosure(string name)
    {
        var frameworks = new HashSet<string>(_projects[name].FrameworkReferences, StringComparer.OrdinalIgnoreCase);
        foreach (var reference in ReferenceClosure(name))
        {
            frameworks.UnionWith(_projects[reference].FrameworkReferences);
        }

        return frameworks;
    }

    private static SolutionProjects Load()
    {
        var sourceFolder = Path.Combine(FindRepositoryRoot(), SourceFolderName);
        var projects = Directory
            .EnumerateFiles(sourceFolder, ProjectFilePattern, SearchOption.AllDirectories)
            .Select(ReadProject)
            .ToDictionary(project => project.Name, StringComparer.Ordinal);

        return new SolutionProjects(projects);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException($"{SolutionFileName} was not found above {AppContext.BaseDirectory}.");
    }

    private static ProjectInfo ReadProject(string projectFile)
    {
        var root = XDocument.Load(projectFile).Root
            ?? throw new InvalidDataException($"{projectFile} has no root element.");

        var projectReferences = root
            .Descendants(ProjectReferenceElement)
            .Select(element => ProjectNameFromPath(IncludeOf(element)))
            .ToHashSet(StringComparer.Ordinal);

        var frameworkReferences = root
            .Descendants(FrameworkReferenceElement)
            .Select(IncludeOf)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var projectDirectory = Path.GetDirectoryName(projectFile)
            ?? throw new InvalidDataException($"{projectFile} has no directory.");

        return new ProjectInfo(
            Path.GetFileNameWithoutExtension(projectFile),
            root.Attribute(SdkAttribute)?.Value ?? string.Empty,
            projectReferences,
            frameworkReferences,
            ReadResolvedPackages(Path.Combine(projectDirectory, ObjFolderName, AssetsFileName)));
    }

    private static string IncludeOf(XElement element) =>
        element.Attribute(IncludeAttribute)?.Value
            ?? throw new InvalidDataException($"A {element.Name} element has no {IncludeAttribute} attribute.");

    private static string ProjectNameFromPath(string includePath) =>
        Path.GetFileNameWithoutExtension(includePath.Replace(WindowsPathSeparator, Path.DirectorySeparatorChar));

    private static HashSet<string> ReadResolvedPackages(string assetsFile)
    {
        if (!File.Exists(assetsFile))
        {
            throw new FileNotFoundException("Restore the solution before running the architecture tests.", assetsFile);
        }

        using var stream = File.OpenRead(assetsFile);
        using var assets = JsonDocument.Parse(stream);

        return assets.RootElement
            .GetProperty(LibrariesProperty)
            .EnumerateObject()
            .Where(library => library.Value.GetProperty(TypeProperty).GetString() == PackageType)
            .Select(library => library.Name.Split(LibraryVersionSeparator)[0])
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
