using SpectraUtils.Concerete;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class Pbkdf2PasswordHasherTests
{
    [Theory]
    [InlineData(0, 16, 32)]
    [InlineData(1, 0, 32)]
    [InlineData(1, 16, 0)]
    public void Constructor_InvalidParameters_Throws(int iterations, int saltSize, int keySize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Pbkdf2PasswordHasher(iterations, saltSize, keySize));
    }

    [Fact]
    public void Hash_NullPassword_Throws()
    {
        var hasher = new Pbkdf2PasswordHasher();
        Assert.Throws<ArgumentNullException>(() => hasher.Hash(null!));
    }

    [Fact]
    public void Verify_NullParameters_Throws()
    {
        var hasher = new Pbkdf2PasswordHasher();
        string hash = hasher.Hash("secret");

        Assert.Throws<ArgumentNullException>(() => hasher.Verify(null!, hash));
        Assert.Throws<ArgumentNullException>(() => hasher.Verify("secret", null!));
    }

    [Fact]
    public void HashAndVerify_Succeeds()
    {
        var hasher = new Pbkdf2PasswordHasher();
        string hash = hasher.Hash("password");

        bool ok = hasher.Verify("password", hash);

        Assert.True(ok);
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hasher = new Pbkdf2PasswordHasher();
        string hash = hasher.Hash("password");

        Assert.False(hasher.Verify("other", hash));
    }

    [Fact]
    public void Verify_InvalidHash_ReturnsFalse()
    {
        var hasher = new Pbkdf2PasswordHasher();
        Assert.False(hasher.Verify("password", "bad-hash"));
    }

    [Fact]
    public void Hash_Format_IsExpected()
    {
        var hasher = new Pbkdf2PasswordHasher(10, 8, 8);
        string hash = hasher.Hash("pwd");

        string[] parts = hash.Split('$', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        Assert.Equal(5, parts.Length);
        Assert.Equal("PBKDF2", parts[0]);
        Assert.Equal("SHA256", parts[1]);
        Assert.True(int.Parse(parts[2]) > 0);
        Assert.False(string.IsNullOrWhiteSpace(parts[3]));
        Assert.False(string.IsNullOrWhiteSpace(parts[4]));
    }
}
