using SpectraUtils.Concerete;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class SecureTokenGeneratorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GenerateToken_InvalidLength_Throws(int length)
    {
        var generator = new SecureTokenGenerator();
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.GenerateToken(length));
    }

    [Fact]
    public void GenerateToken_DefaultLength_ReturnsBase64Url()
    {
        var generator = new SecureTokenGenerator();
        string token = generator.GenerateToken();

        Assert.DoesNotContain(token, c => c == '+');
        Assert.DoesNotContain(token, c => c == '/');
        Assert.DoesNotContain(token, c => c == '=');
    }

    [Fact]
    public void GenerateToken_RespectsByteLength()
    {
        const int byteLength = 24;
        var generator = new SecureTokenGenerator();

        string token = generator.GenerateToken(byteLength);

        int padding = (3 - (byteLength % 3)) % 3;
        int expectedLength = 4 * ((byteLength + 2) / 3) - padding;

        Assert.Equal(expectedLength, token.Length);
    }
}
