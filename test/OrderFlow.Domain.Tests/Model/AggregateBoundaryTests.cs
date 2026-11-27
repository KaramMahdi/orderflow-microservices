using System.Reflection;
using Domain.Shared.Layer.Constants;
using Domain.Shared.Layer.Context;
using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.Repositories;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Catalog;
using OrderFlow.Domain.Customers;
using OrderFlow.Domain.Files;
using OrderFlow.Domain.Notifications;
using OrderFlow.Domain.Orders;
using OrderFlow.Domain.Payments;
using OrderFlow.Domain.Shared.Notifications;
using OrderFlow.Domain.Shared.Orders;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.Model;

/// <summary>
/// The aggregate rules of AGENTS.md, checked by reflection: no public setters, aggregates reference
/// other aggregates by id only, one repository per root, Guid version 7 ids, no infrastructure references.
/// </summary>
public sealed class AggregateBoundaryTests
{
    private const string SystemPrefix = "System";
    private const int GuidVersion7 = 7;

    private static readonly Assembly DomainAssembly = typeof(Order).Assembly;
    private static readonly Assembly SharedLayerAssembly = typeof(AggregateRoot).Assembly;
    private static readonly Assembly ContextSharedAssembly = typeof(OrderStatus).Assembly;

    private static readonly string[] AllowedDomainReferences = [SharedLayerAssembly.GetName().Name!, ContextSharedAssembly.GetName().Name!];

    private static readonly Type[] ModelTypes = DomainAssembly.GetTypes()
        .Concat(SharedLayerAssembly.GetTypes())
        .Where(type => type is { IsClass: true } && (typeof(Entity).IsAssignableFrom(type) || typeof(ValueObject).IsAssignableFrom(type)))
        .ToArray();

    private static readonly Type[] AggregateRoots = DomainAssembly.GetTypes()
        .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(AggregateRoot).IsAssignableFrom(type))
        .ToArray();

    public static TheoryData<Type> Roots => new(AggregateRoots);

    public static TheoryData<string, Func<AggregateRoot>> Factories => new()
    {
        { nameof(Order), () => Order.Place("ORD-1", Guid.CreateVersion7(), "USD", Address.Create("1 Main St", "Amman", null, "JO"), RequestSource.Web, [new OrderItemLine(Guid.CreateVersion7(), 1, Money.Create(1m, "USD"))]) },
        { nameof(Product), () => Product.Create(Sku.Create("P-1"), "Tea") },
        { nameof(StockItem), () => StockItem.Create(Guid.CreateVersion7(), 1) },
        { nameof(Customer), () => Customer.Register(Guid.CreateVersion7(), Email.Create("a@example.com"), Address.Create("1 Main St", "Amman", null, "JO")) },
        { nameof(Payment), () => Payment.Start(Guid.CreateVersion7(), Money.Create(1m, "USD")) },
        { nameof(Notification), () => Notification.Create(Guid.CreateVersion7(), NotificationChannel.Email, "a@example.com", null, "Hi", TimeProvider.System) },
        { nameof(FileRecord), () => FileRecord.CreatePending("a.pdf", "application/pdf", ".pdf", Modules.Orders, nameof(Order), Guid.CreateVersion7(), TimeProvider.System) },
    };

    [Fact]
    public void EveryContextHasItsAggregateRoots()
    {
        Assert.Equal(
            [nameof(Customer), nameof(FileRecord), nameof(Notification), nameof(Order), nameof(Payment), nameof(Product), nameof(StockItem)],
            AggregateRoots.Select(type => type.Name).Order());
    }

    [Fact]
    public void NoEntityOrValueObjectHasAPublicSetter()
    {
        var publicSetters = ModelTypes
            .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(property => property.SetMethod is { IsPublic: true })
            .Select(property => $"{property.DeclaringType!.Name}.{property.Name}");

        Assert.Empty(publicSetters);
    }

    [Theory]
    [MemberData(nameof(Roots))]
    public void AnAggregateNeverHoldsAnotherAggregate(Type root)
    {
        var heldTypes = root.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(property => property.PropertyType)
            .Concat(root.GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Select(field => field.FieldType))
            .SelectMany(Flatten);

        Assert.DoesNotContain(heldTypes, type => typeof(AggregateRoot).IsAssignableFrom(type));
    }

    [Theory]
    [MemberData(nameof(Roots))]
    public void EveryRootHasExactlyOneRepositoryInterface(Type root)
    {
        var repositoryType = typeof(IRepository<>).MakeGenericType(root);

        var repositories = DomainAssembly.GetTypes()
            .Where(type => type.IsInterface && repositoryType.IsAssignableFrom(type));

        Assert.Single(repositories);
    }

    [Fact]
    public void OnlyAggregateRootsHaveRepositories()
    {
        var repositoryTargets = DomainAssembly.GetTypes()
            .Where(type => type.IsInterface)
            .SelectMany(type => type.GetInterfaces())
            .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IRepository<>))
            .Select(type => type.GetGenericArguments()[0]);

        Assert.All(repositoryTargets, target => Assert.Contains(target, AggregateRoots));
    }

    [Theory]
    [MemberData(nameof(Factories))]
    public void TheDomainCreatesVersion7Ids(string name, Func<AggregateRoot> create)
    {
        var aggregate = create();

        Assert.True(aggregate.Id.Version == GuidVersion7, $"{name} id is not version 7.");
    }

    [Theory]
    [MemberData(nameof(Factories))]
    public void ChildEntitiesAlsoGetVersion7Ids(string name, Func<AggregateRoot> create)
    {
        var aggregate = create();
        var children = aggregate.GetType().GetProperties()
            .Where(property => typeof(IEnumerable<Entity>).IsAssignableFrom(property.PropertyType))
            .SelectMany(property => (IEnumerable<Entity>)property.GetValue(aggregate)!);

        Assert.All(children, child => Assert.True(child.Id.Version == GuidVersion7, $"A child of {name} has no version 7 id."));
    }

    [Fact]
    public void TheDomainReferencesNoInfrastructure()
    {
        var references = DomainAssembly.GetReferencedAssemblies()
            .Concat(ContextSharedAssembly.GetReferencedAssemblies())
            .Concat(SharedLayerAssembly.GetReferencedAssemblies())
            .Select(reference => reference.Name!)
            .Where(name => !name.StartsWith(SystemPrefix, StringComparison.Ordinal) && !AllowedDomainReferences.Contains(name));

        Assert.Empty(references);
    }

    [Fact]
    public void TheDomainHasNoDto()
    {
        Assert.DoesNotContain(DomainAssembly.GetTypes(), type => type.Name.EndsWith("Dto", StringComparison.Ordinal));
    }

    private static IEnumerable<Type> Flatten(Type type) =>
        type.IsGenericType
            ? type.GetGenericArguments().SelectMany(Flatten).Append(type)
            : type.IsArray ? Flatten(type.GetElementType()!) : [type];
}
