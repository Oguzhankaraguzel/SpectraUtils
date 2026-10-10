using SpectraUtils.Services;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class SecureTokenGeneratorTests
{
    private readonly SecureTokenGenerator _sut = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(SecureTokenGenerator.MaxByteLength + 1)]
    public void GenerateToken_InvalidLength_Throws(int length)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _sut.GenerateToken(length));
    }

    [Fact]
    public void GenerateToken_DefaultLength_ReturnsBase64UrlWithoutPadding()
    {
        string token = _sut.GenerateToken();

        Assert.Equal(43, token.Length);
        Assert.All(token, c => Assert.True(char.IsAsciiLetterOrDigit(c) || c is '-' or '_', $"Unexpected character '{c}'."));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(24)]
    [InlineData(64)]
    [InlineData(SecureTokenGenerator.MaxByteLength)]
    public void GenerateToken_RespectsByteLength(int byteLength)
    {
        string token = _sut.GenerateToken(byteLength);

        int expectedLength = (byteLength * 4 + 2) / 3;
        Assert.Equal(expectedLength, token.Length);

        string base64 = token.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        Assert.Equal(byteLength, Convert.FromBase64String(base64).Length);
    }

    [Fact]
    public void GenerateToken_ReturnsDifferentValues()
    {
        Assert.NotEqual(_sut.GenerateToken(), _sut.GenerateToken());
    }
}
