using System.Net.Mail;
using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.Errors;
using OrderFlow.Domain.Shared.Customers;

namespace OrderFlow.Domain.Customers;

/// <summary>
/// A validated email address (glossary), trimmed and in lower case so uniqueness does not depend on
/// case. A bare address only: no display name.
/// </summary>
public sealed class Email : ValueObject
{
    private Email(string value)
    {
        Value = value;
    }

    /// <summary>The address.</summary>
    public string Value { get; }

    /// <summary>Creates a validated email address.</summary>
    /// <param name="value">The address.</param>
    public static Email Create(string? value)
    {
        var address = Check.Text(value, nameof(Email), CustomerConstants.EmailMaxLength, CustomersMessageKeys.FieldRequired, CustomersMessageKeys.FieldTooLong)
            .ToLowerInvariant();

        if (!MailAddress.TryCreate(address, out var parsed)
            || !string.Equals(parsed.Address, address, StringComparison.Ordinal)
            || !string.IsNullOrEmpty(parsed.DisplayName))
        {
            throw new ErrorException(ErrorCode.ValidationFailed, CustomersMessageKeys.EmailInvalid, value);
        }

        return new Email(address);
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
