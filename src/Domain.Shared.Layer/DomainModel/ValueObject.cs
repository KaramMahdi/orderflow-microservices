namespace Domain.Shared.Layer.DomainModel;

/// <summary>
/// An immutable object compared by value. Derived types list the values that define equality
/// in <see cref="GetEqualityComponents"/>.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>Returns true when both value objects are equal (both null counts as equal).</summary>
    public static bool operator ==(ValueObject? left, ValueObject? right) => Equals(left, right);

    /// <summary>Returns true when the value objects are not equal.</summary>
    public static bool operator !=(ValueObject? left, ValueObject? right) => !Equals(left, right);

    /// <inheritdoc />
    public bool Equals(ValueObject? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType()
            && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as ValueObject);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(GetType());

        foreach (var component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }

    /// <summary>The values that define equality, in a fixed order.</summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();
}
