using SpectraUtils.Abstract;
using SpectraUtils.Concerete;
using SpectraUtils.Concrete;

namespace SpectraUtils;

/// <summary>
/// Provides utility methods for name editing, password management and security services.
/// This class can be registered as a singleton in dependency injection containers.
/// </summary>
public sealed class SpectraUtil : ISpectraUtil
{
    private readonly Lazy<INameEdit> _nameEdit;
    private readonly Lazy<IPasswordHelper> _passwordHelper;

    // New security services
    private readonly Lazy<IPasswordHasher> _passwordHasher;
    private readonly Lazy<ISecureTokenGenerator> _tokenGenerator;
    private readonly Lazy<IOneTimeCodeGenerator> _oneTimeCodeGenerator;

    /// <summary>
    /// Gets the name editing utility for correcting and formatting names.
    /// </summary>
    public INameEdit NameEdit => _nameEdit.Value;

    /// <summary>
    /// Gets the password helper utility for creating and hashing passwords.
    /// </summary>
    public IPasswordHelper PasswordHelper => _passwordHelper.Value;

    /// <summary>
    /// Gets the low-level password hasher for hashing/verifying persisted passwords (PBKDF2).
    /// </summary>
    public IPasswordHasher PasswordHasher => _passwordHasher.Value;

    /// <summary>
    /// Gets the secure token generator for creating random Base64Url tokens.
    /// </summary>
    public ISecureTokenGenerator TokenGenerator => _tokenGenerator.Value;

    /// <summary>
    /// Gets the one-time code generator for numeric OTPs.
    /// </summary>
    public IOneTimeCodeGenerator OneTimeCodeGenerator => _oneTimeCodeGenerator.Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpectraUtil"/> class with default implementations.
    /// </summary>
    public SpectraUtil()
        : this(new Lazy<INameEdit>(() => new NameEdit()), new Lazy<IPasswordHelper>(() => new PasswordHelper()),
               new Lazy<IPasswordHasher>(() => new Pbkdf2PasswordHasher()),
               new Lazy<ISecureTokenGenerator>(() => new SecureTokenGenerator()),
               new Lazy<IOneTimeCodeGenerator>(() => new OneTimeCodeGenerator()))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpectraUtil"/> class with custom implementations.
    /// Useful for dependency injection scenarios.
    /// </summary>
    public SpectraUtil(INameEdit nameEdit, IPasswordHelper passwordHelper,
                       IPasswordHasher passwordHasher, ISecureTokenGenerator tokenGenerator,
                       IOneTimeCodeGenerator oneTimeCodeGenerator)
        : this(new Lazy<INameEdit>(() => nameEdit), new Lazy<IPasswordHelper>(() => passwordHelper),
               new Lazy<IPasswordHasher>(() => passwordHasher), new Lazy<ISecureTokenGenerator>(() => tokenGenerator),
               new Lazy<IOneTimeCodeGenerator>(() => oneTimeCodeGenerator))
    {
        ArgumentNullException.ThrowIfNull(nameEdit);
        ArgumentNullException.ThrowIfNull(passwordHelper);
        ArgumentNullException.ThrowIfNull(passwordHasher);
        ArgumentNullException.ThrowIfNull(tokenGenerator);
        ArgumentNullException.ThrowIfNull(oneTimeCodeGenerator);
    }

    private SpectraUtil(Lazy<INameEdit> nameEdit, Lazy<IPasswordHelper> passwordHelper,
                        Lazy<IPasswordHasher> passwordHasher, Lazy<ISecureTokenGenerator> tokenGenerator,
                        Lazy<IOneTimeCodeGenerator> oneTimeCodeGenerator)
    {
        _nameEdit = nameEdit;
        _passwordHelper = passwordHelper;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _oneTimeCodeGenerator = oneTimeCodeGenerator;
    }
}
