using Domain.Shared.Layer.Constants;
using Domain.Shared.Layer.Localization;

namespace OrderFlow.Domain.Shared.Customers;

/// <summary>Message keys of the Customers module (ADR-0021).</summary>
public static class CustomersMessageKeys
{
    /// <summary>A required field is empty. Arguments: the field name.</summary>
    public static readonly MessageKey FieldRequired = new(Modules.Customers, nameof(FieldRequired));

    /// <summary>A field is too long. Arguments: the field name, the maximum length.</summary>
    public static readonly MessageKey FieldTooLong = new(Modules.Customers, nameof(FieldTooLong));

    /// <summary>The email address is not valid. Arguments: the email.</summary>
    public static readonly MessageKey EmailInvalid = new(Modules.Customers, nameof(EmailInvalid));

    /// <summary>Another customer already has the email. Arguments: the email.</summary>
    public static readonly MessageKey EmailAlreadyRegistered = new(Modules.Customers, nameof(EmailAlreadyRegistered));

    /// <summary>The user is already registered as a customer. Arguments: the user id.</summary>
    public static readonly MessageKey UserAlreadyRegistered = new(Modules.Customers, nameof(UserAlreadyRegistered));
}
