using SpectraUtils.Services;
using System.Globalization;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class NameEditTests
{
    private readonly NameEdit _sut = new();

    [Fact]
    public void NameCorrection_Null_ReturnsNull()
    {
        Assert.Null(_sut.NameCorrection((string?)null));
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("  ", "")]
    [InlineData("oğuzHan", "Oğuzhan")]
    [InlineData(" i", "İ")]
    [InlineData("I", "I")]
    [InlineData("ı", "I")]
    [InlineData("İSMAİL", "İsmail")]
    [InlineData("ILGIN", "Ilgın")]
    [InlineData("ahmet can", "Ahmet Can")]
    [InlineData("  aYşe   nur ", "Ayşe Nur")]
    [InlineData("ayşe\tnur", "Ayşe Nur")]
    [InlineData("ayşe-nur", "Ayşe-Nur")]
    public void NameCorrection_SingleName_ProducesExpected(string input, string expected)
    {
        Assert.Equal(expected, _sut.NameCorrection(input));
    }

    [Fact]
    public void NameCorrection_Params_IgnoresNullAndEmpty_AndJoinsWithSpaces()
    {
        Assert.Equal("Oğuzhan Ali", _sut.NameCorrection("oğuzHan", null, "", "  ", "aLi"));
    }

    [Fact]
    public void NameCorrection_InvariantCulture_UsesEnglishCasing()
    {
        var sut = new NameEdit(CultureInfo.InvariantCulture);

        Assert.Equal("Isabel", sut.NameCorrection("isabel"));
        Assert.Equal("Isabel", sut.NameCorrection("ISABEL"));
    }

    [Fact]
    public void NameCorrection_AzerbaijaniCulture_UsesTurkicCasing()
    {
        var sut = new NameEdit(CultureInfo.GetCultureInfo("az-Latn-AZ"));
        Assert.Equal("İlkin", sut.NameCorrection("ilkin"));
    }

    [Fact]
    public void Constructor_NullCulture_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new NameEdit(null!));
    }

    [Fact]
    public void SurnameCorrection_Null_ReturnsNull()
    {
        Assert.Null(_sut.SurnameCorrection((string?)null));
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("karagüzel", "KARAGÜZEL")]
    [InlineData("  karagüzel  ", "KARAGÜZEL")]
    [InlineData("kara  güzel", "KARA GÜZEL")]
    [InlineData("yılmaz", "YILMAZ")]
    [InlineData("ışık", "IŞIK")]
    [InlineData("işık", "İŞIK")]
    public void SurnameCorrection_UppercasesTurkish_AndNormalizesWhitespace(string input, string expected)
    {
        Assert.Equal(expected, _sut.SurnameCorrection(input));
    }

    [Fact]
    public void SurnameCorrection_Params_JoinsWithSpaces()
    {
        Assert.Equal("KARA GÜZEL", _sut.SurnameCorrection("kara", null, "güzel"));
    }

    [Fact]
    public void FullNameCorrection_BothNullOrEmpty_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, _sut.FullNameCorrection((string?)null, (string?)null));
        Assert.Equal(string.Empty, _sut.FullNameCorrection(" ", ""));
    }

    [Fact]
    public void FullNameCorrection_OnlyName_ReturnsName()
    {
        Assert.Equal("Oğuzhan", _sut.FullNameCorrection("oğuzHan", null));
    }

    [Fact]
    public void FullNameCorrection_OnlySurname_ReturnsSurname()
    {
        Assert.Equal("KARAGÜZEL", _sut.FullNameCorrection(null, "karagüzel"));
    }

    [Fact]
    public void FullNameCorrection_Both_ReturnsCombined()
    {
        Assert.Equal("Oğuzhan KARAGÜZEL", _sut.FullNameCorrection("oğuzHan", "karagüzel"));
    }

    [Fact]
    public void FullNameCorrection_Arrays_ReturnsCombined()
    {
        Assert.Equal("Mehmet Ali YILMAZ KAYA", _sut.FullNameCorrection(["mehmet", "ali"], ["yılmaz", "kaya"]));
        Assert.Equal("YILMAZ", _sut.FullNameCorrection(null, ["yılmaz"]));
    }

    [Fact]
    public void StandardizeCharacters_Null_ReturnsNull()
    {
        Assert.Null(_sut.StandardizeCharacters(null));
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("plain ascii", "plain ascii")]
    [InlineData("oğuzhan", "oguzhan")]
    [InlineData("İstanbul", "Istanbul")]
    [InlineData("Işık", "Isik")]
    [InlineData("Çağdaş Şöhret", "Cagdas Sohret")]
    [InlineData("ĞÜŞÖÇ ğüşöç", "GUSOC gusoc")]
    [InlineData("Crème Brûlée", "Creme Brulee")]
    [InlineData("Łódź Øresund Straße", "Lodz Oresund Strasse")]
    public void StandardizeCharacters_RemovesDiacritics_AndMapsSpecialLetters(string input, string expected)
    {
        Assert.Equal(expected, _sut.StandardizeCharacters(input));
    }

    [Fact]
    public void StandardizeCharacters_LatinTable_MatchesUnicodeDecomposition()
    {
        // The built-in table must agree with ICU's NFD decomposition, so results are identical
        // whether or not the app runs in globalization-invariant mode.
        for (char c = 'À'; c <= 'ɏ'; c++)
        {
            string expected = string.Concat(c.ToString().Normalize(System.Text.NormalizationForm.FormD)
                .Where(x => CharUnicodeInfo.GetUnicodeCategory(x) != UnicodeCategory.NonSpacingMark));

            if (expected.Length == 1 && char.IsAscii(expected[0]))
                Assert.Equal(expected, _sut.StandardizeCharacters(c.ToString()));
        }
    }

    [Fact]
    public void CreateUserName_Null_ReturnsNull()
    {
        Assert.Null(_sut.CreateUserName(null));
    }

    [Theory]
    [InlineData("oğuzhan", "oguzhan")]
    [InlineData("Ayşe Nur", "aysenur")]
    [InlineData("  Işıl O'Neil ", "isiloneil")]
    public void CreateUserName_ProducesLowercaseAsciiWithFourDigitSuffix(string input, string expectedPrefix)
    {
        string output = _sut.CreateUserName(input);

        Assert.Equal(expectedPrefix.Length + 4, output.Length);
        Assert.StartsWith(expectedPrefix, output, StringComparison.Ordinal);
        Assert.All(output[^4..], c => Assert.InRange(c, '0', '9'));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("@@@")]
    public void CreateUserName_NoLettersOrDigits_Throws(string input)
    {
        Assert.Throws<ArgumentException>(() => _sut.CreateUserName(input));
    }
}
