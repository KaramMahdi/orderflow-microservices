using Domain.Shared.Layer.Localization;

namespace OrderFlow.Domain.Tests.SharedLayer;

/// <summary>
/// The supported languages (ADR-0021): Arabic and English, English by default.
/// </summary>
public sealed class LanguagesTests
{
    [Fact]
    public void TheCodesAreTheTwoLetterCodesOfTheTranslationsTable()
    {
        Assert.Equal("ar", Languages.Arabic);
        Assert.Equal("en", Languages.English);
    }

    [Fact]
    public void EnglishIsTheDefault()
    {
        Assert.Equal(Languages.English, Languages.Default);
    }

    [Fact]
    public void ArabicAndEnglishAreTheOnlySupportedLanguages()
    {
        Assert.Equal([Languages.Arabic, Languages.English], Languages.Supported);
    }

    [Fact]
    public void TheDefaultIsSupported()
    {
        Assert.Contains(Languages.Default, Languages.Supported);
    }

    [Theory]
    [InlineData(Languages.Arabic)]
    [InlineData(Languages.English)]
    [InlineData("AR")]
    [InlineData("En")]
    public void SupportedCodesAreRecognizedIgnoringCase(string language)
    {
        Assert.True(Languages.IsSupported(language));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("fr")]
    [InlineData("en-US")]
    public void OtherValuesAreNotSupported(string? language)
    {
        Assert.False(Languages.IsSupported(language));
    }
}
