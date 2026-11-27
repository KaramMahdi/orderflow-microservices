using System.Text;

namespace OrderFlow.Domain.Catalog;

/// <summary>
/// The normalization of ADR-0014, run on a product name when it is saved and on the search text
/// before the prefix query: alef forms become bare alef, teh marbuta becomes heh, alef maksura
/// becomes yeh, and tashkeel and tatweel are removed. Nothing else changes.
/// </summary>
public static class ProductNameNormalizer
{
    private const char Alef = 'ا';
    private const char AlefWithHamzaAbove = 'أ';
    private const char AlefWithHamzaBelow = 'إ';
    private const char AlefWithMaddaAbove = 'آ';
    private const char TehMarbuta = 'ة';
    private const char Heh = 'ه';
    private const char AlefMaksura = 'ى';
    private const char Yeh = 'ي';
    private const char Tatweel = 'ـ';
    private const char FirstTashkeel = 'ً';
    private const char LastTashkeel = 'ْ';

    /// <summary>Returns the normalized text.</summary>
    /// <param name="text">A product name or a search text.</param>
    public static string Normalize(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var builder = new StringBuilder(text.Length);

        foreach (var character in text)
        {
            switch (character)
            {
                case AlefWithHamzaAbove or AlefWithHamzaBelow or AlefWithMaddaAbove:
                    builder.Append(Alef);
                    break;
                case TehMarbuta:
                    builder.Append(Heh);
                    break;
                case AlefMaksura:
                    builder.Append(Yeh);
                    break;
                case Tatweel:
                case >= FirstTashkeel and <= LastTashkeel:
                    break;
                default:
                    builder.Append(character);
                    break;
            }
        }

        return builder.ToString();
    }
}
