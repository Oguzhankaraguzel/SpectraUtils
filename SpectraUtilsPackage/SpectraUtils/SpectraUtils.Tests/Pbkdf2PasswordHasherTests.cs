using SpectraUtils.Configuration;
using SpectraUtils.Services;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace SpectraUtils.Tests;

public sealed class Pbkdf2PasswordHasherTests
{
    // Low iteration counts keep the tests fast; production defaults are tested separately.
    private static Pbkdf2PasswordHasher CreateFast(Action<Pbkdf2Options>? configure = null)
    {
        var options = new Pbkdf2Options { Iterations = 1_000 };
        configure?.Invoke(options);
        return new Pbkdf2PasswordHasher(options);
    }

    [Fact]
    public void Defaults_FollowOwaspRecommendation()
    {
        var options = new Pbkdf2Options();

        Assert.Equal(600_000, options.Iterations);
        Assert.Equal(Pbkdf2HashAlgorithm.Sha256, options.Algorithm);
        Assert.Equal(16, options.SaltSize);
        Assert.Equal(32, options.KeySize);
    }

    [Fact]
    public void DefaultConstructor_ProducesHashWithDefaultParameters()
    {
        string hash = new Pbkdf2PasswordHasher().Hash("pwd");
        Assert.StartsWith("PBKDF2$SHA256$600000$", hash, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0, 16, 32)]
    [InlineData(-1, 16, 32)]
    [InlineData(1_000, 7, 32)]
    [InlineData(1_000, 16, 15)]
    [InlineData(1_000, 1025, 32)]
    [InlineData(1_000, 16, 1025)]
    public void Constructor_InvalidOptions_Throws(int iterations, int saltSize, int keySize)
    {
        var options = new Pbkdf2Options { Iterations = iterations, SaltSize = saltSize, KeySize = keySize };
        Assert.Throws<ArgumentOutOfRangeException>(() => new Pbkdf2PasswordHasher(options));
    }

    [Fact]
    public void Constructor_MaxIterationsBelowIterations_Throws()
    {
        var options = new Pbkdf2Options { Iterations = 1_000, MaxIterations = 999 };
        Assert.Throws<ArgumentOutOfRangeException>(() => new Pbkdf2PasswordHasher(options));
    }

    [Fact]
    public void Constructor_NullOptions_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new Pbkdf2PasswordHasher(null!));
    }

    [Fact]
    public void Constructor_CopiesOptions()
    {
        var options = new Pbkdf2Options { Iterations = 1_000 };
        var hasher = new Pbkdf2PasswordHasher(options);
        options.Iterations = 5;

        Assert.StartsWith("PBKDF2$SHA256$1000$", hasher.Hash("pwd"), StringComparison.Ordinal);
    }

    [Fact]
    public void Hash_NullPassword_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => CreateFast().Hash(null!));
    }

    [Fact]
    public void Verify_NullParameters_Throws()
    {
        var hasher = CreateFast();
        string hash = hasher.Hash("secret");

        Assert.Throws<ArgumentNullException>(() => hasher.Verify(null!, hash));
        Assert.Throws<ArgumentNullException>(() => hasher.Verify("secret", null!));
    }

    [Theory]
    [InlineData(Pbkdf2HashAlgorithm.Sha256, "SHA256")]
    [InlineData(Pbkdf2HashAlgorithm.Sha384, "SHA384")]
    [InlineData(Pbkdf2HashAlgorithm.Sha512, "SHA512")]
    public void HashAndVerify_AllAlgorithms_Succeed(Pbkdf2HashAlgorithm algorithm, string name)
    {
        var hasher = CreateFast(o => o.Algorithm = algorithm);
        string hash = hasher.Hash("pässwörd 🔒");

        Assert.StartsWith($"PBKDF2${name}$1000$", hash, StringComparison.Ordinal);
        Assert.True(hasher.Verify("pässwörd 🔒", hash));
        Assert.False(hasher.Verify("password", hash));
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        var hasher = CreateFast();
        Assert.NotEqual(hasher.Hash("pwd"), hasher.Hash("pwd"));
    }

    [Fact]
    public void Hash_Format_IsExpected()
    {
        string hash = CreateFast(o => { o.SaltSize = 8; o.KeySize = 16; }).Hash("pwd");
        string[] parts = hash.Split('$');

        Assert.Equal(5, parts.Length);
        Assert.Equal("PBKDF2", parts[0]);
        Assert.Equal("SHA256", parts[1]);
        Assert.Equal("1000", parts[2]);
        Assert.Equal(8, Convert.FromBase64String(parts[3]).Length);
        Assert.Equal(16, Convert.FromBase64String(parts[4]).Length);
    }

    [Fact]
    public void Verify_HashCreatedByVersion2_StillSucceeds()
    {
        // 2.x format: PBKDF2$SHA256$<iterations>$salt$key with UTF-8 password bytes.
        string legacy = CreateLegacyV2Hash("password", iterations: 1_000);
        var hasher = CreateFast();

        Assert.True(hasher.Verify("password", legacy));
        Assert.False(hasher.Verify("other", legacy));
    }

    [Theory]
    [InlineData("")]
    [InlineData("bad-hash")]
    [InlineData("PBKDF2$SHA256$1000$AAAA")]
    [InlineData("PBKDF2$SHA256$1000$AAAA$AAAA$AAAA")]
    [InlineData("BCRYPT$SHA256$1000$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAA==")]
    [InlineData("PBKDF2$MD5$1000$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAA==")]
    [InlineData("PBKDF2$SHA256$abc$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAA==")]
    [InlineData("PBKDF2$SHA256$-5$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAA==")]
    [InlineData("PBKDF2$SHA256$0$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAA==")]
    [InlineData("PBKDF2$SHA256$1000$not base64!$AAAAAAAAAAAAAAAAAAAAAA==")]
    [InlineData("PBKDF2$SHA256$1000$$AAAAAAAAAAAAAAAAAAAAAA==")]
    public void Verify_MalformedHash_ReturnsFalse(string hash)
    {
        Assert.False(CreateFast().Verify("password", hash));
    }

    [Fact]
    public void Verify_HugeIterationCount_IsRejectedImmediately()
    {
        // A tampered hash must not be able to make the server run int.MaxValue PBKDF2 iterations.
        string tampered = $"PBKDF2$SHA256${int.MaxValue}$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAA==";
        var stopwatch = Stopwatch.StartNew();

        Assert.False(CreateFast().Verify("password", tampered));
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Verify_HugeHashString_ReturnsFalse()
    {
        Assert.False(CreateFast().Verify("password", new string('A', 1_000_000)));
    }

    [Fact]
    public void NeedsRehash_CurrentParameters_ReturnsFalse()
    {
        var hasher = CreateFast();
        Assert.False(hasher.NeedsRehash(hasher.Hash("pwd")));
    }

    [Fact]
    public void NeedsRehash_WeakerParameters_ReturnsTrue()
    {
        var current = CreateFast(o => o.Iterations = 2_000);

        Assert.True(current.NeedsRehash(CreateFast(o => o.Iterations = 1_000).Hash("pwd")));
        Assert.True(current.NeedsRehash(CreateFast(o => { o.Iterations = 2_000; o.SaltSize = 8; }).Hash("pwd")));
        Assert.True(current.NeedsRehash(CreateFast(o => { o.Iterations = 2_000; o.KeySize = 16; }).Hash("pwd")));
        Assert.True(current.NeedsRehash(CreateFast(o => { o.Iterations = 2_000; o.Algorithm = Pbkdf2HashAlgorithm.Sha512; }).Hash("pwd")));
    }

    [Fact]
    public void NeedsRehash_StrongerParameters_ReturnsFalse()
    {
        var current = CreateFast();
        Assert.False(current.NeedsRehash(CreateFast(o => o.Iterations = 2_000).Hash("pwd")));
    }

    [Fact]
    public void NeedsRehash_Version2DefaultHash_ReturnsTrue()
    {
        // 2.x used 210,000 iterations; the 3.x default is 600,000.
        string legacy = CreateLegacyV2Hash("pwd", iterations: 1);
        Assert.True(new Pbkdf2PasswordHasher().NeedsRehash(legacy.Replace("$1$", "$210000$")));
    }

    [Fact]
    public void NeedsRehash_MalformedHash_ReturnsTrue()
    {
        Assert.True(CreateFast().NeedsRehash("bad-hash"));
    }

    [Fact]
    public void NeedsRehash_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => CreateFast().NeedsRehash(null!));
    }

    private static string CreateLegacyV2Hash(string password, int iterations)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] key = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA256, 32);
        return string.Join('$', "PBKDF2", "SHA256", iterations.ToString(), Convert.ToBase64String(salt), Convert.ToBase64String(key));
    }
}
