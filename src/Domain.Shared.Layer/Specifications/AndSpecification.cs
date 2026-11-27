using System.Linq.Expressions;

namespace Domain.Shared.Layer.Specifications;

/// <summary>Two rules joined with AND, in one expression that a query provider can translate.</summary>
/// <typeparam name="T">The type the rules apply to.</typeparam>
internal sealed class AndSpecification<T>(Specification<T> left, Specification<T> right) : Specification<T>
{
    /// <inheritdoc />
    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = left.ToExpression();
        var rightExpression = right.ToExpression();
        var parameter = leftExpression.Parameters[0];
        var rightBody = new ParameterReplacer(rightExpression.Parameters[0], parameter).Visit(rightExpression.Body);

        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(leftExpression.Body, rightBody), parameter);
    }

    private sealed class ParameterReplacer(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == from ? to : node;
    }
}
