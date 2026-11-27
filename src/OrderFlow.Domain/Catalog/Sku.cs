using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.Errors;
using OrderFlow.Domain.Shared.Catalog;

namespace OrderFlow.Domain.Catalog;

/// <summary>
/// The stock-keeping unit: the business code of a product (glossary). Trimmed and upper case;
/// letters, digits, '-' and '_' only. Uniqueness among products is checked by <see cref="ProductManager"/>.
/// </summary>
public sealed class Sku : ValueObject
{
    private const char Hyphen = '-';
    private const char Underscore = '_';

    private Sku(string value)
    {
        Value = value;
    }

    /// <summary>The code.</summary>
    public string Value { get; }

    /// <summary>Creates a validated SKU.</summary>
    /// <param name="value">The code, any case.</param>
    public static Sku Create(string? value)
    {
        var code = Check.Text(value, nameof(Sku), CatalogConstants.SkuMaxLength, CatalogMessageKeys.FieldRequired, CatalogMessageKeys.FieldTooLong)
            .ToUpperInvariant();

        if (!code.All(character => char.IsAsciiLetterOrDigit(character) || character is Hyphen or Underscore))
        {
            throw new ErrorException(ErrorCode.ValidationFailed, CatalogMessageKeys.SkuInvalid, value);
        }

        return new Sku(code);
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
