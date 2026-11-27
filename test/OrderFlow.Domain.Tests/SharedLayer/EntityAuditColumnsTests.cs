using System.Reflection;
using Domain.Shared.Layer.Constants;
using Domain.Shared.Layer.DomainModel;

namespace OrderFlow.Domain.Tests.SharedLayer;

/// <summary>
/// The six audit columns on <see cref="Entity"/> (golden rule 19, ADR-0016): they exist, they are
/// nullable, only the type itself can set them, and their names are the ones in <see cref="AuditColumns"/>.
/// </summary>
public sealed class EntityAuditColumnsTests
{
    private static readonly Dictionary<string, Type> ExpectedAuditProperties = new(StringComparer.Ordinal)
    {
        [AuditColumns.CreatorId] = typeof(Guid?),
        [AuditColumns.CreationTime] = typeof(DateTime?),
        [AuditColumns.LastModifierId] = typeof(Guid?),
        [AuditColumns.LastModificationTime] = typeof(DateTime?),
        [AuditColumns.DeletedBy] = typeof(Guid?),
        [AuditColumns.DeletedAt] = typeof(DateTime?),
    };

    private static readonly string[] ExpectedColumnNames =
        [.. ExpectedAuditProperties.Keys.Order(StringComparer.Ordinal)];

    public static TheoryData<string, Type> AuditProperties =>
        new(ExpectedAuditProperties.Select(pair => (pair.Key, pair.Value)));

    public static TheoryData<string> AuditColumnNames => new(ExpectedColumnNames);

    [Fact]
    public void ThereAreSixAuditColumns()
    {
        Assert.Equal(6, ExpectedColumnNames.Length);
    }

    [Theory]
    [MemberData(nameof(AuditProperties))]
    public void AuditPropertyExistsWithTheExpectedNullableType(string name, Type expectedType)
    {
        var property = typeof(Entity).GetProperty(name, BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(property);
        Assert.Equal(expectedType, property.PropertyType);
        Assert.NotNull(Nullable.GetUnderlyingType(property.PropertyType));
    }

    [Theory]
    [MemberData(nameof(AuditColumnNames))]
    public void AuditPropertyHasAPublicGetterAndAPrivateSetter(string name)
    {
        var property = typeof(Entity).GetProperty(name, BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(property);
        Assert.True(property.GetMethod?.IsPublic, $"{name} must have a public getter.");
        Assert.NotNull(property.SetMethod);
        Assert.True(property.SetMethod.IsPrivate, $"{name} must have a private setter.");
    }

    [Fact]
    public void AuditColumnsDeclaresExactlyTheSixColumnNames()
    {
        var columnNames = typeof(AuditColumns)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral)
            .Select(field => (string)field.GetRawConstantValue()!)
            .Order(StringComparer.Ordinal);

        Assert.Equal(ExpectedColumnNames, columnNames);
    }

    [Fact]
    public void AuditColumnNamesMatchTheAuditInterfaceImplementedByEntity()
    {
        Assert.True(typeof(IFullAudited).IsAssignableFrom(typeof(Entity)));

        var interfaceProperties = typeof(IFullAudited)
            .GetProperties()
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal);

        Assert.Equal(ExpectedColumnNames, interfaceProperties);
    }

    [Fact]
    public void ANewEntityHasEveryAuditValueNull()
    {
        var entity = new TestEntity(Guid.CreateVersion7());

        Assert.Null(entity.CreatorId);
        Assert.Null(entity.CreationTime);
        Assert.Null(entity.LastModifierId);
        Assert.Null(entity.LastModificationTime);
        Assert.Null(entity.DeletedBy);
        Assert.Null(entity.DeletedAt);
    }

    [Fact]
    public void EntityKeepsTheGuidIdentityItWasCreatedWith()
    {
        var id = Guid.CreateVersion7();

        var entity = new TestEntity(id);

        Assert.Equal(id, entity.Id);
        Assert.Equal(typeof(Guid), typeof(Entity).GetProperty(nameof(Entity.Id))!.PropertyType);
    }

    private sealed class TestEntity(Guid id) : Entity(id);
}
