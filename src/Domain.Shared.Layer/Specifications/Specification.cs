using System.Linq.Expressions;

namespace Domain.Shared.Layer.Specifications;

/// <summary>
/// A reusable query rule built on an expression, so the same rule filters an
/// <see cref="IQueryable{T}"/> from a repository (translated to SQL) and checks an object in memory.
/// </summary>
/// <typeparam name="T">The type the rule applies to.</typeparam>
public abstract class Specification<T>
{
    private Func<T, bool>? _compiled;

    /// <summary>The rule as an expression, for <c>IQueryable.Where</c>.</summary>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <summary>Returns true when the candidate satisfies the rule.</summary>
    /// <param name="candidate">The object to check.</param>
    public bool IsSatisfiedBy(T candidate)
    {
        _compiled ??= ToExpression().Compile();

        return _compiled(candidate);
    }

    /// <summary>Returns a rule satisfied only when both rules are satisfied.</summary>
    /// <param name="other">The other rule.</param>
    public Specification<T> And(Specification<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return new AndSpecification<T>(this, other);
    }
}
