namespace OrderFlow.Domain.Shared.Customers;

/// <summary>Limits of the Customers model.</summary>
public static class CustomerConstants
{
    /// <summary>The maximum length of an email address (RFC 5321 path limit).</summary>
    public const int EmailMaxLength = 254;
}
