using SpectraUtils.Services;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class PasswordHelperTests
{
    private const string Specials = "!@#$%^&*()-_=+[]{}|;:',.<>?/`~";

    private readonly PasswordHelper _sut = new();

    [Theory]
    [InlineData(3)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(PasswordHelper.MaximumPasswordLength + 1)]
    public void CreatePassword_LengthOutOfRange_Throws(int length)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.CreatePassword(length));
    }

    [Fact]
    public void CreatePassword_NoCharacterTypesSelected_Throws()
    {
        Assert.Throws<ArgumentException>(() => _sut.CreatePassword(
            passwordLength: 8,
            includeUppercaseLetter: false,
            includeLowercaseLetter: false,
            includeSpecialCharacter: false,
            includeNumber: false));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(64)]
    [InlineData(PasswordHelper.MaximumPasswordLength)]
    public void CreatePassword_ReturnsRequestedLength(int length)
    {
        Assert.Equal(length, _sut.CreatePassword(length).Length);
    }

    [Fact]
    public void CreatePassword_MinimumLength_AlwaysContainsEveryCategory()
    {
        for (int i = 0; i < 500; i++)
        {
            string pwd = _sut.CreatePassword(4);

            Assert.Contains(pwd, char.IsAsciiLetterUpper);
            Assert.Contains(pwd, char.IsAsciiLetterLower);
            Assert.Contains(pwd, char.IsAsciiDigit);
            Assert.Contains(pwd, c => Specials.Contains(c));
        }
    }

    [Fact]
    public void CreatePassword_RequiredCharactersAreNotAtFixedPositions()
    {
        // With 2 categories and length 4 the guaranteed characters used to be written first and then shuffled;
        // make sure a digit shows up at every position over many runs.
        var positions = new HashSet<int>();

        for (int i = 0; i < 500; i++)
        {
            string pwd = _sut.CreatePassword(4, includeUppercaseLetter: false, includeSpecialCharacter: false);
            for (int p = 0; p < pwd.Length; p++)
            {
                if (char.IsAsciiDigit(pwd[p]))
                    positions.Add(p);
            }
        }

        Assert.Equal(4, positions.Count);
    }

    [Theory]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, false, false, true)]
    public void CreatePassword_SingleCategory_ContainsOnlyThatCategory(bool upper, bool lower, bool special, bool number)
    {
        string pwd = _sut.CreatePassword(32, upper, lower, special, number);

        Func<char, bool> isAllowed =
            upper ? char.IsAsciiLetterUpper :
            lower ? char.IsAsciiLetterLower :
            special ? Specials.Contains :
            char.IsAsciiDigit;

        Assert.All(pwd, c => Assert.True(isAllowed(c), $"Unexpected character '{c}'."));
    }

    [Fact]
    public void CreateStandardPassword_Returns12CharactersFromAllCategories()
    {
        string pwd = _sut.CreateStandardPassword();

        Assert.Equal(PasswordHelper.StandardPasswordLength, pwd.Length);
        Assert.Contains(pwd, char.IsAsciiLetterUpper);
        Assert.Contains(pwd, char.IsAsciiLetterLower);
        Assert.Contains(pwd, char.IsAsciiDigit);
        Assert.Contains(pwd, c => Specials.Contains(c));
    }

    [Fact]
    public void CreateStandardPassword_ReturnsDifferentValues()
    {
        Assert.NotEqual(_sut.CreateStandardPassword(), _sut.CreateStandardPassword());
    }

#pragma warning disable SPECTRA001 // HashPassword is obsolete, but it must keep working for migrations.
    [Fact]
    public void HashPassword_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.HashPassword(null!));
    }

    [Fact]
    public void HashPassword_KnownInput_ReturnsExpectedSha256HexLowercase()
    {
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", _sut.HashPassword("abc"));
    }
#pragma warning restore SPECTRA001

    [Fact]
    public void HashPassword_IsMarkedObsoleteWithDiagnosticId()
    {
        var attribute = typeof(PasswordHelper).GetMethod(nameof(PasswordHelper.HashPassword))!
            .GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false)
            .Cast<ObsoleteAttribute>()
            .Single();

        Assert.Equal("SPECTRA001", attribute.DiagnosticId);
        Assert.False(attribute.IsError);
    }
}
