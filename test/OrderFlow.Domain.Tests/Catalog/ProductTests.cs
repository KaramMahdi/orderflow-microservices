using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Catalog;
using OrderFlow.Domain.Catalog.Events;
using OrderFlow.Domain.Catalog.Specifications;
using OrderFlow.Domain.Shared.Catalog;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.Catalog;

/// <summary>Product, Sku, prices and the ADR-0014 normalization.</summary>
public sealed class ProductTests
{
    private const string Usd = "USD";

    [Theory]
    [InlineData("أحمد", "احمد")]
    [InlineData("إسلام", "اسلام")]
    [InlineData("آمن", "امن")]
    [InlineData("مدرسة", "مدرسه")]
    [InlineData("مستشفى", "مستشفي")]
    [InlineData("مُحَمَّد", "محمد")]
    [InlineData("كتـــاب", "كتاب")]
    [InlineData("Phone X", "Phone X")]
    public void NormalizerAppliesTheAdrRules(string text, string expected)
    {
        Assert.Equal(expected, ProductNameNormalizer.Normalize(text));
    }

    [Fact]
    public void ASearchForTheBareFormFindsTheHamzaForm()
    {
        var product = Product.Create(Sku.Create("P-1"), "أحمد شاي");

        Assert.True(new ProductNameStartsWithSpecification("احمد").IsSatisfiedBy(product));
        Assert.False(new ProductNameStartsWithSpecification("شاي").IsSatisfiedBy(product));
    }

    [Fact]
    public void SkuIsTrimmedAndUpperCase()
    {
        Assert.Equal("AB-12_X", Sku.Create(" ab-12_x ").Value);
    }

    [Theory]
    [InlineData("AB 12")]
    [InlineData("AB/12")]
    public void SkuRejectsOtherCharacters(string value)
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CatalogMessageKeys.SkuInvalid, () => Sku.Create(value));
    }

    [Fact]
    public void RenameKeepsTheNormalizedNameInStep()
    {
        var product = Product.Create(Sku.Create("P-1"), "شاي");

        product.Rename("قهوة");

        Assert.Equal("قهوة", product.Name);
        Assert.Equal("قهوه", product.NormalizedName);
    }

    [Fact]
    public void OnePricePerCurrency()
    {
        var product = Product.Create(Sku.Create("P-1"), "Tea");

        product.SetPrice(Money.Create(1m, Usd));
        product.SetPrice(Money.Create(2m, Usd));
        product.SetPrice(Money.Create(3m, "EUR"));

        Assert.Equal(2, product.Prices.Count);
        Assert.Equal(Money.Create(2m, Usd), product.GetPrice("usd"));
        Assert.Null(product.GetPrice("JOD"));
    }

    [Fact]
    public void PriceChangedIsRaisedOnlyForARealChange()
    {
        var product = Product.Create(Sku.Create("P-1"), "Tea");

        product.SetPrice(Money.Create(1m, Usd));
        product.SetPrice(Money.Create(1m, Usd));
        product.SetPrice(Money.Create(2m, Usd));

        Assert.Collection(
            product.DomainEvents,
            domainEvent => Assert.Equal(new ProductPriceChanged(product.Id, Usd, null, 1m), domainEvent),
            domainEvent => Assert.Equal(new ProductPriceChanged(product.Id, Usd, 1m, 2m), domainEvent));
    }

    [Fact]
    public async Task ManagerRejectsAnExistingSku()
    {
        var existing = Product.Create(Sku.Create("P-1"), "Tea");
        var manager = new ProductManager(new ProductRepository([existing]), new InMemoryQueryableExecuter());

        await ErrorAssert.ThrowsAsync(ErrorCode.Conflict, CatalogMessageKeys.SkuAlreadyExists, () => manager.CreateAsync("p-1", "Other", TestContext.Current.CancellationToken));

        var created = await manager.CreateAsync("P-2", "Coffee", TestContext.Current.CancellationToken);
        Assert.Equal("P-2", created.Sku.Value);
    }

    private sealed class ProductRepository(List<Product> products) : IProductRepository
    {
        public IQueryable<Product> GetQueryable() => products.AsQueryable();
    }
}
