using SpectraUtils.Concrete;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class PasswordHelperTests
{
    private readonly PasswordHelper _sut = new();

    [Fact]
    public void CreatePassword_LengthLessThanMinimum_Throws()
    {
        Assert.Throws<ArgumentException>(() => _sut.CreatePassword(3));
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
    public void CreatePassword_ReturnsRequestedLength(int length)
    {
        string pwd = _sut.CreatePassword(length);
        Assert.Equal(length, pwd.Length);
    }

    [Fact]
    public void CreatePassword_AllCategories_Selected_ContainsAtLeastOneFromEach()
    {
        string pwd = _sut.CreatePassword(
            passwordLength: 12,
            includeUppercaseLetter: true,
            includeLowercaseLetter: true,
            includeSpecialCharacter: true,
            includeNumber: true);

        Assert.Contains(pwd, c => c is >= 'A' and <= 'Z');
        Assert.Contains(pwd, c => c is >= 'a' and <= 'z');
        Assert.Contains(pwd, c => c is >= '0' and <= '9');
        Assert.Contains(pwd, c => "!@#$%^&*()-_=+[]{}|;:',.<>?/`~".Contains(c));
    }

    [Fact]
    public void CreatePassword_OnlyUppercase_ContainsOnlyUppercase()
    {
        string pwd = _sut.CreatePassword(
            passwordLength: 16,
            includeUppercaseLetter: true,
            includeLowercaseLetter: false,
            includeSpecialCharacter: false,
            includeNumber: false);

        Assert.All(pwd, c => Assert.InRange(c, 'A', 'Z'));
    }

    [Fact]
    public void CreatePassword_OnlyLowercase_ContainsOnlyLowercase()
    {
        string pwd = _sut.CreatePassword(
            passwordLength: 16,
            includeUppercaseLetter: false,
            includeLowercaseLetter: true,
            includeSpecialCharacter: false,
            includeNumber: false);

        Assert.All(pwd, c => Assert.InRange(c, 'a', 'z'));
    }

    [Fact]
    public void CreatePassword_OnlyNumbers_ContainsOnlyNumbers()
    {
        string pwd = _sut.CreatePassword(
            passwordLength: 16,
            includeUppercaseLetter: false,
            includeLowercaseLetter: false,
            includeSpecialCharacter: false,
            includeNumber: true);

        Assert.All(pwd, c => Assert.InRange(c, '0', '9'));
    }

    [Fact]
    public void CreatePassword_OnlySpecials_ContainsOnlySpecials()
    {
        const string specials = "!@#$%^&*()-_=+[]{}|;:',.<>?/`~";

        string pwd = _sut.CreatePassword(
            passwordLength: 16,
            includeUppercaseLetter: false,
            includeLowercaseLetter: false,
            includeSpecialCharacter: true,
            includeNumber: false);

        Assert.All(pwd, c => Assert.True(specials.Contains(c)));
    }

    [Fact]
    public void CreateStardartPassword_Returns8Chars()
    {
        string pwd = _sut.CreateStardartPassword;
        Assert.Equal(8, pwd.Length);
    }

    [Fact]
    public void HashPassword_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.HashPassword(null!));
    }

    [Fact]
    public void HashPassword_KnownInput_ReturnsExpectedSha256HexLowercase()
    {
        string hash = _sut.HashPassword("abc");

        Assert.Equal(64, hash.Length);
        Assert.All(hash, c => Assert.True((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')));
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", hash);
    }
}
