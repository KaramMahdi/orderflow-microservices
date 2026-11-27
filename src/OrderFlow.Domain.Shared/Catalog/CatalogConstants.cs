namespace OrderFlow.Domain.Shared.Catalog;

/// <summary>Limits of the Catalog model.</summary>
public static class CatalogConstants
{
    /// <summary>The maximum length of a SKU.</summary>
    public const int SkuMaxLength = 64;

    /// <summary>The maximum length of a product name (and of its normalized form).</summary>
    public const int ProductNameMaxLength = 200;
}
