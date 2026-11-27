using System.Linq.Expressions;
using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.Specifications;

namespace OrderFlow.Domain.Tests.SharedLayer;

/// <summary>Specification combination and the domain event list of an aggregate root.</summary>
public sealed class SpecificationAndEventTests
{
    [Fact]
    public void AndIsSatisfiedOnlyWhenBothRulesAre()
    {
        var spec = new GreaterThan(2).And(new LessThan(5));

        Assert.True(spec.IsSatisfiedBy(3));
        Assert.False(spec.IsSatisfiedBy(2));
        Assert.False(spec.IsSatisfiedBy(5));
    }

    [Fact]
    public void AndProducesOneExpressionThatFiltersAQuery()
    {
        var expression = new GreaterThan(2).And(new LessThan(5)).ToExpression();

        Assert.Single(expression.Parameters);
        Assert.Equal([3, 4], Enumerable.Range(0, 10).AsQueryable().Where(expression));
    }

    [Fact]
    public void AggregateCollectsAndClearsItsEvents()
    {
        var aggregate = new TestAggregate();
        var first = new TestEvent(1);
        var second = new TestEvent(2);

        aggregate.Raise(first);
        aggregate.Raise(second);

        Assert.Equal([first, second], aggregate.DomainEvents);

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }

    private sealed class GreaterThan(int limit) : Specification<int>
    {
        public override Expression<Func<int, bool>> ToExpression() => value => value > limit;
    }

    private sealed class LessThan(int limit) : Specification<int>
    {
        public override Expression<Func<int, bool>> ToExpression() => value => value < limit;
    }

    private sealed record TestEvent(int Number) : IDomainEvent;

    private sealed class TestAggregate() : AggregateRoot(Guid.CreateVersion7())
    {
        public void Raise(IDomainEvent domainEvent) => AddDomainEvent(domainEvent);
    }
}
